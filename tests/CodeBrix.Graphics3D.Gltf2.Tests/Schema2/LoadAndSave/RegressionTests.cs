using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Graphics3D.Gltf2.Validation;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Schema2.LoadAndSave; //was previously: SharpGLTF.Schema2.LoadAndSave;

public class RegressionTests
{
    [Fact]
    public void LoadWithRelativeAndAbsolutePath()
    {
        // store current directory

        var cdir = Environment.CurrentDirectory;

        var modelPath = ResourceInfo.From("SpecialCases/RelativePaths.gltf");

        // absolute path

        var model1 = ModelRoot.Load(modelPath, Validation.ValidationMode.TryFix);
        model1.Should().NotBeNull();
        model1.LogicalImages.Should().HaveCount(4);

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join("   ", ModelRoot.GetSatellitePaths(modelPath)));

        // local path

        Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(modelPath);
        var modelFile = System.IO.Path.GetFileName(modelPath);

        var model2 = ModelRoot.Load(modelFile, Validation.ValidationMode.TryFix);
        model2.Should().NotBeNull();
        model2.LogicalImages.Should().HaveCount(4);

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join("   ", ModelRoot.GetSatellitePaths(modelPath)));

        // relative path:

        modelFile = System.IO.Path.Combine(System.IO.Path.GetFileName(Environment.CurrentDirectory), modelFile);
        Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Environment.CurrentDirectory);

        var model3 = ModelRoot.Load(modelFile, Validation.ValidationMode.TryFix);
        model3.Should().NotBeNull();
        model3.LogicalImages.Should().HaveCount(4);

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join("   ", ModelRoot.GetSatellitePaths(modelPath)));

        // restore current directory

        Environment.CurrentDirectory = cdir;
    }

    [Fact]
    public void LoadSuzanneTest()
    {
        var path1 = TestFiles.GetSampleModelsPaths().First(item => item.EndsWith("Suzanne.gltf"));

        var suzanne1 = ModelRoot.Load(path1, ValidationMode.TryFix);
        var suzanne1Mem = suzanne1.LogicalBuffers.Sum(item => item.Content.Length);

        // direct save-load

        var path2 = suzanne1
            .AttachToCurrentTest("suzanne2.glb");

        var suzanne2 = ModelRoot.Load(path2);
        var suzanne2Mem = suzanne1.LogicalBuffers.Sum(item => item.Content.Length);

        suzanne2Mem.Should().Be(suzanne1Mem);
        suzanne2.LogicalMeshes.Should().HaveCount(suzanne1.LogicalMeshes.Count);

        // scenebuilder roundtrip

        var path3 = Scenes.SceneBuilder
            .CreateFrom(suzanne1.DefaultScene)
            .ToGltf2()
            .AttachToCurrentTest("suzanne.glb");

        var suzanne3 = ModelRoot.Load(path3);
        var suzanne3Mem = suzanne1.LogicalBuffers.Sum(item => item.Content.Length);

        suzanne3Mem.Should().Be(suzanne1Mem);
        suzanne3.LogicalMeshes.Should().HaveCount(suzanne1.LogicalMeshes.Count);
    }

    [Fact]
    public void LoadBinaryWithLimitedStream()
    {
        var path1 = TestFiles.GetSampleModelsPaths().First(item => item.EndsWith("CesiumMan.glb"));

        var bytes = System.IO.File.ReadAllBytes(path1);
        using(var ls = new ReadOnlyTestStream(bytes))
        {
            var model = ModelRoot.ReadGLB(ls);
            model.Should().NotBeNull();
        }
    }

    [Fact]
    public void LoadMinMaxBoundsOnByteAccessor()
    {
        // https://github.com/vpenades/SharpGLTF/issues/231
        // https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#_accessor_max

        var gltf = ModelRoot.Load(ResourceInfo.From("cube-integer-min-max-bounds.gltf"));
        gltf.Should().NotBeNull();
    }

    [Fact]
    public void SaveToPathWithExtraDots()
    {
        // https://github.com/vpenades/SharpGLTF/issues/217

        var path1 = TestFiles.GetSampleModelsPaths().First(item => item.EndsWith("BoxTextured.gltf"));

        var model = ModelRoot.Load(path1);

        var attachmentPath = AttachmentInfo.From("BoxTextured.xyz.gltf").WriteObject(f => model.Save(f));

        var texturePath = attachmentPath.Directory.GetFiles("BoxTextured.xyz.png").FirstOrDefault();

        texturePath.Name.Should().Be("BoxTextured.xyz.png");
    }
}
