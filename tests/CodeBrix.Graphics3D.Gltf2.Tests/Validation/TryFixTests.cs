using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Validation; //was previously: SharpGLTF.Validation;

public class TryFixTests
{
    /*
    [Fact]
    public void TryFixAnimationWithInvalidByteStride()
    {
        var path = System.IO.Path.Combine(TestFiles.RootDirectory, "teascroll_clubhouse_-_fountain_prop", "scene.gltf");

        Assert.Throws<LinkException>(() => Schema2.ModelRoot.Load(path));

        var mdl = Schema2.ModelRoot.Load(path, ValidationMode.TryFix);

        var vcontext = new ValidationResult(mdl, ValidationMode.Strict, true);
        mdl.ValidateReferences(vcontext.GetContext());
        mdl.Validate(vcontext.GetContext());

        mdl.WriteGLB();
    }*/

}
