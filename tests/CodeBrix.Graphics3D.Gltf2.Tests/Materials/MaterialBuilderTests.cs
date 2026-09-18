using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using CodeBrix.Graphics3D.Gltf2.Scenes;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using CodeBrix.Graphics3D.Gltf2.Validation;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Materials; //was previously: SharpGLTF.Materials;

public class MaterialBuilderTests
{
    [Fact]
    public void TestMaterialEquality()
    {
        // Checking if two materials are the same or not is conceptually ambiguous.
        // The static method AreEqualByContent allows to check if two materials represent
        // the same physical material, even if they're two different references.
        // ... And we could use it for general equality checks, but then, since
        // MaterialBuilder is NOT inmutable, it can mean that two materials can be equal
        // at a given time, and non equal at another. Furthermore, it would imply having
        // a hash code that changes over time. As a consequence, using MaterialBuilder as
        // a dictionary key is possible, but dangerous if not carefully handled.

        var srcMaterial = _CreateUnlitMaterial();

        var clnMaterial = srcMaterial.Clone();

        // srcMaterial and clnMaterial are two different objects, so plain equality checks must apply to reference checks
        (clnMaterial != srcMaterial).Should().BeTrue();
        clnMaterial.Should().NotBe(srcMaterial);
        clnMaterial.GetHashCode().Should().NotBe(srcMaterial.GetHashCode());

        // checking the materials represent the same "material" must be made with AreEqualByContent method.
        MaterialBuilder.AreEqualByContent(srcMaterial, clnMaterial).Should().BeTrue();

        var bag = new HashSet<MaterialBuilder>();
        bag.Add(srcMaterial);
        bag.Add(clnMaterial);

        bag.Should().HaveCount(2);
    }

    [Fact]
    public void CreateUnlit()
    {
        var material = _CreateUnlitMaterial();

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();
    }

    private static MaterialBuilder _CreateUnlitMaterial()
    {
        var tex1 = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        var material = new MaterialBuilder("Unlit Material")
            .WithDoubleSide(true) // notice that DoubleSide enables double face rendering. This is an example, but it's usually NOT NECCESARY.
            .WithAlpha(AlphaMode.MASK, 0.7f);

        material.WithUnlitShader()
            .WithBaseColor(tex1, new Vector4(0.7f, 0, 0f, 0.8f));

        return material;
    }

    [Fact]
    public void CreateMetallicRoughness()
    {
        var material = _CreateMetallicRoughnessMaterial();

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();
    }

    private static MaterialBuilder _CreateMetallicRoughnessMaterial()
    {
        var tex1 = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        var material = new MaterialBuilder("Metallic Roughness Material")
            .WithAlpha(AlphaMode.MASK, 0.6f) // Note: this is just an example, for a default opaque material, use WithAlpha(AlphaMode.OPAQUE, 0.5)
            .WithEmissive(tex1, new Vector3(0.2f, 0.3f, 0.1f), 6)
            .WithNormal(tex1, 0.3f)
            .WithOcclusion(tex1, 0.4f);

        material.WithMetallicRoughnessShader()
            .WithBaseColor(tex1, new Vector4(0.7f, 0, 0f, 0.8f))
            .WithMetallicRoughness(tex1, 0.2f, 0.4f);

        // example of setting additional parameters for a given channel.
        material.GetChannel(KnownChannel.BaseColor)
            .Texture
            .WithCoordinateSet(1)
            .WithSampler(TextureWrapMode.CLAMP_TO_EDGE, TextureWrapMode.MIRRORED_REPEAT, TextureMipMapFilter.LINEAR_MIPMAP_LINEAR, TextureInterpolationFilter.NEAREST)
            .WithTransform(Vector2.One * 0.2f, Vector2.One * 0.3f, 0.1f, 2);

        return material;
    }

    [Fact]
    public void CreateMaterialWithExtensions()
    {
        var material = _CreateMetallicRoughnessMaterial();

        // https://github.com/vpenades/SharpGLTF/issues/246
        material.IndexOfRefraction = 7;

        material.Dispersion = 2;

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();

        _AddClearCoat(material);

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();

        _AddVolume(material);

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();

        _AddIridescence(material);

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();

        _AddAnisotropy(material);

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();

        _AddTransmission(material);

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();

        _AddDiffuseTransmission(material);

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();
    }

    [Fact]
    public void TestMaterialWithDDS()
    {
        var material = new MaterialBuilder();

        material
            .UseChannel(KnownChannel.BaseColor)
            .UseTexture()
            .WithPrimaryImage(ResourceInfo.From("PolyHaven/food_apple_01_diff-dxt5.dds").FilePath);

        var gltf = ModelRoot.CreateModel();
        var gltfMaterial = gltf.CreateMaterial(material);

        gltfMaterial.TryGetChannel("BaseColor", out var gltfBaseColor).Should().BeTrue();

        var gltfBaseTexture = gltfBaseColor.Texture;

        gltfBaseTexture.FallbackImage.Should().BeNull(); // regression https://github.com/vpenades/SharpGLTF/issues/296

        AttachmentInfo.From("result.gltf").WriteObject(f => gltf.SaveGLTF(f));

        TestContext.Current.TestOutputHelper?.WriteLine(gltf.GetJsonPreview());
    }

    private static void _AddVolume(MaterialBuilder material)
    {
        var tex1 = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        material.WithVolumeAttenuation(Vector3.One * 0.3f, 0.6f)
            .WithVolumeThickness(tex1, 0.4f);
    }

