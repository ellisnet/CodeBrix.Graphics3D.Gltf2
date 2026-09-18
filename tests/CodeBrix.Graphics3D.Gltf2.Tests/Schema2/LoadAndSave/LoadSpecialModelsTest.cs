using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Schema2.LoadAndSave; //was previously: SharpGLTF.Schema2.LoadAndSave;

/// <summary>
/// Test cases for models with special characteristics: escaped URIs, generated tangents,
/// in-memory reading and writing, and JSON pre- and post-processing hooks.
/// </summary>
public class LoadSpecialModelsTest
{
    #region setup

    public LoadSpecialModelsTest()
    {
        // TestFiles.DownloadReferenceModels();
    }

    #endregion

    [Fact]
    public void LoadEscapedUriModel()
    {
        var model = ModelRoot.Load(ResourceInfo.From("white space.gltf"));
        model.Should().NotBeNull();

        model.AttachToCurrentTest("white space.glb");
    }

    // these models show normal mapping but lack tangents, which are expected to be
    // generated at runtime; These tests generate the tangents and check them against the baseline.
    [Theory]
    [InlineData("NormalTangentTest.glb")]
    [InlineData("NormalTangentMirrorTest.glb")]
    public void LoadGeneratedTangetsTest(string fileName)
    {
        var path = TestFiles.GetSampleModelsPaths().FirstOrDefault(item => item.EndsWith(fileName));

        var model = ModelRoot.Load(path);

        var mesh = model.DefaultScene
            .EvaluateTriangles<Geometry.VertexTypes.VertexPositionNormalTangent, Geometry.VertexTypes.VertexTexture1>()
            .ToMeshBuilder();

        var editableScene = new Scenes.SceneBuilder();
        editableScene.AddRigidMesh(mesh, Matrix4x4.Identity);

        model.AttachToCurrentTest("original.glb");
        editableScene.ToGltf2().AttachToCurrentTest("WithTangents.glb");
    }

    [Fact]
    public void LoadAndSaveToMemory()
    {
        var path = TestFiles.GetSampleModelsPaths().FirstOrDefault(item => item.EndsWith("CesiumMan.glb"));

        var model = ModelRoot.Load(path);
        // model.LogicalImages[0].TransferToSatelliteFile(); // TODO

        // we will use this dictionary as our in-memory model container.
        var dictionary = new Dictionary<string, ArraySegment<Byte>>();

        // write to dictionary
        var wcontext = WriteContext.CreateFromDictionary(dictionary);
        model.Save("cesiumman.gltf", wcontext);
        dictionary.ContainsKey("cesiumman.gltf").Should().BeTrue();
        dictionary.ContainsKey("cesiumman.bin").Should().BeTrue();

        // read back from dictionary
        var rcontext = ReadContext.CreateFromDictionary(dictionary);
        var model2 = ModelRoot.Load("cesiumman.gltf", rcontext);

        // TODO: verify

    }

    [Fact]
    public void LoadInvalidModelWithJsonFix()
    {
        // try to load an invalid gltf with an empty array

        var xpath = ResourceInfo.From("SpecialCases\\Invalid_EmptyArray.gltf");

        Assert.Throws<Validation.SchemaException>(() => ModelRoot.Load(xpath));

        // try to load an invalid gltf with an empty array, using a hook to fix the json before running the parser.

        var rsettings = new ReadSettings();
        rsettings.JsonPreprocessor = _RemoveEmptyArrayJsonProcessor;

        var model = ModelRoot.Load(xpath, rsettings);
        model.Should().NotBeNull();

        // save the model, using a hook to modify the json before writing it to the file.

        var wsettings = new WriteSettings();
        wsettings.JsonPostprocessor = json =>
        {
            json = json.Replace("CodeBrix test suite", "postprocessed json"); return json;
        };

        var path = model.AttachToCurrentTest("modified.gltf", wsettings);

        model = ModelRoot.Load(path);

        "postprocessed json".Should().Be(model.Asset.Generator);
    }

    private string _RemoveEmptyArrayJsonProcessor(string json)
    {
        var obj = System.Text.Json.Nodes.JsonNode.Parse(json).AsObject();

        obj.Remove("meshes"); // remove the empty "meshes" array.

        json = obj.ToJsonString();

        return json;
    }
}
