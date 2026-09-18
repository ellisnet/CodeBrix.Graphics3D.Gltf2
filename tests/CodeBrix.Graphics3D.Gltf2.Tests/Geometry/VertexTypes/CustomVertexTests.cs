using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes; //was previously: SharpGLTF.Geometry.VertexTypes;

public class CustomVertexTests
{
    [Fact]
    public void CreateCustomVertexTest()
    {
        var v2 = new VertexCustom2(0.3f, Vector4.One);
        var v1 = new VertexColor1Texture1Custom1(v2);

        v1.CustomId.Should().Be(0.3f);
    }

    [Fact]
    public void TransferContentTest()
    {
        var v1 = new VertexColor1Texture1Custom1(Vector4.One, Vector2.One, 0.3f);
        var v2 = v1.ConvertToMaterial<VertexCustom2>();
        v2.CustomId0.Should().Be(0.3f);
    }

}
