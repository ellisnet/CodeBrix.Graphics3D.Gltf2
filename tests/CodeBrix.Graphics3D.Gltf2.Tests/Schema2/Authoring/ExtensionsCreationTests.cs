using System.Numerics;
using SilverAssertions;
using Xunit;
using VPOS = CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexPosition;
using VTEX = CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexTexture1;

namespace CodeBrix.Graphics3D.Gltf2.Schema2.Authoring; //was previously: SharpGLTF.Schema2.Authoring;

public class ExtensionsCreationTests
{
    #region setup

    public ExtensionsCreationTests()
    {
        // TestFiles.DownloadReferenceModels();
    }

    #endregion

    [Fact]
    public void CreateSceneWithWithLightsExtension()
    {

        var root = ModelRoot.CreateModel();
        var scene = root.UseScene("Empty Scene");

        scene.CreateNode()
            .PunctualLight = root.CreatePunctualLight(PunctualLightType.Directional)
            .WithColor(Vector3.UnitX, 2);

        scene.CreateNode()
            .PunctualLight = root.CreatePunctualLight(PunctualLightType.Spot)
            .WithColor(Vector3.UnitY, 3, 10)
            .WithSpotCone(0.2f, 0.3f);

        root.AttachToCurrentTest("sceneWithLight.gltf");
        root.AttachToCurrentTest("sceneWithLight.glb");
    }

