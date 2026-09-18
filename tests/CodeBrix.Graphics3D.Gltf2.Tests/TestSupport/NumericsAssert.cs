using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2; //was previously: SharpGLTF;

// [System.Diagnostics.DebuggerStepThrough]
public static class NumericsAssert
{
    public static double UnitError(this Vector3 v) { return v.LengthError(1); }

    public static double LengthError(this Vector3 v, double expectedLength)
    {
        return Math.Abs(Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z) - expectedLength);
    }

    public static void IsFinite(Single value, string message = null)
    {
        // float.IsFinite(value).Should().BeTrue(message);

        (!float.IsNaN(value) && !float.IsInfinity(value)).Should().BeTrue(message);
    }

    public static void IsFinite(Double value, string message = null)
    {
        // double.IsFinite(value).Should().BeTrue(message);

        (!Double.IsNaN(value) && !Double.IsInfinity(value)).Should().BeTrue(message);
    }

    public static void IsFinite(Vector2 vector)
    {
        IsFinite(vector.X, "X");
        IsFinite(vector.Y, "Y");
    }

    public static void IsFinite(Vector3 vector)
    {
        IsFinite(vector.X, "X");
        IsFinite(vector.Y, "Y");
        IsFinite(vector.Z, "Z");
    }

    public static void IsFinite(Vector4 vector)
    {
        IsFinite(vector.X, "X");
        IsFinite(vector.Y, "Y");
        IsFinite(vector.Z, "Z");
        IsFinite(vector.W, "W");
    }

    public static void IsFinite(Quaternion quaternion)
    {
        IsFinite(quaternion.X, "X");
        IsFinite(quaternion.Y, "Y");
        IsFinite(quaternion.Z, "Z");
        IsFinite(quaternion.W, "W");
    }

    public static void IsFinite(Plane plane)
    {
        IsFinite(plane.Normal.X, "Normal.X");
        IsFinite(plane.Normal.Y, "Normal.Y");
        IsFinite(plane.Normal.Z, "Normal.Z");
        IsFinite(plane.D, "D");
    }

    public static void IsFinite(Matrix3x2 matrix)
    {
        IsFinite(matrix.M11, "M11");
        IsFinite(matrix.M12, "M12");

        IsFinite(matrix.M21, "M21");
        IsFinite(matrix.M22, "M22");

        IsFinite(matrix.M31, "M31");
        IsFinite(matrix.M32, "M32");
    }

    public static void IsFinite(Matrix4x4 matrix)
    {
        IsFinite(matrix.M11, "M11");
        IsFinite(matrix.M12, "M12");
        IsFinite(matrix.M13, "M13");
        IsFinite(matrix.M14, "M14");

        IsFinite(matrix.M21, "M21");
        IsFinite(matrix.M22, "M22");
        IsFinite(matrix.M23, "M23");
        IsFinite(matrix.M24, "M24");

        IsFinite(matrix.M31, "M31");
        IsFinite(matrix.M32, "M32");
        IsFinite(matrix.M33, "M33");
        IsFinite(matrix.M34, "M34");

        IsFinite(matrix.M41, "M41");
        IsFinite(matrix.M42, "M42");
        IsFinite(matrix.M43, "M43");
        IsFinite(matrix.M44, "M44");
    }

    public static void AreEqual(BigInteger expected, BigInteger actual, double tolerance = 0)
    {
        ((double)BigInteger.Abs(actual - expected)).Should().BeApproximately(0, tolerance);
    }

    public static void AreEqual(Vector2 expected, Vector2 actual, double tolerance = 0)
    {
        Assert.Multiple(() =>
        {
            actual.X.Should().BeApproximately(expected.X, (float)tolerance, "X");
            actual.Y.Should().BeApproximately(expected.Y, (float)tolerance, "Y");
        });
    }

    public static float AreEqual(Vector3 expected, Vector3 actual, double tolerance = 0)
    {
        Assert.Multiple(() =>
        {
            actual.X.Should().BeApproximately(expected.X, (float)tolerance, "X");
            actual.Y.Should().BeApproximately(expected.Y, (float)tolerance, "Y");
            actual.Z.Should().BeApproximately(expected.Z, (float)tolerance, "Z");
        });

        // get tolerance
        var tx = Math.Abs(expected.X - actual.X);
        var ty = Math.Abs(expected.Y - actual.Y);
        var tz = Math.Abs(expected.Z - actual.Z);
        return Math.Max(tx, Math.Max(ty, tz));
    }

    public static void AreEqual(Vector4 expected, Vector4 actual, double tolerance = 0)
    {
        Assert.Multiple(() =>
        {
            actual.X.Should().BeApproximately(expected.X, (float)tolerance, "X");
            actual.Y.Should().BeApproximately(expected.Y, (float)tolerance, "Y");
            actual.Z.Should().BeApproximately(expected.Z, (float)tolerance, "Z");
            actual.W.Should().BeApproximately(expected.W, (float)tolerance, "W");
        });
    }

    public static void AreEqual(Quaternion expected, Quaternion actual, double tolerance = 0)
    {
        Assert.Multiple(() =>
        {
            actual.X.Should().BeApproximately(expected.X, (float)tolerance, "X");
            actual.Y.Should().BeApproximately(expected.Y, (float)tolerance, "Y");
            actual.Z.Should().BeApproximately(expected.Z, (float)tolerance, "Z");
            actual.W.Should().BeApproximately(expected.W, (float)tolerance, "W");
        });
    }

    public static void AreEqual(Matrix4x4 expected, Matrix4x4 actual, double tolerance = 0)
    {
        Assert.Multiple(() =>
        {
            actual.M11.Should().BeApproximately(expected.M11, (float)tolerance, "M11");
            actual.M12.Should().BeApproximately(expected.M12, (float)tolerance, "M12");
            actual.M13.Should().BeApproximately(expected.M13, (float)tolerance, "M13");
            actual.M14.Should().BeApproximately(expected.M14, (float)tolerance, "M14");

            actual.M21.Should().BeApproximately(expected.M21, (float)tolerance, "M21");
            actual.M22.Should().BeApproximately(expected.M22, (float)tolerance, "M22");
            actual.M23.Should().BeApproximately(expected.M23, (float)tolerance, "M23");
            actual.M24.Should().BeApproximately(expected.M24, (float)tolerance, "M24");

            actual.M31.Should().BeApproximately(expected.M31, (float)tolerance, "M31");
            actual.M32.Should().BeApproximately(expected.M32, (float)tolerance, "M32");
            actual.M33.Should().BeApproximately(expected.M33, (float)tolerance, "M33");
            actual.M34.Should().BeApproximately(expected.M34, (float)tolerance, "M34");

            actual.M41.Should().BeApproximately(expected.M41, (float)tolerance, "M41");
            actual.M42.Should().BeApproximately(expected.M42, (float)tolerance, "M42");
            actual.M43.Should().BeApproximately(expected.M43, (float)tolerance, "M43");
            actual.M44.Should().BeApproximately(expected.M44, (float)tolerance, "M44");
        });
    }

    public static float AreGeometryicallyEquivalent(Matrix4x4 expected, Matrix4x4 actual, double tolerance = 0)
    {
        var expectedX = Vector3.Transform(Vector3.UnitX, expected);
        var expectedY = Vector3.Transform(Vector3.UnitY, expected);
        var expectedZ = Vector3.Transform(Vector3.UnitZ, expected);

        var actualX = Vector3.Transform(Vector3.UnitX, actual);
        var actualY = Vector3.Transform(Vector3.UnitY, actual);
        var actualZ = Vector3.Transform(Vector3.UnitZ, actual);

        var tx = AreEqual(expectedX, actualX, tolerance);
        var ty = AreEqual(expectedY, actualY, tolerance);
        var tz = AreEqual(expectedZ, actualZ, tolerance);
        return Math.Max(tx, Math.Max(ty, tz));
    }

    public static void IsInvertible(Matrix3x2 matrix)
    {
        IsFinite(matrix);
        Matrix3x2.Invert(matrix, out Matrix3x2 inverted).Should().BeTrue();
    }

    public static void IsInvertible(Matrix4x4 matrix)
    {
        IsFinite(matrix);
        Matrix4x4.Invert(matrix, out Matrix4x4 inverted).Should().BeTrue();
    }

    public static void IsOrthogonal3x3(Matrix4x4 matrix, double tolerance = 0)
    {
        IsFinite(matrix);

        Assert.Multiple(() =>
        {
            matrix.M41.Should().Be(0);
            matrix.M42.Should().Be(0);
            matrix.M43.Should().Be(0);
            matrix.M44.Should().Be(1);
        });

        var cx = new Vector3(matrix.M11, matrix.M21, matrix.M31);
        var cy = new Vector3(matrix.M12, matrix.M22, matrix.M32);
        var cz = new Vector3(matrix.M13, matrix.M23, matrix.M33);

        Assert.Multiple(() =>
        {
            Vector3.Dot(cx, cy).Should().BeApproximately(0, (float)tolerance);
            Vector3.Dot(cx, cz).Should().BeApproximately(0, (float)tolerance);
            Vector3.Dot(cy, cz).Should().BeApproximately(0, (float)tolerance);
        });
    }

    public static void Length(Vector2 actual, double length, double tolerance = 0)
    {
        IsFinite(actual);

        length = Math.Abs(actual.Length() - length);

        length.Should().BeApproximately(0, tolerance);
    }

    public static void Length(Vector3 actual, double length, double tolerance = 0)
    {
        IsFinite(actual);

        length = Math.Abs(actual.Length() - length);

        length.Should().BeApproximately(0, tolerance);
    }

    public static void Length(Vector4 actual, double length, double tolerance = 0)
    {
        IsFinite(actual);

        length = Math.Abs(actual.Length() - length);

        length.Should().BeApproximately(0, tolerance);
    }

    public static void Length(Quaternion actual, double length, double tolerance = 0)
    {
        IsFinite(actual);

        length = Math.Abs(actual.Length() - length);

        length.Should().BeApproximately(0, tolerance);
    }

    public static void IsNormalized(Vector2 actual, double tolerance = 0)
    {
        Length(actual, 1, tolerance);
    }

    public static void IsNormalized(Vector3 actual, double tolerance = 0)
    {
        Length(actual, 1, tolerance);
    }

    public static void IsNormalized(Vector4 actual, double tolerance = 0)
    {
        Length(actual, 1, tolerance);
    }

    public static void IsNormalized(Quaternion actual, double tolerance = 0)
    {
        Length(actual, 1, tolerance);
    }

    public static void InRange(BigInteger value, BigInteger min, BigInteger max)
    {
        GreaterOrEqual(value, min);
        LessOrEqual(value, max);
    }

    public static void InRange(Vector2 value, Vector2 min, Vector2 max)
    {
        GreaterOrEqual(value, min);
        LessOrEqual(value, max);
    }

    public static void InRange(Vector3 value, Vector3 min, Vector3 max)
    {
        GreaterOrEqual(value, min);
        LessOrEqual(value, max);
    }

    public static void InRange(Vector4 value, Vector4 min, Vector4 max)
    {
        GreaterOrEqual(value, min);
        LessOrEqual(value, max);
    }

    public static void Less(BigInteger arg1, BigInteger arg2)
    {
        arg1.CompareTo(arg2).Should().BeLessThan(0);
    }

    public static void Less(Vector2 arg1, Vector2 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeLessThan(arg2.X, "X");
            arg1.Y.Should().BeLessThan(arg2.Y, "Y");
        });
    }

    public static void Less(Vector3 arg1, Vector3 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeLessThan(arg2.X, "X");
            arg1.Y.Should().BeLessThan(arg2.Y, "Y");
            arg1.Z.Should().BeLessThan(arg2.Z, "Z");
        });
    }

    public static void Less(Vector4 arg1, Vector4 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeLessThan(arg2.X, "X");
            arg1.Y.Should().BeLessThan(arg2.Y, "Y");
            arg1.Z.Should().BeLessThan(arg2.Z, "Z");
            arg1.W.Should().BeLessThan(arg2.W, "W");
        });
    }

    public static void LessOrEqual(BigInteger arg1, BigInteger arg2)
    {
        arg1.CompareTo(arg2).Should().BeLessThanOrEqualTo(0);
    }

    public static void LessOrEqual(Vector2 arg1, Vector2 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeLessThanOrEqualTo(arg2.X, "X");
            arg1.Y.Should().BeLessThanOrEqualTo(arg2.Y, "Y");
        });
    }

    public static void LessOrEqual(Vector3 arg1, Vector3 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeLessThanOrEqualTo(arg2.X, "X");
            arg1.Y.Should().BeLessThanOrEqualTo(arg2.Y, "Y");
            arg1.Z.Should().BeLessThanOrEqualTo(arg2.Z, "Z");
        });
    }

    public static void LessOrEqual(Vector4 arg1, Vector4 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeLessThanOrEqualTo(arg2.X, "X");
            arg1.Y.Should().BeLessThanOrEqualTo(arg2.Y, "Y");
            arg1.Z.Should().BeLessThanOrEqualTo(arg2.Z, "Z");
            arg1.W.Should().BeLessThanOrEqualTo(arg2.W, "W");
        });
    }

    public static void Greater(BigInteger arg1, BigInteger arg2)
    {
        arg1.CompareTo(arg2).Should().BeGreaterThan(0);
    }

    public static void Greater(Vector2 arg1, Vector2 arg2)
    {
        arg1.X.Should().BeGreaterThan(arg2.X, "X");
        arg1.Y.Should().BeGreaterThan(arg2.Y, "Y");
    }

    public static void Greater(Vector3 arg1, Vector3 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeGreaterThan(arg2.X, "X");
            arg1.Y.Should().BeGreaterThan(arg2.Y, "Y");
            arg1.Z.Should().BeGreaterThan(arg2.Z, "Z");
        });
    }

    public static void Greater(Vector4 arg1, Vector4 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeGreaterThan(arg2.X, "X");
            arg1.Y.Should().BeGreaterThan(arg2.Y, "Y");
            arg1.Z.Should().BeGreaterThan(arg2.Z, "Z");
            arg1.W.Should().BeGreaterThan(arg2.W, "W");
        });
    }

    public static void GreaterOrEqual(BigInteger arg1, BigInteger arg2)
    {
        arg1.CompareTo(arg2).Should().BeGreaterThanOrEqualTo(0);
    }

    public static void GreaterOrEqual(Vector2 arg1, Vector2 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeGreaterThanOrEqualTo(arg2.X, "X");
            arg1.Y.Should().BeGreaterThanOrEqualTo(arg2.Y, "Y");
        });
    }

    public static void GreaterOrEqual(Vector3 arg1, Vector3 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeGreaterThanOrEqualTo(arg2.X, "X");
            arg1.Y.Should().BeGreaterThanOrEqualTo(arg2.Y, "Y");
            arg1.Z.Should().BeGreaterThanOrEqualTo(arg2.Z, "Z");
        });
    }

    public static void GreaterOrEqual(Vector4 arg1, Vector4 arg2)
    {
        Assert.Multiple(() =>
        {
            arg1.X.Should().BeGreaterThanOrEqualTo(arg2.X, "X");
            arg1.Y.Should().BeGreaterThanOrEqualTo(arg2.Y, "Y");
            arg1.Z.Should().BeGreaterThanOrEqualTo(arg2.Z, "Z");
            arg1.W.Should().BeGreaterThanOrEqualTo(arg2.W, "W");
        });
    }

    public static void AngleLessOrEqual(Vector2 a, Vector2 b, double radians)
    {
        var angle = (a, b).GetAngle();

        ((double)angle).Should().BeLessThanOrEqualTo(radians, "Angle");
    }

    public static void AngleLessOrEqual(Vector3 a, Vector3 b, double radians)
    {
        var angle = (a, b).GetAngle();

        ((double)angle).Should().BeLessThanOrEqualTo(radians, "Angle");
    }

    public static void AngleLessOrEqual(Quaternion a, Quaternion b, double radians)
    {
        var angle = (a, b).GetAngle();

        ((double)angle).Should().BeLessThanOrEqualTo(radians, "Angle");
    }
}
