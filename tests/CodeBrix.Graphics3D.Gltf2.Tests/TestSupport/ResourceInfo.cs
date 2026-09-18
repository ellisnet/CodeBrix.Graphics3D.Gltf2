using System;
using System.IO;

namespace CodeBrix.Graphics3D.Gltf2;

/// <summary>
/// A test input file from the <c>Assets/</c> folder that the test project copies
/// next to the test assembly. Paths are written relative to <c>Assets/</c>, with
/// either '/' or '\' as the separator.
/// </summary>
public sealed class ResourceInfo
{
    /// <summary>Creates a resource for <paramref name="relativePath"/> under <c>Assets/</c>.</summary>
    public ResourceInfo(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        FilePath = Path.GetFullPath(Path.Combine(AssetsDirectory, relativePath.Replace('\\', '/')));
    }

    /// <summary>The absolute path of the <c>Assets/</c> folder next to the test assembly.</summary>
    public static string AssetsDirectory => Path.Combine(AppContext.BaseDirectory, "Assets");

    /// <summary>The absolute path of the resource file.</summary>
    public string FilePath { get; }

    /// <summary>The resource file.</summary>
    public FileInfo File => new FileInfo(FilePath);

    /// <summary>Creates a resource for <paramref name="relativePath"/> under <c>Assets/</c>.</summary>
    public static ResourceInfo From(string relativePath) => new ResourceInfo(relativePath);

    /// <summary>Converts the resource to its absolute file path.</summary>
    public static implicit operator string(ResourceInfo resource) => resource?.FilePath;

    /// <inheritdoc />
    public override string ToString() => FilePath;
}