    [Fact]
    public void CreateSceneWithSpecularGlossinessExtension()
    {

        var basePath = System.IO.Path.Combine(ResourceInfo.AssetsDirectory, "PolyHaven");

        // first, create a default material
        var material = new Materials.MaterialBuilder("material1")
            .WithMetallicRoughnessShader()
            .WithChannelImage(Materials.KnownChannel.Normal, System.IO.Path.Combine(basePath, "food_apple_01_nor_gl.png"))
            .WithChannelImage(Materials.KnownChannel.Emissive, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.Occlusion, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.BaseColor, System.IO.Path.Combine(basePath, "food_apple_01_diff.png"))
            .WithChannelImage(Materials.KnownChannel.MetallicRoughness, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"));

        // wrap the fallback material with a PBR Specular Glossiness material.
        #pragma warning disable CS0618 // explicitly calling obsolete methods for the sake of testing SpecularGlossiness
        material = new Materials.MaterialBuilder("material1")
            .WithFallback(material)
            .WithSpecularGlossinessShader()
            .WithChannelImage(Materials.KnownChannel.Normal, System.IO.Path.Combine(basePath, "food_apple_01_nor_gl.png"))
            .WithChannelImage(Materials.KnownChannel.Emissive, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.Occlusion, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.Diffuse, System.IO.Path.Combine(basePath, "food_apple_01_diff.png"))
            .WithChannelImage(Materials.KnownChannel.SpecularGlossiness, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"));
        #pragma warning restore CS0618 // Type or member is obsolete

        var mesh = new Geometry.MeshBuilder<VPOS, VTEX>("mesh1");
        mesh.UsePrimitive(material).AddQuadrangle
            ((new Vector3(-10, 10, 0), new Vector2(1, 0))
            , (new Vector3(10, 10, 0), new Vector2(0, 0))
            , (new Vector3(10, -10, 0), new Vector2(0, 1))
            , (new Vector3(-10, -10, 0), new Vector2(1, 1))
            );

        var scene = new Scenes.SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);

        scene.AttachToCurrentTest("result.glb");
        scene.AttachToCurrentTest("result.gltf");
    }

    [Fact]
    public void CreateSceneWithClearCoatExtension()
    {

        var basePath = System.IO.Path.Combine(ResourceInfo.AssetsDirectory, "PolyHaven");

        // first, create a default material
        var material = new Materials.MaterialBuilder("material")
            .WithMetallicRoughnessShader()
            .WithChannelImage(Materials.KnownChannel.Normal, System.IO.Path.Combine(basePath, "food_apple_01_nor_gl.png"))
            .WithChannelImage(Materials.KnownChannel.Emissive, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.Occlusion, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.BaseColor, System.IO.Path.Combine(basePath, "food_apple_01_diff.png"))
            .WithChannelImage(Materials.KnownChannel.MetallicRoughness, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.ClearCoat, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelParam(Materials.KnownChannel.ClearCoat, Materials.KnownProperty.ClearCoatFactor, 0.5f)
            .WithChannelImage(Materials.KnownChannel.ClearCoatRoughness, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.ClearCoatNormal, System.IO.Path.Combine(basePath, "food_apple_01_nor_gl.png"));

        var mesh = new Geometry.MeshBuilder<VPOS, VTEX>("mesh1");
        mesh.UsePrimitive(material).AddQuadrangle
            ((new Vector3(-10, 10, 0), new Vector2(1, 0))
            , (new Vector3(10, 10, 0), new Vector2(0, 0))
            , (new Vector3(10, -10, 0), new Vector2(0, 1))
            , (new Vector3(-10, -10, 0), new Vector2(1, 1))
            );

        var scene = new Scenes.SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);

        var gltf2 = scene.ToGltf2();
        var clearCoatFactor = gltf2.LogicalMaterials[0].FindChannel("ClearCoat").Value.GetFactor("ClearCoatFactor");
        clearCoatFactor.Should().Be(0.5f);

        scene.AttachToCurrentTest("result.glb");
        scene.AttachToCurrentTest("result.gltf");
    }

    [Fact]
    public void CreateSceneWithTransmissionExtension()
    {

        var basePath = System.IO.Path.Combine(ResourceInfo.AssetsDirectory, "PolyHaven");

        var material = new Materials.MaterialBuilder("material")
            .WithMetallicRoughnessShader()
            .WithChannelImage(Materials.KnownChannel.Normal, System.IO.Path.Combine(basePath, "food_apple_01_nor_gl.png"))
            .WithChannelImage(Materials.KnownChannel.Emissive, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.Occlusion, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.BaseColor, System.IO.Path.Combine(basePath, "food_apple_01_diff.png"))
            .WithChannelImage(Materials.KnownChannel.MetallicRoughness, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.Transmission, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelParam(Materials.KnownChannel.Transmission, Materials.KnownProperty.TransmissionFactor, 0.75f);

        var mesh = new Geometry.MeshBuilder<VPOS, VTEX>("mesh1");
        mesh.UsePrimitive(material).AddQuadrangle
            ((new Vector3(-10, 10, 0), new Vector2(1, 0))
            , (new Vector3(10, 10, 0), new Vector2(0, 0))
            , (new Vector3(10, -10, 0), new Vector2(0, 1))
            , (new Vector3(-10, -10, 0), new Vector2(1, 1))
            );

        var scene = new Scenes.SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);

        var gltf2 = scene.ToGltf2();
        var transmissionFactor = gltf2.LogicalMaterials[0].FindChannel("Transmission").Value.GetFactor("TransmissionFactor");
        transmissionFactor.Should().Be(0.75f);

        scene.AttachToCurrentTest("result.glb");
        scene.AttachToCurrentTest("result.gltf");
    }

    [Fact]
    public void CreateSceneWithsSheenExtension()
    {

        var basePath = System.IO.Path.Combine(ResourceInfo.AssetsDirectory, "PolyHaven");

        var material = new Materials.MaterialBuilder("material")
            .WithMetallicRoughnessShader()
            .WithChannelImage(Materials.KnownChannel.Normal, System.IO.Path.Combine(basePath, "food_apple_01_nor_gl.png"))
            .WithChannelImage(Materials.KnownChannel.Emissive, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.Occlusion, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.BaseColor, System.IO.Path.Combine(basePath, "food_apple_01_diff.png"))
            .WithChannelImage(Materials.KnownChannel.MetallicRoughness, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelImage(Materials.KnownChannel.SheenColor, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelParam(Materials.KnownChannel.SheenColor, Materials.KnownProperty.RGB, Vector3.One)
            .WithChannelImage(Materials.KnownChannel.SheenRoughness, System.IO.Path.Combine(basePath, "food_apple_01_rough.png"))
            .WithChannelParam(Materials.KnownChannel.SheenRoughness, Materials.KnownProperty.RoughnessFactor, 0.5f);

        var mesh = new Geometry.MeshBuilder<VPOS, VTEX>("mesh1");
        mesh.UsePrimitive(material).AddQuadrangle
            ((new Vector3(-10, 10, 0), new Vector2(1, 0))
            , (new Vector3(10, 10, 0), new Vector2(0, 0))
            , (new Vector3(10, -10, 0), new Vector2(0, 1))
            , (new Vector3(-10, -10, 0), new Vector2(1, 1))
            );

        var scene = new Scenes.SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.Identity);

        var gltf2 = scene.ToGltf2();
        var sheenColorFactor = gltf2.LogicalMaterials[0].FindChannel("SheenColor").Value.Color;
        sheenColorFactor.Should().Be(Vector4.One);

        var sheenRoughnessFactor = gltf2.LogicalMaterials[0].FindChannel("SheenRoughness").Value.GetFactor("RoughnessFactor");
        sheenRoughnessFactor.Should().Be(0.5f);

        scene.AttachToCurrentTest("result.glb");
        scene.AttachToCurrentTest("result.gltf");
    }

    [Theory]
    [InlineData("PolyHaven/food_apple_01_diff-dxt5.dds")]
    [InlineData("PolyHaven/food_apple_01_diff.webp")]
    [InlineData("KhronosSampleAssets/Models/FlightHelmet/glTF-KTX-BasisU/FlightHelmet_Materials_LensesMat_Normal.ktx2")]
    public void CreateSceneWithTextureImageExtension(string textureFileName)
    {

        // first, create a default material
        var material = new Materials.MaterialBuilder("material1")
            .WithDoubleSide(true)
            .WithMetallicRoughnessShader()
            .WithChannelImage(Materials.KnownChannel.BaseColor, ResourceInfo.From(textureFileName).FilePath);

        var mesh = new Geometry.MeshBuilder<VPOS, VTEX>("mesh1");

        mesh
            .UsePrimitive(material)
            .AddQuadrangle
            ((new Vector3(-10, 10, 0), new Vector2(1, 0))
            , (new Vector3(10, 10, 0), new Vector2(0, 0))
            , (new Vector3(10, -10, 0), new Vector2(0, 1))
            , (new Vector3(-10, -10, 0), new Vector2(1, 1))
            );

        var model = ModelRoot.CreateModel();

        model.CreateMeshes(mesh);

        model.UseScene("Default")
            .CreateNode("RootNode")
            .WithMesh(model.LogicalMeshes[0]);

        model.AttachToCurrentTest("result_wf.obj");
        model.AttachToCurrentTest("result_glb.glb");
        model.AttachToCurrentTest("result_gltf.gltf");
    }

    [Fact]
    public void CrateSceneWithTextureTransformExtension()
    {

        // first, create a default material
        var material = new Materials.MaterialBuilder("material1")
            .WithDoubleSide(true)
            .WithMetallicRoughnessShader()
            .WithChannelImage(Materials.KnownChannel.BaseColor, ResourceInfo.From("PolyHaven/food_apple_01_diff.jpg").FilePath);

        material.GetChannel(Materials.KnownChannel.BaseColor).UseTexture().WithTransform(0.40f,0.25f, 0.5f,0.5f);

        var mesh = new Geometry.MeshBuilder<VPOS, VTEX>("mesh1");

        mesh
            .UsePrimitive(material)
            .AddQuadrangle
            ((new Vector3(-10, 10, 0), new Vector2(1, 0))
            , (new Vector3(10, 10, 0), new Vector2(0, 0))
            , (new Vector3(10, -10, 0), new Vector2(0, 1))
            , (new Vector3(-10, -10, 0), new Vector2(1, 1))
            );

        var model = ModelRoot.CreateModel();

        model.CreateMeshes(mesh);

        model.UseScene("Default")
            .CreateNode("RootNode")
            .WithMesh(model.LogicalMeshes[0]);

        model.AttachToCurrentTest("result_glb.glb");
        model.AttachToCurrentTest("result_gltf.gltf");
    }
}
