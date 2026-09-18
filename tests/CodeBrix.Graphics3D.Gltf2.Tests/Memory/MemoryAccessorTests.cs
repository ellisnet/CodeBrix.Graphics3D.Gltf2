using System;
using System.Collections.Generic;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Memory; //was previously: SharpGLTF.Memory;

public class MemoryAccessorTests
{
    [Fact]
    public void CreateInterleaved1()
    {
        var pos = MemoryAccessInfo.CreateDefaultElement("POSITION");
        var nrm = MemoryAccessInfo.CreateDefaultElement("NORMAL");

        var attributes = new[] { pos, nrm };

        const int baseOffset = 8;

        var byteStride = MemoryAccessInfo.SetInterleavedInfo(attributes, baseOffset, 5);

        pos = attributes[0];
        nrm = attributes[1];

        byteStride.Should().Be(24);

        pos.ByteOffset.Should().Be(baseOffset + 0);
        pos.ByteStride.Should().Be(24);
        pos.ItemsCount.Should().Be(5);

        nrm.ByteOffset.Should().Be(baseOffset + 12);
        nrm.ByteStride.Should().Be(24);
        nrm.ItemsCount.Should().Be(5);

        pos.IsValidVertexAttribute.Should().BeTrue();
        nrm.IsValidVertexAttribute.Should().BeTrue();
    }
}
