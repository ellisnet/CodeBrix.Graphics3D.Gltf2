using System.Collections.Generic;
using System.Numerics;
using CodeBrix.Graphics3D.Gltf2.IO;
using SilverAssertions;
using Xunit;
using JSONEXTRAS = System.Text.Json.Nodes.JsonNode;
using VPOSNRM = CodeBrix.Graphics3D.Gltf2.Geometry.VertexBuilder<CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexPositionNormal, CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexEmpty, CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexEmpty>;

namespace CodeBrix.Graphics3D.Gltf2.Schema2.Authoring; //was previously: SharpGLTF.Schema2.Authoring;

public class BasicSceneCreationTests
{
    [Fact]
    public void CreateEmptyScene()
    {
        var root = ModelRoot.CreateModel();

        var scene = root.UseScene("Empty Scene");

        scene.Should().NotBeNull();
        root.DefaultScene.Name.Should().Be("Empty Scene");
    }

    [Fact]
    public void CreateSceneWithExtras()
    {
        var root = ModelRoot.CreateModel();
        var scene = root.UseScene("Empty Scene");

        var dict = new Dictionary<string, object>();

        dict["author"] = "me";
        dict["string_null"] = (string)null;

        dict["value1"] = 17;
        dict["array1"] = new List<int> { 1, 2, 3 };
        dict["array_empty"] = new List<int>();
        dict["dict1"] = new Dictionary<string, object>
        {
            ["A"] = 16,
            ["B"] = "delta",
            ["C"] = new List<int> { 4, 6, 7 },
            ["D"] = new Dictionary<string, int> { ["S"]= 1, ["T"] = 2 }
        };
        dict["dict2"] = new Dictionary<string, int> { ["2"] = 2, ["3"] = 3 };

        var extras = JSONEXTRAS.Parse(System.Text.Json.JsonSerializer.Serialize(dict));

        root.Extras = extras;

        var bytes = root.WriteGLB();
        var rootBis = ModelRoot.ParseGLB(bytes);

        var a = root.Extras;
        var b = rootBis.Extras;
        var json = rootBis.Extras.ToJsonString();
        var c = JSONEXTRAS.Parse(json);

        JsonContentTests.AreEqual(a, b).Should().BeTrue();
        JsonContentTests.AreEqual(a, extras).Should().BeTrue();
        JsonContentTests.AreEqual(b, extras).Should().BeTrue();
        JsonContentTests.AreEqual(c, extras).Should().BeTrue();

        // c.GetValue<int>("dict1","D","T").Should().Be(2);
    }

    [Fact]
    public void CreateSceneWithSolidTriangle()
    {

        // create model
        var model = ModelRoot.CreateModel();

        // create scene
        var scene = model.DefaultScene = model.UseScene("Default");

        // create node
        var rnode = scene.CreateNode("Triangle Node");

        // create material
        var material = model
            .CreateMaterial("Default")
            .WithDefault(new Vector4(0, 1, 0, 1))
            .WithDoubleSide(true);

        // create mesh
        var rmesh = rnode.Mesh = model.CreateMesh("Triangle Mesh");

        // create the vertex positions
        var positions = new[]
        {
            new Vector3(0, 10, 0),
            new Vector3(-10, -10, 0),
            new Vector3(10, -10, 0),
        };

        // create an index buffer and fill it
        var indices = new[] { 0, 1, 2 };

        // create mesh primitive
        var primitive = rmesh
            .CreatePrimitive()
            .WithVertexAccessor("POSITION", positions)
            .WithIndicesAccessor(PrimitiveType.TRIANGLES, indices)
            .WithMaterial(material);

        model.AttachToCurrentTest("result.glb");
        model.AttachToCurrentTest("result.gltf");
    }

    [Fact]
    public void CreateSceneWithTexturedTriangle()
    {

        // we'll use a CC0 PolyHaven texture from the test assets as the source texture
        var imagePath = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        // create a basic scene
        var model = ModelRoot.CreateModel();
        var scene = model.UseScene("Default");
        var rnode = scene.CreateNode("Triangle Node");
        var rmesh = rnode.Mesh = model.CreateMesh("Triangle Mesh");

        var material = model
            .CreateMaterial("Default")
            .WithPBRMetallicRoughness(Vector4.One, imagePath)
            .WithDoubleSide(true);

        // define the triangle positions
        var positions = new[]
        {
            new Vector3(0, 10, 0),
            new Vector3(-10, -10, 0),
            new Vector3(10, -10, 0)
        };

        // define the triangle UV coordinates
        var texCoords = new[]
        {
            new Vector2(0.5f, -0.8f),
            new Vector2(-0.5f, 1.2f),
            new Vector2(1.5f, 1.2f)
        };

        // create a mesh primitive and assgin the accessors and other properties
        var primitive = rmesh
            .CreatePrimitive()
            .WithVertexAccessor("POSITION", positions)
            .WithVertexAccessor("TEXCOORD_0", texCoords)
            .WithIndicesAutomatic(PrimitiveType.TRIANGLES)
            .WithMaterial(material);

        model.AttachToCurrentTest("result.glb");
        model.AttachToCurrentTest("result.obj");
        model.AttachToCurrentTest("result.gltf");
    }

    [Fact]
    public void CreateSceneWithInterleavedQuadMesh()
    {

        var vertices = new[]
        {
            VPOSNRM.Create(new Vector3(-10,  10, 0), Vector3.UnitZ),
            VPOSNRM.Create(new Vector3( 10,  10, 0), Vector3.UnitZ),
            VPOSNRM.Create(new Vector3( 10, -10, 0), Vector3.UnitZ),
            VPOSNRM.Create(new Vector3(-10, -10, 0), Vector3.UnitZ)
        };

        var model = ModelRoot.CreateModel();

        var mesh = model.CreateMesh("mesh1");

        mesh.CreatePrimitive()
            .WithMaterial(model.CreateMaterial("Default").WithDefault(Vector4.One).WithDoubleSide(true))
            .WithVertexAccessors(vertices)
            .WithIndicesAccessor(PrimitiveType.TRIANGLES, new int[] { 0, 1, 2, 0, 2, 3 });

        var scene = model.UseScene("Default");
        var rnode = scene.CreateNode("RootNode").WithMesh(mesh);

        model.AttachToCurrentTest("result.glb");
        model.AttachToCurrentTest("result.gltf");
    }

    [Fact]
    public void CreateSceneWithCamera()
    {

        var model = ModelRoot.CreateModel();

        model.UseScene(0)
            .CreateNode()
            .WithLocalTranslation(new Vector3(0, 3, 10))
            .WithPerspectiveCamera(null, 1, 0.1f);

        model.AttachToCurrentTest("result.glb");
        model.AttachToCurrentTest("result.gltf");
    }
}
