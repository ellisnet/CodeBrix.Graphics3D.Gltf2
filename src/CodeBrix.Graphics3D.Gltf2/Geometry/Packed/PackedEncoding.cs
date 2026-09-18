using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENCODING = CodeBrix.Graphics3D.Gltf2.Schema2.EncodingType;

namespace CodeBrix.Graphics3D.Gltf2.Geometry; //was previously: SharpGLTF.Geometry;

class PackedEncoding
{
    public ENCODING? ColorEncoding;
    public ENCODING? JointsEncoding;
    public ENCODING? WeightsEncoding;

    public void AdjustJointEncoding<TVertex>(IReadOnlyList<TVertex> vertices)
        where TVertex : IVertexBuilder
    {
        if (JointsEncoding.HasValue) return;

        var indices = vertices.Select(item => item.GetSkinning().GetBindings().MaxIndex);
        var maxIndex = indices.Aggregate(0, (a, b) => Math.Max(a, b));
        JointsEncoding = maxIndex < 256 ? ENCODING.UNSIGNED_BYTE : ENCODING.UNSIGNED_SHORT;
    }
}
