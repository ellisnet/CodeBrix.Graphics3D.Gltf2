using System;
using System.Linq;
using System.Numerics;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Memory; //was previously: SharpGLTF.Memory;

public class MemoryArrayTests
{
    [Fact]
    public void TestFloatingArrayDecoding()
    {
        _CreateFloatingAccessor(new Byte[] { 17 }, Schema2.EncodingType.UNSIGNED_BYTE, false)[0].Should().Be(17);
        _CreateFloatingAccessor(new Byte[] { 17, 0 }, Schema2.EncodingType.UNSIGNED_SHORT, false)[0].Should().Be(17);

        _CreateFloatingAccessor(new Byte[] { 17 }, Schema2.EncodingType.BYTE, false)[0].Should().Be(17);
        _CreateFloatingAccessor(new Byte[] { 17, 0 }, Schema2.EncodingType.SHORT, false)[0].Should().Be(17);

        _CreateFloatingAccessor(new Byte[] { 255 }, Schema2.EncodingType.UNSIGNED_BYTE, true)[0].Should().Be(1);
        _CreateFloatingAccessor(new Byte[] { 127 }, Schema2.EncodingType.BYTE, true)[0].Should().Be(1);
        _CreateFloatingAccessor(new Byte[] { 128 }, Schema2.EncodingType.BYTE, true)[0].Should().Be(-1);

        _CreateFloatingAccessor(new Byte[] { 255, 255 }, Schema2.EncodingType.UNSIGNED_SHORT, true)[0].Should().Be(1);
        _CreateFloatingAccessor(new Byte[] { 255, 127 }, Schema2.EncodingType.SHORT, true)[0].Should().Be(1);
        _CreateFloatingAccessor(new Byte[] { 0,  128 }, Schema2.EncodingType.SHORT, true)[0].Should().Be(-1);

        _CreateFloatingAccessor(new Byte[] { 17, 0, 0, 0 }, Schema2.EncodingType.UNSIGNED_INT, false)[0].Should().Be(17);
        _CreateFloatingAccessor(new Byte[] { 0,0, 0x80, 0x3f }, Schema2.EncodingType.FLOAT, false)[0].Should().Be(1);
    }

    private static FloatingAccessor _CreateFloatingAccessor(byte[] data, Schema2.EncodingType encoding, bool normalized)
    {
        return new FloatingAccessor(new ArraySegment<byte>(data), 0, int.MaxValue, 0, 1, encoding, normalized);
    }

    [Fact]
    public void TestFloatingArrayEncoding1()
    {
        var v1 = new Vector4(0.1f, 0.2f, 0.6f, 0.8f);
        var v2 = new Vector4(1, 2, 3, 4);

        var bytes = new Byte[256];

        var v4n = new Vector4Array(bytes, 0, Schema2.EncodingType.UNSIGNED_BYTE, true);
        v4n[1] = v1;
        NumericsAssert.AreEqual(v4n[1], v1, 0.1f);

        var v4u = new Vector4Array(bytes, 0, Schema2.EncodingType.UNSIGNED_BYTE, false);
        v4u[1] = v2;
        NumericsAssert.AreEqual(v4u[1], v2);
    }

    [Fact]
    public void TestFloatingArrayEncoding2()
    {
        var v1 = new Vector4(0.1f, 0.2f, 0.6f, 0.8f);
        var v2 = new Vector4(1, 2, 3, 4);

        var bytes = new Byte[256];

        var v4n = new Vector4Array(bytes, 0, 5, 8, Schema2.EncodingType.UNSIGNED_BYTE, true);
        var v4u = new Vector4Array(bytes, 4, 5, 8, Schema2.EncodingType.UNSIGNED_BYTE, false);

        v4n[1] = v1;
        NumericsAssert.AreEqual(v4n[1], v1, 0.1f);

        v4u[1] = v2;
        NumericsAssert.AreEqual(v4u[1], v2);
    }

    [Fact]
    public void TestLinqAccess()
    {
        var buffer = new Byte[] { 1, 52, 43, 6, 23, 234 };

        var accessor = new Vector2Array(buffer, 0, Schema2.EncodingType.BYTE, true);

        var result = accessor.ToArray();

        result.Length.Should().Be(3);
    }
}
