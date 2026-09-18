using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CodeBrix.Graphics3D.Gltf2;

/// <summary>
/// Writes sampled 2D points as a CSV attachment of the running test, so a
/// sampled curve can be inspected or charted after a run.
/// </summary>
public static class PointSeriesUtils
{
    /// <summary>Writes <paramref name="points"/> as "x,y" lines to <paramref name="fileName"/>.</summary>
    public static FileInfo AttachToCurrentTest(this IEnumerable<Vector2> points, string fileName)
    {
        var lines = points.Select(p => string.Create(CultureInfo.InvariantCulture, $"{p.X},{p.Y}"));

        return AttachmentInfo.From(fileName).WriteTextLines(new[] { "x,y" }.Concat(lines));
    }
}
