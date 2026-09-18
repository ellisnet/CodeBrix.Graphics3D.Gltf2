using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using CodeBrix.Graphics3D.Gltf2.Geometry;
using CodeBrix.Graphics3D.Gltf2.Materials;
using SilverAssertions;
using Xunit;
using VERTEX = CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexPositionNormal;

namespace CodeBrix.Graphics3D.Gltf2.ThirdParty; //was previously: SharpGLTF.ThirdParty;

public class EdMackeyTests
{
    [Fact]
    public void TestVertexEquality()
    {
        var material1 = new MaterialBuilder()
            .WithDoubleSide(true)
            .WithMetallicRoughnessShader()
            .WithChannelParam(KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4(1, 0, 0, 1));

        var mesh = new MeshBuilder<VERTEX>("mesh");

        var v0 = new VERTEX(15, 5, 0, 1, 0, 0);
        var v1 = new VERTEX(15, 5, 10, 1, 0, 0);
        var v2 = new VERTEX(15, 0, 10, 1, 0, 0);
        var v3 = new VERTEX(15, 0, 0, 1, 0, 0);

        var prim = mesh.UsePrimitive(material1);
        prim.AddTriangle(v0, v1, v3);
        prim.AddTriangle(v3, v1, v2);

        v0 = new VERTEX(15, 5, 10, 0, 0, 1);
        v1 = new VERTEX(0, 5, 10, 0, 0, 1);
        v2 = new VERTEX(0, 0, 10, 0, 0, 1);
        v3 = new VERTEX(15, 0, 10, 0, 0, 1);

        prim.AddTriangle(v0, v1, v3);
        prim.AddTriangle(v3, v1, v2);

        prim.Vertices.Should().HaveCount(8);

        // create a scene

        var scene = new Scenes.SceneBuilder();

        scene.AddRigidMesh(mesh, Matrix4x4.Identity);

        // save the model in different formats

        AttachmentInfo
            .From("mesh.glb")
            .WriteObject(f => scene.ToGltf2().Save(f));

        AttachmentInfo
            .From("mesh.gltf")
            .WriteObject(f => scene.ToGltf2().Save(f));
    }

}
