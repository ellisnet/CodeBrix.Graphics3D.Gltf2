using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Materials; //was previously: SharpGLTF.Materials;

public class MaterialTypesTests
{
    [Fact]
    public void CheckKnownPropertyEnum()
    {
        var knownPropertyValues = Enum.GetNames(typeof(KnownProperty));
        var schemaPropertyValues = Enum.GetNames(typeof(Schema2._MaterialParameterKey));

        schemaPropertyValues.Should().Equal(knownPropertyValues);
    }
}
