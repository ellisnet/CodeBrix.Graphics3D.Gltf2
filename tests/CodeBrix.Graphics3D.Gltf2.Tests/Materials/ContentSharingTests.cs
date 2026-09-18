using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using CodeBrix.Graphics3D.Gltf2.Geometry.Parametric;
using CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes;
using CodeBrix.Graphics3D.Gltf2.Scenes;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Materials; //was previously: SharpGLTF.Materials;

public class ContentSharingTests
{

    [Fact]
    public void TestMaterialBuilderEquality()
    {
        var tex1Bytes = System.IO.File.ReadAllBytes(ResourceInfo.From("PolyHaven/food_apple_01_diff.png"));

        // create a material using our shared texture
        var material1 = new MaterialBuilder()
            .WithUnlitShader()
            .WithBaseColor(tex1Bytes);

        var material2 = material1.Clone();

        MaterialBuilder.AreEqualByContent(material1, material2).Should().BeTrue();

        var extras = new System.Text.Json.Nodes.JsonObject();
        extras["hello"] = 1;

        material2
            .GetChannel(KnownChannel.BaseColor)
            .Texture
            .PrimaryImage
            .Extras = extras;

        var material3 = material2.Clone();

        MaterialBuilder.AreEqualByContent(material1, material2).Should().BeFalse();
        MaterialBuilder.AreEqualByContent(material2, material3).Should().BeTrue();

        var dict = material3.GetChannel(KnownChannel.BaseColor)
            .Texture
            .PrimaryImage
            .Extras.Deserialize<Dictionary<string,int>>();

        dict.Should().HaveCount(1);
        dict["hello"].Should().Be(1);
    }

    [Fact]
    public void WriteTwoModelsWithSharedTexture()
    {

        var tex1Bytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(ResourceInfo.From("PolyHaven/food_apple_01_diff.png")));
        var tex2Bytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(ResourceInfo.From("PolyHaven/food_apple_01_nor_gl.jpg")));

        var tex1 = AttachmentInfo
            .From("shared-apple.png")
            .WriteAllBytes(tex1Bytes)
            .FullName;

        var tex2 = AttachmentInfo
            .From("subdir\\shared-in-dir-apple-normal.jpg")
            .WriteAllBytes(tex2Bytes)
            .FullName;

        // create a material using our shared texture
        var material1 = new MaterialBuilder()
            .WithUnlitShader()
            .WithBaseColor(tex1);

        // create a material using our shared texture
        var material2 = new MaterialBuilder()
            .WithUnlitShader()
            .WithBaseColor(tex2);

        // create a simple cube mesh
        var mesh1 = new Cube<MaterialBuilder>(material1).ToMesh(Matrix4x4.Identity);
        var mesh2 = new Cube<MaterialBuilder>(material2).ToMesh(Matrix4x4.Identity);
        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh1, Matrix4x4.CreateTranslation(-2, 0, 0));
        scene.AddRigidMesh(mesh2, Matrix4x4.CreateTranslation(2, 0, 0));

        var gltf = scene.ToGltf2();

        // define the texture sharing hook; this is a pretty naive approach, but it's good
        // enough to demonstrate how it works.

        string imageSharingHook(WriteContext ctx, string uri, Memory.MemoryImage image)
        {
            new string[] { tex1, tex2 }.Should().Contain(image.SourcePath);

            if (File.Exists(image.SourcePath))
            {
                // image.SourcePath is an absolute path, we must make it relative to ctx.CurrentDirectory

                var currDir = ctx.CurrentDirectory.FullName + Path.DirectorySeparatorChar;

                // if the shared texture can be reached by the model in its directory, reuse the texture.
                if (image.SourcePath.StartsWith(currDir, StringComparison.OrdinalIgnoreCase))
                {
                    // we've found the shared texture!, return the uri relative to the model:
                    return image.SourcePath.Substring(currDir.Length);
                }

                // TODO: Here we could also try to find a texture equivalent to MemoryImage in the
                // CurrentDirectory even if it has a different name, to minimize texture duplication.
            }

            // we were unable to reuse the shared texture,
            // default to write our own texture.

            image.SaveToFile(Path.Combine(ctx.CurrentDirectory.FullName, uri));

            return uri;
        }

        var settings = new WriteSettings();
        settings.ImageWriting = ResourceWriteMode.SatelliteFile;
        settings.ImageWriteCallback = imageSharingHook;

        // save the model several times:

        var path1 = gltf.AttachToCurrentTest("model1.glb", settings);
        var path2 = gltf.AttachToCurrentTest("model2.glb", settings);
        var path3 = gltf.AttachToCurrentTest("model3.gltf", settings);

        var satellites1 = ModelRoot.GetSatellitePaths(path1);
        var satellites2 = ModelRoot.GetSatellitePaths(path2);
        var satellites3 = ModelRoot.GetSatellitePaths(path3);

        satellites1.Should().Contain("shared-apple.png");
        satellites1.Should().Contain("subdir/shared-in-dir-apple-normal.jpg");

        satellites2.Should().Contain("shared-apple.png");
        satellites2.Should().Contain("subdir/shared-in-dir-apple-normal.jpg");

        satellites3.Should().Contain("shared-apple.png");
        satellites3.Should().Contain("subdir/shared-in-dir-apple-normal.jpg");
    }

}
