using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Reflection; //was previously: SharpGLTF.Reflection;

public class ReflectionTests
{
    [Theory]
    [InlineData("AnimationPointerUVs")]
    public void ReflectionDump(string modelName)
    {
        var mpath = TestFiles.GetSampleModelsPaths().FirstOrDefault(item => item.Contains(modelName));

        var model = ModelRoot.Load(mpath, Validation.ValidationMode.TryFix);

        _DumpReflectionItem(string.Empty, string.Empty, model);
    }

    private static void _DumpReflectionItem(string indent, string name, Object value)
    {
        if (value == null) return;

        TestContext.Current.TestOutputHelper?.Write($"{indent}{name}:");

        switch(value)
        {
            case IReflectionObject reflectionObject:
                TestContext.Current.TestOutputHelper?.WriteLine(string.Empty);
                indent += "    ";
                foreach (var field in reflectionObject.GetFields())
                {
                    _DumpReflectionItem(indent, field.Name, field.Value);
                }
                break;

            case IConvertible convertible:
                TestContext.Current.TestOutputHelper?.WriteLine($"{value}");
                break;

            case IEnumerable enumerable:
                TestContext.Current.TestOutputHelper?.WriteLine(string.Empty);
                indent += "    ";
                foreach (var item in enumerable)
                {
                    TestContext.Current.TestOutputHelper?.WriteLine($"{indent}{value}");
                }
                break;

            default:
                TestContext.Current.TestOutputHelper?.WriteLine($"{value}");
                break;
        }
    }

    [Theory]
    [InlineData("AnimationPointerUVs", "/materials/61/extensions/KHR_materials_specular/specularTexture/extensions/KHR_texture_transform/offset", "<-0.2388889, 0.2388889>")]
    [InlineData("AnimationPointerUVs", "/materials/25/extensions/KHR_materials_anisotropy/anisotropyTexture/extensions/KHR_texture_transform/offset", "<-0.2388889, 0.2388889>")]
    [InlineData("AnimationPointerUVs", "/materials/1/extensions/KHR_materials_diffuse_transmission/diffuseTransmissionTexture/extensions/KHR_texture_transform/offset", "<-0.2388889, 0.2388889>")]
    public void ReflectionPointerPathTest(string modelName, string pointerPath, string expectedValue)
    {
        var mpath = TestFiles.GetSampleModelsPaths()
            .Where(item => !item.Contains("Quantized"))
            .FirstOrDefault(item => item.Contains(modelName));

        var model = ModelRoot.Load(mpath, Validation.ValidationMode.TryFix);

        var field = Reflection.FieldInfo.From(model, pointerPath);

        var result = FormattableString.Invariant($"{field.Value}");

        result.Should().Be(expectedValue);
    }
}
