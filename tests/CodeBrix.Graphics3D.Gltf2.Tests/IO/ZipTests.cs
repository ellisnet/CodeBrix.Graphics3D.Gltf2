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

public class ZipTests
{
    [Fact]
    public void ZipRoundtripTest()
    {
        // create a model

        var mesh = new MeshBuilder<Geometry.VertexTypes.VertexPositionNormal, Geometry.VertexTypes.VertexEmpty, Geometry.VertexTypes.VertexEmpty>("SphereMesh");
        mesh.AddSphere(Materials.MaterialBuilder.CreateDefault(), 50, System.Numerics.Matrix4x4.Identity);

        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh, System.Numerics.Matrix4x4.Identity).WithName("Sphere");

        Schema2.ModelRoot model = scene.ToGltf2();

        model.LogicalMeshes[0].Name.Should().Be("SphereMesh");
        model.LogicalNodes[0].Name.Should().Be("Sphere");

        model = _ZipRoundtrip(model);

        model.LogicalMeshes[0].Name.Should().Be("SphereMesh");
        model.LogicalNodes[0].Name.Should().Be("Sphere");
    }

    private static Schema2.ModelRoot _ZipRoundtrip(Schema2.ModelRoot model)
    {
        byte[] raw;

        // write to zip into memory:

        using (var memory = new System.IO.MemoryStream())
        {
            using (var zipWriter = new ZipWriter(memory))
            {
                zipWriter.AddModel("model.gltf", model);
            }

            raw = memory.ToArray();
        }

        // read the model back:

        using (var memory = new System.IO.MemoryStream(raw, false))
        {
            using (var zipReader = new ZipReader(memory))
            {
                return zipReader.LoadModel("model.gltf");
            }
        }
    }
}
