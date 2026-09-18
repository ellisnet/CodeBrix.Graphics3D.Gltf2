using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Schema2.LoadAndSave; //was previously: SharpGLTF.Schema2.LoadAndSave;

/// <summary>
/// Test cases for models found in <see href="https://github.com/KhronosGroup/glTF-Sample-Models"/> and more....
/// </summary>
public class LoadSampleTests
{
    #region setup

    public LoadSampleTests()
    {
        // TestFiles.DownloadReferenceModels();
    }

    #endregion

    #region helpers

    private static ModelRoot _LoadModel(string f, bool tryFix = false)
    {
        var settings = tryFix
            ? Validation.ValidationMode.TryFix
            : Validation.ValidationMode.Strict;

        ModelRoot model = null;

        var perf = System.Diagnostics.Stopwatch.StartNew();

        model = ModelRoot.Load(f, settings);

        model.Should().NotBeNull();

        if (model == null) return null;

        var perf_load = perf.ElapsedMilliseconds;

        // do a model clone and compare it
        _AssertAreEqual(model, model.DeepClone());

        var perf_clone = perf.ElapsedMilliseconds;

        if (!f.Contains("Iridescence")) // the iridescence sample models declares using IOR but it's not actually used
        {
            var unsupportedExtensions = new[] { "MSFT_lod", "EXT_lights_image_based" };

            // check extensions used
            if (unsupportedExtensions.All(uex => !model.ExtensionsUsed.Contains(uex)))
            {
                var detectedExtensions = model.GatherUsedAndRequiredExtensions().Select(item => item.ext).ToArray();
                detectedExtensions.Should().BeEquivalentTo(model.ExtensionsUsed);
            }
        }

        // Save models
        model.AttachToCurrentTest(System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(f), ".obj"));
        var perf_wavefront = perf.ElapsedMilliseconds;

