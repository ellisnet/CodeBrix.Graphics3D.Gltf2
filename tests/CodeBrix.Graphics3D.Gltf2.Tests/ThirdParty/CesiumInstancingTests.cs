using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using CodeBrix.Graphics3D.Gltf2.Scenes;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using CodeBrix.Graphics3D.Gltf2.Transforms;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.ThirdParty; //was previously: SharpGLTF.ThirdParty;

public class CesiumTests
{
    [Fact]
    public void WriteInstancedGlbWithFeatureIds()
    {
        var modelRoot = ModelRoot.Load(ResourceInfo.From("Kenney/SurvivalKit/tree.glb"));
        var meshBuilder = modelRoot.LogicalMeshes[0].ToMeshBuilder();
        var sceneBuilder = new SceneBuilder();
        var quaternion = Quaternion.CreateFromYawPitchRoll(0, 0, 0);
        var scale = Vector3.One;

        sceneBuilder
            .AddRigidMesh(meshBuilder, new AffineTransform(scale, quaternion, new Vector3(-10, 0, 10)))
            .WithExtras(JsonNode.Parse("{\"_FEATURE_ID_0\":0}"));
        sceneBuilder
            .AddRigidMesh(meshBuilder, new AffineTransform(scale, quaternion, new Vector3(0, 0, 0)))
            .WithExtras(JsonNode.Parse("{\"_FEATURE_ID_0\":1}"));

        var settings = SceneBuilderSchema2Settings.WithGpuInstancing;
        settings.GpuMeshInstancingMinCount = 0;
        var instancedModel = sceneBuilder.ToGltf2(settings);

        instancedModel.LogicalNodes.Count.Should().Be(1);

        var node = instancedModel.LogicalNodes[0];
        var instances = node.GetExtension<MeshGpuInstancing>();
        instances.Should().NotBeNull();

        instances.Accessors.Should().HaveCount(2);
        instances.Accessors.Keys.Should().Contain("TRANSLATION");
        instances.Accessors.Keys.Should().Contain("_FEATURE_ID_0");

        var ids = instances.Accessors["_FEATURE_ID_0"].AsIndicesArray();

        ids.Should().Equal(new uint[] { 0, 1 });

        var dstPath = AttachmentInfo
            .From("instanced_model_with_feature_id.glb")
            .WriteObject(f => instancedModel.SaveGLB(f));
    }
}
