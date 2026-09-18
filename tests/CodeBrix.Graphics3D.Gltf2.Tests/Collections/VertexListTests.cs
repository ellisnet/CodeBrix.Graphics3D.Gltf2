using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SilverAssertions;
using Xunit;
using XYZ = System.Numerics.Vector3;

namespace CodeBrix.Graphics3D.Gltf2.Collections; //was previously: SharpGLTF.Collections;

public class VertexListTests
{
    [Fact]
    public void TestFloatHashCode()
    {
        // it's important to ensure that the hash or positive and negative Zero is the same.

        float positiveZero = 0f;
        float negativeZero = -positiveZero;

        var floats = new float[] { positiveZero, negativeZero };
        var integers = System.Runtime.InteropServices.MemoryMarshal.Cast<float, uint>(floats);
        integers[1].Should().NotBe(integers[0]);

        var positiveHash = positiveZero.GetHashCode();
        var negativeHash = negativeZero.GetHashCode();
        negativeHash.Should().Be(positiveHash);
    }

    [System.Diagnostics.DebuggerDisplay("{Value}")]
    struct _VertexExample
    {
        public static implicit operator _VertexExample(int value)
        {
            return new _VertexExample(value);
        }

        public _VertexExample(int val)
        {
            Value = val;
        }

        public int Value;
    }

    [Fact]
    public void TestVertexListDictionary()
    {
        var list = new VertexList<_VertexExample>();

        list.Use(5);
        list.Should().HaveCount(1);

        list.Use(7);
        list.Should().HaveCount(2);
        list[0].Value.Should().Be(5);
        list[1].Value.Should().Be(7);

        list.Use(5);
        list.Should().HaveCount(2);

        var list2 = new VertexList<_VertexExample>();
        list.CopyTo(list2);
        list2.Should().HaveCount(2);

    }

    [Fact]
    public void TestValueListSet()
    {
        var a = new XYZ(1.1f);
        var b = new XYZ(1.2f);
        var c = new XYZ(1.3f);
        var d = new XYZ(1.4f);

        var vlist = new ValueListSet<XYZ>();

        var idx0 = vlist.Use(a); idx0.Should().Be(0);
        var idx1 = vlist.Use(b); idx1.Should().Be(1);
        var idx2 = vlist.Use(a); idx2.Should().Be(0);

        vlist[idx0].Should().Be(a);
        vlist[idx1].Should().Be(b);
        vlist[idx2].Should().Be(a);

        new[] { a, b }.Should().Equal(vlist.ToArray());

        vlist.Use(c);
        vlist.Use(d);
        new[] { a, b, c, d }.Should().Equal(vlist.ToArray());

        var vlist2 = new ValueListSet<XYZ>();
        vlist.CopyTo(vlist2);

        vlist2[0].Should().Be(vlist[0]);
        vlist2[1].Should().Be(vlist[1]);
        vlist2[2].Should().Be(vlist[2]);
        vlist2[3].Should().Be(vlist[3]);

    }

}