        model.AttachToCurrentTest(System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(f), ".glb"));
        var perf_glb = perf.ElapsedMilliseconds;

        TestContext.Current.TestOutputHelper?.WriteLine($"processed {System.IO.Path.GetFileName(f)} - Load:{perf_load}ms Clone:{perf_clone}ms S.obj:{perf_wavefront}ms S.glb:{perf_glb}ms");

        return model;
    }

    private static void _AssertAreEqual(ModelRoot a, ModelRoot b)
    {
        var aa = a.GetLogicalChildrenFlattened().ToList();
        var bb = b.GetLogicalChildrenFlattened().ToList();

        bb.Should().HaveCount(aa.Count);

        aa.Select(item => item.GetType()).Should().Equal(bb.Select(item => item.GetType()));
    }

    #endregion

    [Theory]
    [InlineData("/glTF/")]
    // [TestCase("\\glTF-Draco\\")] // Not supported
    [InlineData("/glTF-IBL/")]
    [InlineData("/glTF-Binary/")]
    [InlineData("/glTF-Embedded/")]
    [InlineData("/glTF-Quantized/")]
    // [TestCase("\\glTF-Meshopt\\")] // not supported
    public void LoadModelsFromKhronosSamples(string section)
    {

        #if !DEBUG
        Assert.Multiple( () => {
        #endif

            foreach (var f in TestFiles.GetSampleModelsPaths())
            {
                if (f.Contains("SuzanneMorphSparse")) continue; // temporarily skipping due to empty BufferView issue
                if (f.Contains("SunglassesKhronos")) continue; // KHR_materials_specular is declared but not used

            if (!f.Replace('\\', '/').Contains(section)) continue;

                _LoadModel(f);
            }

        #if !DEBUG
        } );
        #endif
    }

    [Fact]
    public void LoadModelsFromBabylonJs()
    {

        foreach (var f in TestFiles.GetBabylonJSModelsPaths())
        {
            _LoadModel(f, true);
        }
    }

    [Theory]
    [InlineData("TeapotsGalore.gltf")]
    public void LoadModelsWithGpuMeshInstancingExtension(string fileFilter)
    {

        var f = TestFiles.GetMeshIntancingModelPaths().FirstOrDefault(item => item.Contains(fileFilter));

        var model = _LoadModel(f, false);

        var ff = System.IO.Path.GetFileNameWithoutExtension(f);

        model.AttachToCurrentTest($"{ff}.loaded.glb");

        // perform roundtrip

        var roundtripDefault = model.DefaultScene
            .ToSceneBuilder()                                       // glTF to SceneBuilder
            .ToGltf2(Scenes.SceneBuilderSchema2Settings.Default);   // SceneBuilder to glTF

        var roundtripInstanced = model.DefaultScene
            .ToSceneBuilder()                                               // glTF to SceneBuilder
            .ToGltf2(Scenes.SceneBuilderSchema2Settings.WithGpuInstancing); // SceneBuilder to glTF

        // compare bounding spheres

        var modelBounds = Runtime.MeshDecoder.EvaluateBoundingBox(model.DefaultScene);
        var rtripDefBounds = Runtime.MeshDecoder.EvaluateBoundingBox(roundtripDefault.DefaultScene);
        var rtripGpuBounds = Runtime.MeshDecoder.EvaluateBoundingBox(roundtripInstanced.DefaultScene);

        rtripDefBounds.Should().Be(modelBounds);
        rtripGpuBounds.Should().Be(modelBounds);

        // save results

        roundtripDefault.AttachToCurrentTest($"{ff}.roundtrip.default.glb");
        roundtripInstanced.AttachToCurrentTest($"{ff}.roundtrip.instancing.glb");
    }

    [Theory]
    [InlineData("AnimationPointerUVs.glb")]
    [InlineData("IridescenceMetallicSpheres.gltf")]
    [InlineData("TextureTransformTest.gltf")]
    [InlineData("UnlitTest/glTF-Binary/UnlitTest.glb")]
    [InlineData("glTF-Quantized/AnimatedMorphCube.gltf")]
    public void LoadModelsWithExtensions(string filePath)
    {

        filePath = TestFiles
            .GetSampleModelsPaths()
            .FirstOrDefault(item => item.Replace('\\', '/').EndsWith(filePath));

        _LoadModel(filePath);
    }

    [Fact]
    public void LoadModelWithUnlitMaterial()
    {
        var f = TestFiles
            .GetSampleModelsPaths()
            .FirstOrDefault(item => item.Replace('\\', '/').EndsWith("UnlitTest/glTF-Binary/UnlitTest.glb"));

        var model = ModelRoot.Load(f);
        model.Should().NotBeNull();

        model.LogicalMaterials[0].Unlit.Should().BeTrue();

        // do a model roundtrip
        var modelBis = ModelRoot.ParseGLB(model.WriteGLB());
        modelBis.Should().NotBeNull();

        modelBis.LogicalMaterials[0].Unlit.Should().BeTrue();
    }

    [Theory]
    [InlineData("SimpleSparseAccessor.gltf")]
    // [TestCase("SuzanneMorphSparse.gltf")] requires supporting empty BufferView
    public void LoadModelWithSparseAccessor(string fileName)
    {
        var path = TestFiles
            .GetSampleModelsPaths()
            .FirstOrDefault(item => item.Contains(fileName));

        var model = ModelRoot.Load(path);
        model.Should().NotBeNull();

        var primitive = model.LogicalMeshes[0].Primitives[0];

        var accessor = primitive.GetVertexAccessor("POSITION");

        if (!accessor._TryGetMemoryAccessor(out var baseMem)) Assert.Fail("can't get underlaying data");

        var basePositions = baseMem.AsArrayOf<Vector3>();

        var positions = accessor.AsArrayOf<Vector3>();
    }

    [Fact]
    public void LoadModelWithMorphTargets()
    {
        var path = TestFiles
            .GetSampleModelsPaths()
            .FirstOrDefault(item => item.Contains("MorphPrimitivesTest.glb"));

        var model = ModelRoot.Load(path);
        model.Should().NotBeNull();

        var triangles = model.DefaultScene
            .EvaluateTriangles<Geometry.VertexTypes.VertexPosition, Geometry.VertexTypes.VertexEmpty>(null, null, 0)
            .ToArray();

        model.AttachToCurrentTest(System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(path), ".obj"));
        model.AttachToCurrentTest(System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(path), ".glb"));
    }

    [Theory]
    [InlineData("AnimationPointerUVs.glb")]
    [InlineData("RiggedFigure.glb")]
    [InlineData("RiggedSimple.glb")]
    [InlineData("BoxAnimated.glb")]
    [InlineData("AnimatedMorphCube.glb")]
    [InlineData("CesiumMan.glb")]
    [InlineData("Fox.glb")]
    public void LoadModelsWithAnimations(string path)
    {
        path = TestFiles
            .GetSampleModelsPaths()
            .FirstOrDefault(item => item.Contains(path));

        var model = ModelRoot.Load(path);
        model.Should().NotBeNull();

        path = System.IO.Path.GetFileNameWithoutExtension(path);
        model.AttachToCurrentTest(path + ".glb");

        var triangles = model.DefaultScene
            .EvaluateTriangles<Geometry.VertexTypes.VertexPosition, Geometry.VertexTypes.VertexEmpty>()
            .ToArray();

        var anim = model.LogicalAnimations[0];

        var duration = anim.Duration;

        for(int i=0; i < 10; ++i)
        {
            var t = duration * i / 10;
            int tt = (int)(t * 1000.0f);

            model.AttachToCurrentTest($"{path} at {tt}.obj", anim, t);
        }
    }

    [Fact]
    public void LoadAnimatedMorphCube()
    {
        var path = TestFiles
            .GetSampleModelsPaths()
            .FirstOrDefault(item => item.Contains("AnimatedMorphCube.glb"));

        var model = ModelRoot.Load(path);
        model.Should().NotBeNull();

        var anim = model.LogicalAnimations[0];
        var node = model.LogicalNodes[0];

        var acc_master = node.Mesh.Primitives[0].GetVertexAccessor("POSITION");
        var acc_morph0 = node.Mesh.Primitives[0].GetMorphTargetAccessors(0)["POSITION"];
        var acc_morph1 = node.Mesh.Primitives[0].GetMorphTargetAccessors(1)["POSITION"];

        var pos_master = acc_master.AsVector3Array();
        var pos_morph0 = acc_morph0.AsVector3Array();
        var pos_morph1 = acc_morph1.AsVector3Array();

        // pos_master

        var instance = Runtime.SceneTemplate
            .Create(model.DefaultScene)
            .CreateInstance();

        var pvrt = node.Mesh.Primitives[0].GetVertexColumns();

        for (float t = 0; t < 5; t+=0.25f)
        {
            instance.Armature.SetAnimationFrame(anim.LogicalIndex, t);

            var nodexform = instance.AsEnumerable().First().Transform;

            TestContext.Current.TestOutputHelper?.WriteLine($"Animation at {t}");

            var curves = node.GetCurveSamplers(anim);

            if (t < anim.Duration)
            {
                var mw = curves.GetMorphingSampler<float[]>()
                    .CreateCurveSampler()
                    .GetPoint(t);

                TestContext.Current.TestOutputHelper?.WriteLine($"    Morph Weights: {mw[0]} {mw[1]}");
            }

            var msw = curves.GetMorphingSampler<Transforms.SparseWeight8>()
                .CreateCurveSampler()
                .GetPoint(t);

            TestContext.Current.TestOutputHelper?.WriteLine($"    Morph Sparse : {msw.Weight0} {msw.Weight1}");

            var triangles = model.DefaultScene
                .EvaluateTriangles<Geometry.VertexTypes.VertexPosition, Geometry.VertexTypes.VertexEmpty>(null, anim, t)
                .ToList();

            var vertices = triangles
                .SelectMany(item => new[] { item.A.Position, item.B.Position, item.C.Position })
                .Distinct()
                .ToList();

            foreach (var v in vertices) TestContext.Current.TestOutputHelper?.WriteLine($"{v}");

            TestContext.Current.TestOutputHelper?.WriteLine(string.Empty);
        }

    }

    [Fact]
    public void LoadMultiUVTexture()
    {
        var path = TestFiles
            .GetSampleModelsPaths()
            .FirstOrDefault(item => item.Contains("TextureTransformMultiTest.glb"));

        var model = ModelRoot.Load(path);
        model.Should().NotBeNull();

        var materials = model.LogicalMaterials;

        var normalTest0Mat = materials.FirstOrDefault(item => item.Name == "NormalTest0Mat");
        var normalTest0Mat_normal = normalTest0Mat.FindChannel("Normal").Value;
        var normalTest0Mat_normal_xform = normalTest0Mat_normal.TextureTransform;

        normalTest0Mat_normal_xform.Should().NotBeNull();
    }

    [Fact]
    public void FindDependencyFiles()
    {

        foreach (var f in TestFiles.GetBabylonJSModelsPaths())
        {
            TestContext.Current.TestOutputHelper?.WriteLine(f);

            var dependencies = ModelRoot.GetSatellitePaths(f);

            foreach(var d in dependencies)
            {
                TestContext.Current.TestOutputHelper?.WriteLine($"    {d}");
            }

            TestContext.Current.TestOutputHelper?.WriteLine(string.Empty);
        }
    }
}
