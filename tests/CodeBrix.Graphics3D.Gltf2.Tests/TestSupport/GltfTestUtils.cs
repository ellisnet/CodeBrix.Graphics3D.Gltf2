using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2; //was previously: SharpGLTF;

public static class GltfTestUtils
{
    public static void AttachToCurrentTest(this Scenes.SceneBuilder scene, string fileName)
    {
        var model = scene.ToGltf2();

        model.AttachToCurrentTest(fileName);
    }

    public static void AttachToCurrentTest(this ModelRoot model, string fileName, Animation animation, float time)
    {
        // wavefront does't like files paths with spaces because
        // some implementations would not find the material file
        fileName = fileName.Replace(" ", "_");

        AttachmentInfo
            .From(fileName)
            .WriteObject(f => model.SaveAsWavefront(f, animation, time));
    }

    public static string AttachToCurrentTest<TvG, TvM, TvS>(this Geometry.MeshBuilder<TvG, TvM, TvS> mesh, string fileName)
        where TvG : struct, Geometry.VertexTypes.IVertexGeometry
        where TvM : struct, Geometry.VertexTypes.IVertexMaterial
        where TvS : struct, Geometry.VertexTypes.IVertexSkinning
    {
        var gl2model = ModelRoot.CreateModel();

        var gl2mesh = gl2model.CreateMeshes(mesh)[0];

        var node = gl2model.UseScene(0).CreateNode();
        node.Mesh = gl2mesh;

        return gl2model.AttachToCurrentTest(fileName);
    }

    public static string AttachToCurrentTest(this ModelRoot model, string fileName, WriteSettings settings = null)
    {
        string validationPath = null;

        if (fileName.ToUpperInvariant().EndsWith(".GLB"))
        {
            validationPath = fileName = AttachmentInfo
                .From(fileName)
                .WriteObject(f => model.SaveGLB(f, settings))
                .FullName;
        }
        else if (fileName.ToUpperInvariant().EndsWith(".GLTF"))
        {
            if (settings == null) settings = new WriteSettings { JsonIndented = true };

            validationPath = fileName = AttachmentInfo
                .From(fileName)
                .WriteObject(f => model.Save(f, settings))
                .FullName;
        }
        else if (fileName.ToUpperInvariant().EndsWith(".OBJ"))
        {
            // skip exporting to obj if gpu instancing is there
            if (Node.Flatten(model.DefaultScene).Any(n => n.GetGpuInstancing() != null)) return fileName;

            fileName = fileName.Replace(" ", "_");

            fileName = AttachmentInfo
                .From(fileName)
                .WriteObject(f => model.SaveAsWavefront(f))
                .FullName;
        }

        // validator is unable to analyze models with gaussian splatting
        if (model.ExtensionsUsed.Contains("KHR_gaussian_splatting")) validationPath = null;

        if (validationPath != null)
        {
            var report = GltfValidator.ValidationReport.ValidateAsync(fileName, TestContext.Current.CancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();

            if (report == null) return fileName;

            if (report.Severity == GltfValidator.Severity.Error || report.Severity == GltfValidator.Severity.Warning)
            {
                TestContext.Current.TestOutputHelper?.WriteLine(report.ToString());
            }

            report.Severity.Should().NotBe(GltfValidator.Severity.Error, $"on file: {fileName}");
        }

        return fileName;
    }
}
