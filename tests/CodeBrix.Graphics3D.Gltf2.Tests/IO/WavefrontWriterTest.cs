using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Graphics3D.Gltf2.Geometry;
using CodeBrix.Graphics3D.Gltf2.Geometry.Parametric;
using CodeBrix.Graphics3D.Gltf2.Scenes;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.IO; //was previously: SharpGLTF.IO;

public class WavefrontWriterTest
{
    [Fact]
    public void WriteWavefrontFileTest()
    {
        var material = Materials.MaterialBuilder.CreateDefault();
        material.WithBaseColor(Memory.MemoryImage.DefaultPngImage);

        var mesh = new MeshBuilder<Geometry.VertexTypes.VertexPositionNormal, Geometry.VertexTypes.VertexEmpty, Geometry.VertexTypes.VertexEmpty>("SphereMesh");
        mesh.AddSphere(material, 50, System.Numerics.Matrix4x4.Identity);

        var outPath = mesh.AttachToCurrentTest("result.obj");

        var pngPath = System.IO.Path.ChangeExtension(outPath, ".png");

        System.IO.File.Exists(pngPath).Should().BeTrue();
    }
}
