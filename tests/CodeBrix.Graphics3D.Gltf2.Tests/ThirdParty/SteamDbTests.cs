using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeBrix.Graphics3D.Gltf2.Geometry;
using CodeBrix.Graphics3D.Gltf2.Geometry.Parametric;
using CodeBrix.Graphics3D.Gltf2.Materials;
using CodeBrix.Graphics3D.Gltf2.Memory;
using CodeBrix.Graphics3D.Gltf2.Scenes;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.ThirdParty; //was previously: SharpGLTF.ThirdParty;

public class SteamDbTests
{
    [Fact]
    public void TestWriteAlternateTexturePath()
    {
        // create material
        var material = new MaterialBuilder()
            .WithDoubleSide(true)
            .WithMetallicRoughnessShader();

        var imgBuilder = ImageBuilder.From(new MemoryImage(ResourceInfo.From("PolyHaven/food_apple_01_diff.png")));
        imgBuilder.AlternateWriteFileName = "alternateTextureName.*";

        material.WithBaseColor(imgBuilder);

        var meshBuilder = new MeshBuilder<MaterialBuilder, Geometry.VertexTypes.VertexPosition, Geometry.VertexTypes.VertexEmpty, Geometry.VertexTypes.VertexEmpty>();
        meshBuilder.AddCube(material, System.Numerics.Matrix4x4.Identity);

        var sceneBuilder = new SceneBuilder();
        sceneBuilder.AddRigidMesh(meshBuilder, System.Numerics.Matrix4x4.Identity);

        var model = sceneBuilder.ToGltf2();

        model.LogicalImages[0].AlternateWriteFileName.Should().Be("alternateTextureName.*");

        model = model.DeepClone();

        model.LogicalImages[0].AlternateWriteFileName.Should().Be("alternateTextureName.*");

        var dstPath = AttachmentInfo
            .From("model.gltf")
            .WriteObject(f => model.Save(f));

        var altPath = System.IO.Path.Combine(dstPath.Directory.FullName, "alternateTextureName.png");

        System.IO.File.Exists(altPath).Should().BeTrue();
    }
}
