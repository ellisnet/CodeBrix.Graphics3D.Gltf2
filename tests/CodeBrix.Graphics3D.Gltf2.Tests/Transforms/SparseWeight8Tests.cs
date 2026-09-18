using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Transforms; //was previously: SharpGLTF.Transforms;

public class SparseWeight8Tests
{
    [Theory]
    [InlineData(0f)]
    [InlineData(1f)]
    [InlineData(0f, 0.0001f)]
    [InlineData(2f, -2f, 2f, -2f)]
    [InlineData(0.2f, 0.15f, 0.25f, 0.10f, 0.30f)]
    [InlineData(0f, 0f, 1f, 0f, 2f, 0f, 3f, 4f, 5f, 0f, 6f, 0f, 7f, 0f, 6f, 0f, 9f, 0f, 11f)]
    [InlineData(9f, -9f, 8f, -8f, 7f, -7f, 6f, -6f, 5f, -5f, 4f, -4f, 3f, -3f, 2f, -2f, 1f, -1f)]
    [InlineData(1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f)]
    [InlineData(0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f)]
    public void TestSparseCreation(params float[] array1)
    {
        var array2 = CreateSparseCompatibleArray(array1);

        var array3 = array1
            .Select((val, idx) => (idx, val))
            .Where(item => item.val != 0)
            .Reverse()
            .ToArray();

        // creation mode 1
        var sparse = SparseWeight8.Create(array1);
        sparse.WeightSum.Should().Be(array2.Sum());
        sparse.Expand(array2.Length).Should().Equal(array2);

        // creation mode 2
        var indexedSparse = SparseWeight8.Create(array3);
        indexedSparse.WeightSum.Should().BeApproximately(array2.Sum(), 0.000001f);
        indexedSparse.Expand(array2.Length).Should().Equal(array2);

        SparseWeight8.AreEqual(sparse, indexedSparse).Should().BeTrue();

        // sort by weights
        var sByWeights = SparseWeight8.OrderedByWeight(sparse);
        sByWeights.WeightSum.Should().Be(array2.Sum());
        sByWeights.Expand(array2.Length).Should().Equal(array2);
        CheckWeightOrdered(sByWeights);

        // sort by indices
        var sByIndices = SparseWeight8.OrderedByIndex(sByWeights);
        CheckIndexOrdered(sByIndices);
        sByIndices.WeightSum.Should().Be(array2.Sum());
        sByIndices.Expand(array2.Length).Should().Equal(array2);

        // equality
        SparseWeight8.AreEqual(sByIndices, sByWeights).Should().BeTrue();
        sByWeights.GetHashCode().Should().Be(sByIndices.GetHashCode());

        // sum
        var sum = SparseWeight8.Add(sByIndices, sByWeights);
        sum.WeightSum.Should().Be(array2.Sum() * 2);

        // complement normalization
        if (!array2.Any(item => item<0))
        {
            sparse.GetNormalizedWithComplement(int.MaxValue).WeightSum.Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public void TestSparseCreationWithRepeatedIndices()
    {
        var sparse = SparseWeight8.Create
            (
            (9, 9),
            (8, 2),
            (5, 1), // we set these weights separately
            (5, 1), // to check that 5 will pass 8
            (5, 1), // in the sorted set.
            (7, 1)
            );

        sparse[5].Should().Be(3);
        sparse[7].Should().Be(1);
        sparse[8].Should().Be(2);
        sparse[9].Should().Be(9);
    }

    [Fact]
    public void TestCreateSparseFromVectors()
    {
        SparseWeight8.Create(new System.Numerics.Vector4(0, 1, 2, 3), new System.Numerics.Vector4(1, 1, 1, 1)).Expand(4).Should().Equal(SparseWeight8.Create(1, 1, 1, 1).Expand(4));

        SparseWeight8.Create(new System.Numerics.Vector4(0, 1, 2, 3), new System.Numerics.Vector4(1, 2, 3, 4)).Expand(4).Should().Equal(SparseWeight8.Create(1, 2, 3, 4).Expand(4));

        SparseWeight8.Create(new System.Numerics.Vector4(0, 1, 2, 3), new System.Numerics.Vector4(4, 3, 2, 1)).Expand(4).Should().Equal(SparseWeight8.Create(4, 3, 2, 1).Expand(4));

        SparseWeight8.Create(new System.Numerics.Vector4(0, 2, 2, 3), new System.Numerics.Vector4(4, 3, 2, 1)).Expand(4).Should().Equal(SparseWeight8.Create(4, 0, 5, 1).Expand(4));

        SparseWeight8.Create(new System.Numerics.Vector4(1, 1, 1, 1), new System.Numerics.Vector4(1, 1, 1, 1)).Expand(4).Should().Equal(SparseWeight8.Create(0, 4, 0, 0).Expand(4));
    }

    /// <summary>
    /// Creates a new array with only the 8 most relevant weights.
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    static float[] CreateSparseCompatibleArray(params float[] array)
    {
        const int MAXWEIGHTS = 8;

        if (array == null) return null;

        var threshold =array
            .Select(item => Math.Abs(item))
            .OrderByDescending(item => item)
            .Take(MAXWEIGHTS)
            .Min();

        var array2 = new float[array.Length];

        var c = 0;

        for(int i=0; i < array2.Length; ++i)
        {
            var v = array[i];
            if (v == 0) continue;

            if (Math.Abs(v) >= threshold)
            {
                array2[i] = v;
                ++c;

                if (c >= MAXWEIGHTS) return array2;
            }
        }

        return array2;
    }

    static void CheckWeightOrdered(SparseWeight8 sparse)
    {
        Assert.Multiple(() =>
        {
            Math.Abs(sparse.Weight0).Should().BeGreaterThanOrEqualTo(Math.Abs(sparse.Weight1));
            Math.Abs(sparse.Weight1).Should().BeGreaterThanOrEqualTo(Math.Abs(sparse.Weight2));
            Math.Abs(sparse.Weight2).Should().BeGreaterThanOrEqualTo(Math.Abs(sparse.Weight3));
            Math.Abs(sparse.Weight3).Should().BeGreaterThanOrEqualTo(Math.Abs(sparse.Weight4));
            Math.Abs(sparse.Weight4).Should().BeGreaterThanOrEqualTo(Math.Abs(sparse.Weight5));
            Math.Abs(sparse.Weight5).Should().BeGreaterThanOrEqualTo(Math.Abs(sparse.Weight6));
            Math.Abs(sparse.Weight6).Should().BeGreaterThanOrEqualTo(Math.Abs(sparse.Weight7));
        });
    }

    static void CheckIndexOrdered(SparseWeight8 sparse)
    {
        var pairs = sparse.GetIndexedWeights();

        bool zeroFound = false;
        long lastIndex = long.MinValue;

        foreach(var (index,weight) in pairs)
        {
            if (weight == 0) zeroFound = true;

            if (zeroFound)
            {
                index.Should().Be(0);
                weight.Should().Be(0);
                continue;
            }

            ((long)index).Should().BeGreaterThan(lastIndex);

            lastIndex = index;
        }
    }

    [Fact]
    public void TestSparseNormalization()
    {
        var sparse1 = SparseWeight8
            .Create(0, 0, 0, 0, 0, 0.1f, 0.7f, 0, 0, 0, 0.1f)
            .GetNormalizedWithComplement(int.MaxValue);

        sparse1[5].Should().Be(0.1f);
        sparse1[6].Should().Be(0.7f);
        sparse1[10].Should().Be(0.1f);
        sparse1[int.MaxValue].Should().BeApproximately(0.1f, 0.0000001f);
        sparse1.WeightSum.Should().Be(1);
    }

    [Fact]
    public void TestSparseEquality()
    {
        SparseWeight8.AreEqual(SparseWeight8.Create(0, 1), SparseWeight8.Create(0, 1)).Should().BeTrue();

        SparseWeight8.AreEqual(SparseWeight8.Create(0, 1), SparseWeight8.Create(0, 1, 0.25f)).Should().BeFalse();
        SparseWeight8.AreEqual(SparseWeight8.Create(0, 1), SparseWeight8.Create(1, 0)).Should().BeFalse();

        // check if two "half weights" are equal to one "full weight"
        //SparseWeight8.AreWeightsEqual(SparseWeight8.Create((3, 5), (3, 5)), SparseWeight8.Create((3, 10))).Should().BeTrue();
    }

    [Fact]
    public void TestSparseWeightsLinearInterpolation1()
    {
        var x = SparseWeight8.Create(0,0,1,2); x.Expand(4).Should().Equal(new[] { 0f, 0f, 1f, 2f });
        var y = SparseWeight8.Create(1,2,0,0); y.Expand(4).Should().Equal(new[] { 1f, 2f, 0f, 0f });

        var z = SparseWeight8.InterpolateLinear(x, y, 0.5f);
        z[0].Should().Be(0.5f);
        z[1].Should().Be(1);
        z[2].Should().Be(0.5f);
        z[3].Should().Be(1);
    }

    [Fact]
    public void TestSparseWeightsLinearInterpolation2()
    {
        var ax = new float[] { 0, 0,    0, 0,    0, 0.1f, 0.7f, 0, 0, 0, 0.1f };
        var ay = new float[] { 0, 0, 0.2f, 0, 0.1f,    0,    0, 0, 0, 0,    0, 0, 0.2f };
        var cc = Math.Min(ax.Length, ay.Length);

        var x = SparseWeight8.Create(ax); x.Expand(ax.Length).Should().Equal(ax);
        var y = SparseWeight8.Create(ay); y.Expand(ay.Length).Should().Equal(ay);

        var z = SparseWeight8.InterpolateLinear(x, y, 0.5f);

        for (int i=0; i < cc; ++i)
        {
            var w = (ax[i] + ay[i]) / 2;
            z[i].Should().Be(w);
        }
    }

    [Fact]
    public void TestSparseWeightsCubicInterpolation()
    {
        var a = SparseWeight8.Create(0, 0, 0.2f, 0, 0, 0, 1);
        var b = SparseWeight8.Create(1, 1, 0.4f, 0, 0, 1, 0);
        var t = SparseWeight8.Subtract(b, a);
        t[0].Should().Be(1);
        t[1].Should().Be(1);
        t[2].Should().Be(0.2f);
        t[3].Should().Be(0);
        t[4].Should().Be(0);
        t[5].Should().Be(1);
        t[6].Should().Be(-1);

        var lr = SparseWeight8.InterpolateLinear(a, b, 0.4f);
        var cr = SparseWeight8.InterpolateCubic(a, t, b, t, 0.4f);

        cr[0].Should().BeApproximately(lr[0], 0.000001f);
        cr[1].Should().BeApproximately(lr[1], 0.000001f);
        cr[2].Should().BeApproximately(lr[2], 0.000001f);
        cr[3].Should().BeApproximately(lr[3], 0.000001f);
        cr[4].Should().BeApproximately(lr[4], 0.000001f);
        cr[5].Should().BeApproximately(lr[5], 0.000001f);
        cr[6].Should().BeApproximately(lr[6], 0.000001f);
        cr[7].Should().BeApproximately(lr[7], 0.000001f);
    }

    [Fact]
    public void TestSparseWeightReduction()
    {
        var a = SparseWeight8.Create(5, 3, 2, 4, 0, 4, 2, 6, 3, 6, 1);

        var b = a.GetTrimmed(4);

        b.GetNonZeroWeights().Count().Should().Be(4);

        b[0].Should().Be(a[0]);
        b[3].Should().Be(a[3]);
        b[7].Should().Be(a[7]);
        b[9].Should().Be(a[9]);

        b.Weight4.Should().Be(0);
        b.Weight5.Should().Be(0);
        b.Weight6.Should().Be(0);
        b.Weight7.Should().Be(0);
    }
}