    private static void _AddClearCoat(MaterialBuilder material)
    {
        var tex1 = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        material.WithClearCoat(tex1, 0.9f)
            .WithClearCoatNormal(tex1)
            .WithClearCoatRoughness(tex1, 0.9f);
    }

    private static void _AddIridescence(MaterialBuilder material)
    {
        material
            .WithIridescence(default, 0.2f, 1.5f)
            .WithIridescenceThickness(default, 120, 330);
    }

    private static void _AddAnisotropy(MaterialBuilder material)
    {
        var tex1 = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        material.WithAnisotropy(tex1, 0.2f, 3f);
    }

    private static void _AddTransmission(MaterialBuilder material)
    {
        var tex1 = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        material.WithTransmission(tex1, 0.9f);
    }

    private static void _AddDiffuseTransmission(MaterialBuilder material)
    {
        var tex1 = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        material.WithDiffuseTransmissionFactor(tex1, 0.9f);
        material.WithDiffuseTransmissionColor(tex1, Vector3.One * 0.2f);
    }

    private static MaterialBuilder _Schema2Roundtrip(MaterialBuilder srcMaterial)
    {
        // converts a MaterialBuilder to a Schema2.Material and back to a MaterialBuilder

        var dstModel = Schema2.ModelRoot.CreateModel();
        var dstMaterial = dstModel.CreateMaterial(srcMaterial.Name);

        srcMaterial.CopyTo(dstMaterial); // copy MaterialBuilder to Schema2.Material.

        var ctx = new ValidationResult(dstModel,ValidationMode.Strict, true);
        dstModel.ValidateReferences(ctx.GetContext());
        dstModel.ValidateContent(ctx.GetContext());

        var rtpMaterial = new MaterialBuilder(dstMaterial.Name);

        dstMaterial.CopyTo(rtpMaterial);// copy Schema2.Material to MaterialBuilder.

        return rtpMaterial;
    }

    #region obsolete (kept for backwards compatibility)

    [Fact]
    public void CreateSpecularGlossiness()
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        var material = _CreateSpecularGlossinessMaterial();
        #pragma warning restore CS0618 // Type or member is obsolete

        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();
    }

    [Fact]
    public void CreateSpecularGlossinessWithFallback()
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        var material = _CreateSpecularGlossinessMaterialWithFallback();
        #pragma warning restore CS0618 // Type or member is obsolete

        // check
        MaterialBuilder.AreEqualByContent(material, _Schema2Roundtrip(material)).Should().BeTrue();
        MaterialBuilder.AreEqualByContent(material, material.Clone()).Should().BeTrue();
    }

    [Obsolete("SpecularGlossiness has been deprecated by Khronos")]
    private static MaterialBuilder _CreateSpecularGlossinessMaterialWithFallback()
    {

        var tex1 = ResourceInfo.From("PolyHaven/food_apple_01_diff.webp").FilePath;
        var tex2 = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        var primary = new MaterialBuilder("primary")

            // fallback and primary material must have exactly the same properties
            .WithDoubleSide(true)
            .WithAlpha(AlphaMode.MASK, 0.75f)
            .WithEmissive(tex1, new Vector3(0.2f, 0.3f, 0.1f), 4)
            .WithNormal(tex1, 0.3f)
            .WithOcclusion(tex1, 0.4f);

        // primary must use Specular Glossiness shader.
        primary.WithSpecularGlossinessShader()
            .WithDiffuse(tex1, new Vector4(0.7f, 0, 0f, 1.0f))
            .WithSpecularGlossiness(tex1, new Vector3(0.7f, 0, 0f), 0.8f);

        // set fallback textures for engines that don't support WEBP texture format
        primary.GetChannel(KnownChannel.Normal).Texture.FallbackImage = tex2;
        primary.GetChannel(KnownChannel.Emissive).Texture.FallbackImage = tex2;
        primary.GetChannel(KnownChannel.Occlusion).Texture.FallbackImage = tex2;
        primary.GetChannel(KnownChannel.Diffuse).Texture.FallbackImage = tex2;
        primary.GetChannel(KnownChannel.SpecularGlossiness).Texture.FallbackImage = tex2;

        // set fallback material for engines that don't support Specular Glossiness shader.
        primary.WithMetallicRoughnessFallback(tex1, new Vector4(0.7f, 0, 0, 1), String.Empty, 0.6f, 0.7f);
        primary.CompatibilityFallback.GetChannel(KnownChannel.BaseColor).Texture.FallbackImage = tex2;

        return primary;
    }

    [Obsolete("SpecularGlossiness has been deprecated by Khronos")]
    private static MaterialBuilder _CreateSpecularGlossinessMaterial()
    {

        var tex1 = ResourceInfo.From("PolyHaven/food_apple_01_diff.png").FilePath;

        var material = new MaterialBuilder()
            .WithAlpha(AlphaMode.MASK, 0.6f)
            .WithEmissive(tex1, new Vector3(0.2f, 0.3f, 0.1f))
            .WithNormal(tex1, 0.3f)
            .WithOcclusion(tex1, 0.4f);

        material.WithSpecularGlossinessShader()
            .WithDiffuse(tex1, new Vector4(0.7f, 0, 0f, 0.8f))
            .WithSpecularGlossiness(tex1, new Vector3(0.7f, 0, 0f), 0.8f);

        return material;
    }

    #endregion
}
