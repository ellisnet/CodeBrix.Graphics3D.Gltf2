using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2;

/// <summary>
/// A file the running test writes as evidence of what it produced (a saved
/// .glb/.gltf/.obj, a text dump, ...). Files land under
/// <c>TestResults/&lt;TestClass&gt;/&lt;TestMethod&gt;[_&lt;row&gt;]/</c> next to the
/// test assembly, so they are easy to find and open after a run.
/// </summary>
public sealed class AttachmentInfo
{
    private AttachmentInfo(FileInfo file)
    {
        File = file;
    }

    /// <summary>The file this attachment writes to.</summary>
    public FileInfo File { get; }

    /// <summary>Creates an attachment named <paramref name="fileName"/> for the running test.</summary>
    public static AttachmentInfo From(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var testCase = TestContext.Current.TestCase;
        var className = _Sanitize(testCase?.TestClassSimpleName ?? "NoTestClass");
        var methodName = _Sanitize(testCase?.TestMethodName ?? "NoTestMethod");

        // Theory rows share a method name; a short hash of the row's display
        // name keeps each row's files apart.
        var displayName = testCase?.TestCaseDisplayName ?? string.Empty;
        if (displayName.Contains('('))
        {
            var hash = SHA1.HashData(Encoding.UTF8.GetBytes(displayName));
            methodName += "_" + Convert.ToHexString(hash, 0, 4).ToLowerInvariant();
        }

        var dir = Path.Combine(AppContext.BaseDirectory, "TestResults", className, methodName);
        return new AttachmentInfo(new FileInfo(Path.Combine(dir, fileName.Replace('\\', '/'))));
    }

    /// <summary>
    /// Writes the file by passing its full path to <paramref name="writer"/>, and
    /// returns it.
    /// </summary>
    public FileInfo WriteObject(Action<string> writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        File.Directory.Create();
        writer(File.FullName);
        File.Refresh();
        return File;
    }

    /// <summary>Writes <paramref name="text"/> to the file and returns it.</summary>
    public FileInfo WriteAllText(string text)
        => WriteObject(path => System.IO.File.WriteAllText(path, text));

    /// <summary>Writes <paramref name="lines"/> to the file and returns it.</summary>
    public FileInfo WriteTextLines(IEnumerable<string> lines)
        => WriteObject(path => System.IO.File.WriteAllLines(path, lines));

    /// <summary>Writes <paramref name="bytes"/> to the file and returns it.</summary>
    public FileInfo WriteAllBytes(IEnumerable<byte> bytes)
        => WriteObject(path => System.IO.File.WriteAllBytes(path, bytes.ToArray()));

    private static string _Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }
}
