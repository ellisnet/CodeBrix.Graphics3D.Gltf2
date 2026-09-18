using System;
using System.Collections.Generic;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Schema2.LoadAndSave; //was previously: SharpGLTF.Schema2.LoadAndSave;

public class LoadInvalidTests
{
    [Fact]
    public void LoadInvalidJsonModel()
    {
        var path = ResourceInfo.From("Invalid_Json.gltf").FilePath;

        Assert.Throws<Validation.SchemaException>(() => ModelRoot.Load(path));

        var validation = ModelRoot.Validate(path);

        validation.HasErrors.Should().BeTrue();
    }
}
