using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2; //was previously: SharpGLTF;

public class AnimationSamplingTests
{
    [Fact]
    public void TestAnimationSplit()
    {
        var anim0 = new[]
        {
            (0.1f, 1),
        };

        var anim1 = new[]
        {
            (0.1f, 1),
            (0.2f, 2)
        };

        var anim2 = new[]
        {
            (0.1f, 1),
            (0.2f, 2),
            (3.2f, 2),
            (3.3f, 2)
        };

        var anim3 = new[]
        {
            (2.1f, 1),
            (2.2f, 2),
            (3.2f, 3),
            (3.3f, 4),
            (4.0f, 5),
            (4.1f, 6),
            (5.0f, 7),
        };

        void checkSegment(int time, (float,int)[] segment)
        {
            // should check all times are incremental
            segment.Length.Should().BeGreaterThan(1);
            segment.First().Item1.Should().BeLessThanOrEqualTo(time);
            segment.Last().Item1.Should().BeGreaterThan(time);
        }

        var r0 = Animations.CurveSampler.SplitByTime(anim0).ToArray();
        r0.Length.Should().Be(1);
        r0[0].Length.Should().Be(1);

        var r1 = Animations.CurveSampler.SplitByTime(anim1).ToArray();
        r1.Length.Should().Be(1);
        r1[0].Length.Should().Be(2);

        var r2 = Animations.CurveSampler.SplitByTime(anim2).ToArray();
        r2.Length.Should().Be(4);
        r2[0].Length.Should().Be(3);
        r2[1].Length.Should().Be(2); checkSegment(1, r2[1]);
        r2[2].Length.Should().Be(2); checkSegment(2, r2[2]);
        r2[3].Length.Should().Be(3); checkSegment(3, r2[3]);

        var r3 = Animations.CurveSampler.SplitByTime(anim3).ToArray();
        r3.Length.Should().Be(6);
        r3[0].Length.Should().Be(1);
        r3[1].Length.Should().Be(1);
        r3[2].Length.Should().Be(3);
        r3[3].Length.Should().Be(4); checkSegment(3, r3[3]);
        r3[3].Length.Should().Be(4); checkSegment(4, r3[4]);
        r3[5].Length.Should().Be(1);
    }

    [Fact]
    public void TestFastSampler()
    {
        var curve = Enumerable
            .Range(0, 1000)
            .Select(idx => (0.1f * (float)idx, new Vector3(idx, idx, idx)))
            .ToArray();

        var slowSampler = Animations.CurveSampler.CreateSampler(curve, true, false);
        var fastSampler = Animations.CurveSampler.CreateSampler(curve, true, true);

        foreach (var k in curve)
        {
            slowSampler.GetPoint(k.Item1).Should().Be(k.Item2);
            fastSampler.GetPoint(k.Item1).Should().Be(k.Item2);
        }

        for(float t=0; t < 100; t+=0.232f)
        {
            var dv = slowSampler.GetPoint(t);
            var fv = fastSampler.GetPoint(t);

            fv.Should().Be(dv);
        }
    }

    [Theory]
    [InlineData(0, 0, 0, 1, 1, 1, 1, 0)]
    [InlineData(0, 0, 0.1f, 5, 0.7f, 3, 1, 0)]
    public void TestHermiteInterpolation1(float p1x, float p1y, float p2x, float p2y, float p3x, float p3y, float p4x, float p4y)
    {
        var p1 = new Vector2(p1x, p1y);
        var p2 = new Vector2(p2x, p2y);
        var p3 = new Vector2(p3x, p3y);
        var p4 = new Vector2(p4x, p4y);

        var ppp = new List<Vector2>();

        for (float amount = 0; amount <= 1; amount += 0.01f)
        {
            var (startPosition, endPosition, startTangent, endTangent) = Animations.CurveSampler.CreateHermitePointWeights(amount);

            var p = Vector2.Zero;

            p += p1 * startPosition;
            p += p4 * endPosition;
            p += (p2 - p1) * 4 * startTangent;
            p += (p4 - p3) * 4 * endTangent;

            ppp.Add(p);
        }

        // now lets calculate an arbitrary point and tangent

        float k = 0.3f;

        var hb = Animations.CurveSampler.CreateHermitePointWeights(k);
        var ht = Animations.CurveSampler.CreateHermiteTangentWeights(k);

        var pp = p1 * hb.StartPosition + p4 * hb.EndPosition + (p2 - p1) * 4 * hb.StartTangent + (p4 - p3) * 4 * hb.EndTangent;
        var pt = p1 * ht.StartPosition + p4 * ht.EndPosition + (p2 - p1) * 4 * ht.StartTangent + (p4 - p3) * 4 * ht.EndTangent;

        // plotting

        ppp.AttachToCurrentTest("sampling.csv");
        new[] { p1, p2, p3, p4 }.AttachToCurrentTest("source.csv");
        new[] { pp, pp + pt }.AttachToCurrentTest("tangent.csv");
    }

    [Fact]
    public void TestHermiteAsLinearInterpolation()
    {
        var p1 = new Vector2(1, 0);
        var p2 = new Vector2(3, 1);
        var t = p2 - p1;

        var ppp = new List<Vector2>();

        for (float amount = 0; amount <= 1; amount += 0.1f)
        {
            var (startPosition, endPosition, startTangent, endTangent) = Animations.CurveSampler.CreateHermitePointWeights(amount);

            var p = Vector2.Zero;

            p += p1 * startPosition;
            p += p2 * endPosition;
            p += t * startTangent;
            p += t * endTangent;

            ppp.Add(p);
        }

        ppp.AttachToCurrentTest("plot.csv");
    }

