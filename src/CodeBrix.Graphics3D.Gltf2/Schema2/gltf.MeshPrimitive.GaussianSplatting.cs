using System;
using System.Collections.Generic;
using System.Text;

namespace CodeBrix.Graphics3D.Gltf2.Schema2; //was previously: SharpGLTF.Schema2;

[System.Diagnostics.CodeAnalysis.Experimental("GLTFRT1002")]
public partial class GaussianSplatting
{
    public GaussianSplatting(MeshPrimitive parent)
    {
        _Parent = parent;
    }

    private MeshPrimitive _Parent;

    public string Kernel
    {
        get => _kernel;
        set => _kernel = value;
    }

    public string Projection
    {
        get => _projection;
        set => _projection = value;
    }

    public string ColorSpace
    {
        get => _colorSpace;
        set => _colorSpace = value;
    }

    public string SortingMethod
    {
        get => _sortingMethod;
        set => _sortingMethod = value;
    }
}
