using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeBrix.Graphics3D.Gltf2; //was previously: SharpGLTF;

/// <summary>
/// Centralices the access to test files.
/// </summary>
/// <remarks>
/// Every file lives in the test project's <c>Assets/</c> folder; nothing is
/// downloaded or read from outside the repository. The sample models are a
/// pinned subset of the Khronos glTF-Sample-Assets repository (see
/// THIRD-PARTY-NOTICES.txt).
/// </remarks>
public static class TestFiles
{
    #region data

    private static DirectoryInfo _KhronosSampleModelsDir => new DirectoryInfo(Path.Combine(ResourceInfo.AssetsDirectory, "KhronosSampleAssets", "Models"));

    private static DirectoryInfo _BabylonJsMeshesDir => new DirectoryInfo(Path.Combine(ResourceInfo.AssetsDirectory, "BabylonJS", "meshes"));

    #endregion

    #region API

    public static string GetKhronosAssetDir(params string[] path)
    {
        return Path.Combine(_KhronosSampleModelsDir.FullName, Path.Combine(path));
    }

    public static IReadOnlyList<string> GetSampleModelsPaths()
    {
        return _FindModelInDirectory(_KhronosSampleModelsDir);
    }

    public static IReadOnlyList<string> GetBabylonJSModelsPaths()
    {
        return _FindModelInDirectory(_BabylonJsMeshesDir);
    }

    public static IEnumerable<string> GetMeshIntancingModelPaths()
    {
        return GetBabylonJSModelsPaths()
            .Where(item => item.ToUpperInvariant().Contains("TEAPOT"));
    }

    private static IReadOnlyList<string> _FindModelInDirectory(DirectoryInfo dinfo)
    {
        var gltf = Directory.GetFiles(dinfo.FullName, "*.gltf", SearchOption.AllDirectories);
        var glbb = Directory.GetFiles(dinfo.FullName, "*.glb", SearchOption.AllDirectories);

        return gltf.Concat(glbb).OrderBy(item => item, StringComparer.Ordinal).ToList();
    }

    #endregion
}
