using System;
using System.IO;
using System.Linq;
using System.Numerics;
using CodeBrix.Graphics3D.Gltf2.Geometry;
using CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes;
using CodeBrix.Graphics3D.Gltf2.Materials;
using CodeBrix.Graphics3D.Gltf2.Memory;
using CodeBrix.Graphics3D.Gltf2.Scenes;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using CodeBrix.Graphics3D.Gltf2.Transforms;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.ThirdParty; //was previously: SharpGLTF.ThirdParty;

public class MeltyPlayerTests
{
    /// <summary>
    /// Regression: The only way to write models that exceed 2gb is to disable MergeBuffers.
    /// </summary>
    /// <remarks>
    /// <see href="https://github.com/vpenades/SharpGLTF/issues/165">#165</see>
    /// </remarks>
    [Fact]
    public void CreateSceneWithMultipleBuffers()
    {

        // create model
        var model = ModelRoot.CreateModel();

        // create scene
        var scene = model.DefaultScene = model.UseScene("Default");

        // create material
        var material = model.CreateMaterial("Default")
            .WithDefault(new Vector4(0, 1, 0, 1))
            .WithDoubleSide(true);

        void addNodeMesh(string name, Matrix4x4 xform)
        {
            // create node
            var rnode = scene.CreateNode(name);

            // create mesh
            var rmesh = rnode.Mesh = model.CreateMesh($"{name} Triangle Mesh");

            // create the vertex positions
            var positions = new[]
            {
                Vector3.Transform(new Vector3(0, 10, 0), xform),
                Vector3.Transform(new Vector3(-10, -10, 0), xform),
                Vector3.Transform(new Vector3(10, -10, 0), xform),
            };

            // create an index buffer and fill it
            var indices = new[] { 0, 1, 2 };

            // create mesh primitive
            var primitive = rmesh.CreatePrimitive()
                .WithVertexAccessor("POSITION", positions)
                .WithIndicesAccessor(PrimitiveType.TRIANGLES, indices)
                .WithMaterial(material);
        }

        addNodeMesh("Node1", Matrix4x4.Identity);
        addNodeMesh("Node2", Matrix4x4.CreateTranslation(20, 0, 0));

        var ws = new WriteSettings();
        ws.MergeBuffers = false;
        ws.JsonIndented = true;

        var resultPath0 = AttachmentInfo.From("result0.gltf").WriteObject(f => model.Save(f, ws));

        var satellites = ModelRoot.GetSatellitePaths(resultPath0.FullName);

        satellites.Length.Should().Be(4);
    }

    [Fact]
    public void TestHugeSceneViaBuilders()
    {
        // create a scene

        var mesh = new MeshBuilder<VertexPositionNormal, VertexColor2Texture2, VertexJoints8>();

        var gridSize = 1000;

        // generate texture
        var diffuseImageSize = 2 * gridSize;
        var occlusionImageSize = gridSize;

        var diffuseImage = _CreateEmptyImage(diffuseImageSize);
        var occlusionImage = _CreateEmptyImage(occlusionImageSize);

        #pragma warning disable CS0618 // Type or member is obsolete
        var material = MaterialBuilder.CreateDefault().WithSpecularGlossinessShader();
        material.UseChannel(KnownChannel.Diffuse)
                .UseTexture()
                .WithPrimaryImage(ImageBuilder.From(diffuseImage))
                .WithCoordinateSet(0);
        #pragma warning restore CS0618 // Type or member is obsolete
        material.UseChannel(KnownChannel.Occlusion)
                .UseTexture()
                .WithPrimaryImage(ImageBuilder.From(occlusionImage))
                .WithCoordinateSet(0);

        // generate heightmap

        for (var y = 0; y < gridSize; ++y) {
            for (var x = 0; x < gridSize; ++x) {
                var vertices = new (float X, float Y)[]
                {
                    (x, y),
                    (x + 1, y),
                    (x, y + 1),
                    (x + 1, y + 1)
                }.Select(pos => VertexBuilder<VertexPositionNormal, VertexColor2Texture1, VertexJoints8>
                         .Create(new Vector3(pos.X, pos.Y, 0), new Vector3(x, y, 0))
                         .WithMaterial(new Vector4(pos.X / gridSize, pos.Y / gridSize, 0, 1),
                                       new Vector4(0, pos.X / gridSize, pos.Y / gridSize, 1),
                                       new Vector2(pos.X / gridSize, pos.Y / gridSize),
                                       new Vector2(pos.X / gridSize, pos.Y / gridSize))
                         .WithSkinning(SparseWeight8.Create((0, 1))))
                 .ToArray();

                mesh.UsePrimitive(material).AddTriangle(vertices[0], vertices[1], vertices[2]);
                mesh.UsePrimitive(material).AddTriangle(vertices[1], vertices[2], vertices[3]);
            }
        }

        var scene = new SceneBuilder();
        scene.AddSkinnedMesh(mesh, Matrix4x4.Identity, new NodeBuilder());

        // convert to gltf

        var gltf = scene.ToGltf2();

        gltf.LogicalMeshes.Should().HaveCount(1);

        var outFiles = new[]
        {
            "huge_scene.glb",
            "huge_scene.gltf",
            "huge_scene.obj",
        };

        foreach (var outFile in outFiles) {
            gltf.AttachToCurrentTest(outFile);

            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForFullGCComplete();
        }
    }

    private static MemoryImage _CreateEmptyImage(int diffuseImageSize)
    {

        using var img = new CodeBrix.Imaging.Image<CodeBrix.Imaging.PixelFormats.Argb32>(diffuseImageSize, diffuseImageSize);

        using var mem = new MemoryStream();
        CodeBrix.Imaging.ImageExtensions.SaveAsPng(img, mem);

        mem.TryGetBuffer(out var bytes);
        return new MemoryImage(bytes);

    }
}
