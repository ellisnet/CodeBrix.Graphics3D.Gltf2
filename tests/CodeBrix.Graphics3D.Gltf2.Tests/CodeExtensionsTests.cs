using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2; //was previously: SharpGLTF;

public class CodeExtensionsTests
{
    internal static int _WordPadded(int length)
    {
        var padding = (length & 3);

        return length + (padding == 0 ? 0 : 4 - padding);
    }

    [Fact]
    public void TestPadding()
    {
        _WordPadded(1).Should().Be(4);
        _WordPadded(2).Should().Be(4);
        _WordPadded(3).Should().Be(4);
        _WordPadded(4).Should().Be(4);
        _WordPadded(5).Should().Be(8);
    }

    [Fact]
    public void TestAsNullableExtensions()
    {
        // the AsNullable extensions are a bit tricky;
        // they should default to null regardless of the value being inside or outside the bounds of min-max
        // but if after the min.max has affected the value, the default-to-null check still applies.

        5.AsNullable(5).Should().Be(null);

        0.AsNullable(3, 1, 5).Should().Be(1);
        1.AsNullable(3, 1, 5).Should().Be(1);
        2.AsNullable(3, 1, 5).Should().Be(2);
        3.AsNullable(3, 1, 5).Should().Be(null);
        4.AsNullable(3, 1, 5).Should().Be(4);
        5.AsNullable(3, 1, 5).Should().Be(5);
        6.AsNullable(3, 1, 5).Should().Be(5);

        // vectors

        Vector2.Zero.AsNullable(Vector2.One, Vector2.One, Vector2.One * 2).Should().Be(null);
        new Vector2(3).AsNullable(Vector2.One, Vector2.One, Vector2.One * 2).Should().Be(new Vector2(2));

        Vector3.Zero.AsNullable(Vector3.One, Vector3.One, Vector3.One * 2).Should().Be(null);
        new Vector3(3).AsNullable(Vector3.One, Vector3.One, Vector3.One * 2).Should().Be(new Vector3(2));

        Vector4.Zero.AsNullable(Vector4.One, Vector4.One, Vector4.One * 2).Should().Be(null);
        new Vector4(3).AsNullable(Vector4.One, Vector4.One, Vector4.One * 2).Should().Be(new Vector4(2));

        // special case: default values outside the min-max range should also return null
        0.AsNullable( 0, 1, 5).Should().Be(null);
        10.AsNullable(10, 1, 5).Should().Be(null);

        Vector2.Zero.AsNullable(Vector2.Zero, Vector2.One, Vector2.One * 5).Should().Be(null);
        Vector3.Zero.AsNullable(Vector3.Zero, Vector3.One, Vector3.One * 5).Should().Be(null);
        Vector4.Zero.AsNullable(Vector4.Zero, Vector4.One, Vector4.One * 5).Should().Be(null);
    }
}