    [Fact]
    public void TestHermiteAsSphericalInterpolation()
    {
        // given two quaternions, we must find a tangent quaternion so that the quaternion
        // hermite interpolation gives roughly the same results as a plain spherical interpolation.

        // reference implementation with matrices
        var m1 = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, 1);
        var m2 = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, 2);
        var mt = Matrix4x4.Multiply(m2, Matrix4x4.Transpose(m1));
        var m2bis = Matrix4x4.Multiply(mt, m1); // roundtrip; M2 == M2BIS

        // implementation with quaternions
        var q1 = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 1);
        var q2 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 2);
        var qt = Quaternion.Concatenate(q2, Quaternion.Conjugate(q1));
        var q2bis = Quaternion.Concatenate(qt, q1); // roundtrip; Q2 == Q2BIS

        NumericsAssert.AreEqual(qt, Animations.CurveSampler.CreateTangent(q1, q2), 0.000001f);

        var angles = new List<Vector2>();

        for (float amount = 0; amount <= 1; amount += 0.025f)
        {
            // slerp interpolation
            var sq = Quaternion.Normalize(Quaternion.Slerp(q1, q2, amount));

            // hermite interpolation with a unit tangent
            var hermite = Animations.CurveSampler.CreateHermitePointWeights(amount);
            var hq = default(Quaternion);
            hq += q1 * hermite.StartPosition;
            hq += q2 * hermite.EndPosition;
            hq += qt * hermite.StartTangent;
            hq += qt * hermite.EndTangent;
            hq = Quaternion.Normalize(hq);

            // check
            NumericsAssert.AreEqual(sq, hq, 0.1f);
            NumericsAssert.AngleLessOrEqual(sq, hq, 0.22f);

            // diff
            var a = (sq, hq).GetAngle() * 180.0f / 3.141592f;
            angles.Add(new Vector2(amount, a));
        }

        angles.AttachToCurrentTest("plot.csv");

    }

    private static (float, (Vector3, Vector3, Vector3))[] _TransAnim = new []
    {
        (0.0f, (        Vector3.Zero, new Vector3(0, 0, 0),new Vector3(0, 0, 0))),
        (1.0f, (new Vector3(0, 0, 0), new Vector3(1, 0, 0),new Vector3(0, 1, 0))),
        (2.0f, (new Vector3(0, -1, 0), new Vector3(2, 0, 0),new Vector3(0, 0, 0))),
        (3.0f, (new Vector3(0, 0, 0), new Vector3(3, 0, 0),       Vector3.Zero ))
    };

    private static (float, (Quaternion, Quaternion, Quaternion))[] _RotAnim = new[]
    {
        (0.0f, (new Quaternion(0,0,0,0), Quaternion.CreateFromYawPitchRoll(+1.6f, 0, 0), new Quaternion(0,0,0,0))),
        (1.0f, (new Quaternion(0,0,0,0), Quaternion.Identity, new Quaternion(0,0,0,0))),
        (2.0f, (new Quaternion(0,0,0,0), Quaternion.CreateFromYawPitchRoll(-1.6f, 0, 0), new Quaternion(0,0,0,0))),
        (3.0f, (new Quaternion(0,0,0,0), Quaternion.Identity, new Quaternion(0,0,0,0))),
        (4.0f, (new Quaternion(0,0,0,0), Quaternion.CreateFromYawPitchRoll(+1.6f, 0, 0), new Quaternion(0,0,0,0))),
    };

    [Fact]
    public void TestVector3CubicSplineSampling()
    {
        var sampler = Animations.CurveSampler.CreateSampler(_TransAnim);

        var points = new List<Vector3>();

        for(int i=0; i < 300; ++i)
        {
            var sample = sampler.GetPoint(((float)i) / 100.0f);
            points.Add( sample );
        }

        points
            .Select(p => new Vector2(p.X, p.Y))
            .AttachToCurrentTest("plot.csv");
    }

    [Fact]
    public void TestQuaternionCubicSplineSampling()
    {
        var sampler = Animations.CurveSampler.CreateSampler(_RotAnim);

        var a = sampler.GetPoint(0);
        var b = sampler.GetPoint(1);
        var bc = sampler.GetPoint(1.5f);
        var c = sampler.GetPoint(2);
    }

    [Fact]
    public void TestBooleanCurve()
    {
        var anim = new[]
        {
            (2.1f, true),
            (2.2f, false),
            (3.2f, true),
            (3.3f, false),
            (4.0f, true),
            (4.1f, false),
            (5.0f, true),
        };

        var r0 = Animations.CurveSampler.SplitByTime(anim).ToArray();
        r0.Length.Should().Be(6);

        var sampler = Animations.CurveSampler.CreateSampler(anim);
        sampler.GetPoint(0).Should().Be(true);
        sampler.GetPoint(2.1f).Should().Be(true);
        sampler.GetPoint(2.12f).Should().Be(true);
        sampler.GetPoint(2.18f).Should().Be(true);
        sampler.GetPoint(2.22f).Should().Be(false);
        sampler.GetPoint(6).Should().Be(true);
    }

}
