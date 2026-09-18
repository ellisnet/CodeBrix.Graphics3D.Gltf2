using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Runtime; //was previously: SharpGLTF.Runtime;

public class ExtensionTests
{
    [Fact]
    public void TestAnimatedVisibility()
    {
        var modelPath = TestFiles.GetSampleModelsPaths()
                            .FirstOrDefault(item => item.Contains("CubeVisibility.glb"));

        var model = Schema2.ModelRoot.Load(modelPath);

        var vinput = model.LogicalAccessors[3].AsScalarArray();
        var voutput = model.LogicalAccessors[4].AsIndexArray();
        var vtrack = vinput.Zip(voutput, (i,o) => (i,o));

        var sceneTemplate = SceneTemplate.Create(model.DefaultScene);
        var sceneInstance = sceneTemplate.CreateInstance();

        foreach(var (time,val) in vtrack)
        {
            sceneInstance.Armature.SetAnimationFrame(0, time);

            TestContext.Current.TestOutputHelper?.WriteLine($"Time:{time} Value:{val}");

            foreach (var nodeInst in sceneInstance.Armature.LogicalNodes)
            {
                TestContext.Current.TestOutputHelper?.WriteLine($"{nodeInst.Name} {nodeInst.IsVisible}");
            }

            var drawables = sceneInstance.AsEnumerable().ToArray();

            TestContext.Current.TestOutputHelper?.WriteLine($"count: {drawables.Length}");
            TestContext.Current.TestOutputHelper?.WriteLine(string.Empty);

            drawables.Length.Should().Be(val != 0 ? 2 : 1);

            // evaluate triangles:

            AttachmentInfo.From($"animated_frame_{time}.obj").WriteObject(f => model.SaveAsWavefront(f, model.LogicalAnimations[0], time));
        }
    }
}
