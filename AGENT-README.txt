================================================================================
AGENT-README: CodeBrix.Graphics3D.Gltf2
A Guide for AI Coding Agents - CONSUMING the
CodeBrix.Graphics3D.Gltf2.MitLicenseForever NuGet package
================================================================================

OVERVIEW
========
CodeBrix.Graphics3D.Gltf2 is a fully managed library for .NET 10 or later that
reads, writes and builds Khronos glTF 2.0 3D models - both the JSON form
(.gltf, with satellite or embedded buffers and images) and the binary form
(.glb). It has three layers, all in ONE assembly:

  * The document model (namespace ...Schema2): a complete, low-level object
    model of a glTF 2.0 document - ModelRoot, Scene, Node, Mesh, MeshPrimitive,
    Accessor, Buffer, BufferView, Material, Texture, Image, TextureSampler,
    Skin, Camera, Animation, PunctualLight - plus loading, saving and
    validation.
  * The toolkit (namespaces ...Geometry, ...Materials, ...Scenes and extension
    methods in ...Schema2): builders for authoring models from code -
    MeshBuilder, MaterialBuilder, NodeBuilder, SceneBuilder - and helpers that
    convert between the document model and the builders.
  * The runtime helpers (namespace ...Runtime): SceneTemplate / SceneInstance
    and MeshDecoder, which turn a loaded document into what a renderer needs -
    decoded vertex streams, per-instance rigid, skinned or GPU-instanced
    transforms, and animation playback.

Provenance: this library is a port of SharpGLTF, merging the three upstream
packages SharpGLTF.Core, SharpGLTF.Runtime and SharpGLTF.Toolkit into this one
package with an identical public API under renamed namespaces. To migrate code
written against those packages, replace all three package references with
CodeBrix.Graphics3D.Gltf2.MitLicenseForever and rename the namespace root:

    SharpGLTF[.Sub]  ->  CodeBrix.Graphics3D.Gltf2[.Sub]

    e.g. SharpGLTF.Schema2  -> CodeBrix.Graphics3D.Gltf2.Schema2
         SharpGLTF.Scenes   -> CodeBrix.Graphics3D.Gltf2.Scenes
         SharpGLTF.Runtime  -> CodeBrix.Graphics3D.Gltf2.Runtime

Type names, member names and behaviour are otherwise unchanged. Two visible
differences: saved models write "CodeBrix.Graphics3D.Gltf2 <version>" as
asset.generator, and relative URIs that use '\' as a separator resolve on
Linux and macOS as well as Windows. Do NOT use the SharpGLTF namespaces - they
do not exist in this package.


INSTALLATION
============
PackageId:  CodeBrix.Graphics3D.Gltf2.MitLicenseForever

    dotnet add package CodeBrix.Graphics3D.Gltf2.MitLicenseForever

IMPORTANT: the NuGet package id is CodeBrix.Graphics3D.Gltf2.MitLicenseForever
(NOT "CodeBrix.Graphics3D.Gltf2" - the suffix makes the license obvious
forever). The assembly and the namespace root are CodeBrix.Graphics3D.Gltf2.

NuGet dependencies: none. JSON handling uses the in-box System.Text.Json.

License: MIT (SPDX: MIT)

Requirements: .NET 10 or later, any OS. The assembly is marked trim- and
AOT-compatible. It uses unsafe code internally; consumers do not need
<AllowUnsafeBlocks>.


KEY NAMESPACES / USINGS
=======================
    CodeBrix.Graphics3D.Gltf2.Schema2
        ModelRoot and the document model; ReadSettings, WriteSettings,
        ReadContext, WriteContext; the "Toolkit" extension methods
    CodeBrix.Graphics3D.Gltf2.Scenes
        SceneBuilder, NodeBuilder, InstanceBuilder, CameraBuilder, LightBuilder
    CodeBrix.Graphics3D.Gltf2.Geometry
        MeshBuilder, PrimitiveBuilder, VertexBuilder
    CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes
        VertexPosition, VertexPositionNormal, VertexTexture1, VertexJoints4,
        VertexEmpty, ...
    CodeBrix.Graphics3D.Gltf2.Materials
        MaterialBuilder, KnownChannel, ImageBuilder, ChannelBuilder
    CodeBrix.Graphics3D.Gltf2.Runtime
        SceneTemplate, SceneInstance, MeshDecoder, RuntimeOptions
    CodeBrix.Graphics3D.Gltf2.Transforms
        AffineTransform, RigidTransform, SkinnedTransform, InstancingTransform,
        SparseWeight8
    CodeBrix.Graphics3D.Gltf2.Memory
        MemoryImage and the typed accessor arrays
    CodeBrix.Graphics3D.Gltf2.Animations
        curve sampling
    CodeBrix.Graphics3D.Gltf2.Validation
        ValidationMode, ModelException and its subclasses
    CodeBrix.Graphics3D.Gltf2.IO
        ZipReader, ZipWriter

Most programs need Schema2, plus Scenes/Geometry/Geometry.VertexTypes/Materials
to author models, or Runtime/Transforms to render them. Vector and matrix types
are System.Numerics (Vector2, Vector3, Vector4, Quaternion, Matrix4x4).


CORE API REFERENCE
==================

ModelRoot (CodeBrix.Graphics3D.Gltf2.Schema2) - the glTF document
-----------------------------------------------------------------
Create / load:
    static ModelRoot CreateModel()
    static ModelRoot Load(string filePath, ReadSettings settings = null)
    static ModelRoot ParseGLB(ArraySegment<byte> glb, ReadSettings settings = null)
    static ModelRoot ReadGLB(Stream stream, ReadSettings settings = null)
    static string[]  GetSatellitePaths(string filePath)   // .bin / image files
    static ValidationResult Validate(string filePath)

  Load() detects .gltf vs .glb from the content. A ValidationMode converts
  implicitly to ReadSettings, so Load(path, ValidationMode.TryFix) works.

Save:
    void SaveGLB(string filePath, WriteSettings settings = null)
    void SaveGLTF(string filePath, WriteSettings settings = null)
    void Save(string filePath, WriteSettings settings = null)  // by extension
    ArraySegment<byte> WriteGLB(WriteSettings settings = null)
    void WriteGLB(Stream stream, WriteSettings settings = null)

Document-level members:
    Asset Asset                     // generator, copyright, version
    Scene DefaultScene { get; set; }
    Scene UseScene(int index)       // gets or creates
    Scene UseScene(string name)
    IEnumerable<string> ExtensionsUsed, ExtensionsRequired
    ModelRoot DeepClone()
    void ApplyBasisTransform(Matrix4x4 basis, string basisNodeName = ...)
    void MergeBuffers(); void MergeBuffers(int maxSize); void MergeImages()

  Logical collections - every object of each kind, in document order:
    LogicalScenes, LogicalNodes, LogicalMeshes, LogicalMaterials,
    LogicalTextures, LogicalImages, LogicalTextureSamplers, LogicalAccessors,
    LogicalBuffers, LogicalBufferViews, LogicalSkins, LogicalCameras,
    LogicalAnimations, LogicalPunctualLights

  Factory methods (low-level authoring):
    CreateMesh(name), CreateMaterial(name), CreateAnimation(name),
    CreateSkin(name), CreateCamera(name), CreatePunctualLight(type),
    CreateAccessor(name), CreateBuffer(size), UseImage(MemoryImage),
    UseTexture(image, sampler), UseTextureSampler(...)

Two ways to traverse a document:
  * Logical: model.LogicalMeshes[i], model.LogicalNodes[i], ... - every object
    as stored in the file.
  * Visual: model.DefaultScene.VisualChildren, then node.VisualChildren
    recursively - the scene graph as rendered.

Reading options - ReadSettings / ReadContext
--------------------------------------------
    ReadSettings { Validation, JsonPreprocessor, ImageDecoder }
    ValidationMode: Skip, TryFix, Strict (default is Strict)
    ReadContext.CreateFromDirectory(DirectoryInfo)       // satellite files
    ReadContext.CreateFromDictionary(IReadOnlyDictionary<string, ArraySegment<byte>>)
    ReadContext.Create(FileReaderCallback)               // your own reader
    ModelRoot.Load("name.gltf", readContext)             // ReadContext IS a
                                                         // ReadSettings

  JsonPreprocessor is a string -> string hook that runs on the JSON before
  parsing - use it to repair known-bad files.

Writing options - WriteSettings / WriteContext
----------------------------------------------
    WriteSettings { JsonIndented, JsonOptions, MergeBuffers, BuffersMaxSize,
                    ImageWriting, ImageWriteCallback, JsonPostprocessor,
                    Validation }
    ResourceWriteMode (ImageWriting): Default, SatelliteFile, EmbeddedAsBase64,
                                      BufferView
    WriteContext.CreateFromDictionary(IDictionary<string, ArraySegment<byte>>)

  ImageWriting controls IMAGES only. Buffers of a .gltf are always written as
  satellite .bin files (a .glb embeds everything).

Scenes and nodes
----------------
    Scene: string Name; IEnumerable<Node> VisualChildren;
           Node CreateNode(string name = null)
    Node:  Node CreateNode(string name = null)
           Mesh Mesh; Skin Skin; Camera Camera; PunctualLight PunctualLight
           AffineTransform LocalTransform; Matrix4x4 LocalMatrix
           Matrix4x4 WorldMatrix; Matrix4x4 GetWorldMatrix(Animation, float)
           IEnumerable<Node> VisualChildren; Node VisualParent; Node VisualRoot
           GetCurveSamplers(Animation) -> per-node TRS / morph samplers
           static IEnumerable<Node> Flatten(Node)
  Fluent extension methods (Schema2 "Toolkit" class):
           node.WithMesh(mesh), node.WithSkin(skin),
           node.WithLocalTranslation(v3), node.WithLocalRotation(q),
           node.WithLocalScale(v3), node.WithLocalTransform(affine),
           node.WithSkinnedMesh(mesh, meshWorldMatrix, params Node[] joints),
           scene.FindNode(predicate), node.FindNode(predicate)

Meshes, primitives and accessors
--------------------------------
    Mesh:          IReadOnlyList<MeshPrimitive> Primitives;
                   MeshPrimitive CreatePrimitive()
    MeshPrimitive: Material Material; PrimitiveType DrawPrimitiveType;
                   Accessor GetVertexAccessor("POSITION" | "NORMAL" |
                     "TEXCOORD_0" | "COLOR_0" | "JOINTS_0" | "WEIGHTS_0" ...);
                   Accessor IndexAccessor; IList<uint> GetIndices();
                   GetTriangleIndices(), GetMorphTargetAccessors(int)
    Accessor:      AsVector2Array(), AsVector3Array(), AsVector4Array(),
                   AsQuaternionArray(), AsMatrix4x4Array(), AsScalarArray(),
                   AsIndicesArray(), AsColorArray(), Count, Dimensions,
                   Encoding, Normalized
  Low-level authoring extension methods:
    primitive.WithVertexAccessor("POSITION", Vector3[] values)
    primitive.WithIndicesAccessor(PrimitiveType.TRIANGLES, int[] indices)
    primitive.WithMaterial(material)
  PrimitiveType: POINTS, LINES, LINE_LOOP, LINE_STRIP, TRIANGLES,
                 TRIANGLE_STRIP, TRIANGLE_FAN

Materials (document model)
--------------------------
    Material: Name, Alpha, AlphaCutoff, DoubleSided, Unlit,
              IndexOfRefraction, Dispersion, Channels,
              MaterialChannel? FindChannel(string channelKey),
              bool TryGetChannel(string, out MaterialChannel)
    Extension methods: WithDefault(), WithDefault(Vector4 color),
              WithPBRMetallicRoughness(...), WithUnlit(),
              WithChannelColor(...), WithChannelTexture(...)
  Channel keys are strings such as "BaseColor", "MetallicRoughness",
  "Normal", "Occlusion", "Emissive". For authoring, MaterialBuilder (toolkit)
  is much easier.

Animations (document model)
---------------------------
    Animation anim = model.CreateAnimation("name");   // or model.UseAnimation
    anim.CreateTranslationChannel(node, IReadOnlyDictionary<float, Vector3>, linear)
    anim.CreateRotationChannel(node, IReadOnlyDictionary<float, Quaternion>, linear)
    anim.CreateScaleChannel(node, IReadOnlyDictionary<float, Vector3>, linear)
    anim.CreateMorphChannel(node, keyframes, morphCount, linear)
    anim.CreateVisibilityChannel(node, IReadOnlyDictionary<float, bool>)
    anim.CreateMaterialPropertyChannel(material, propertyName, keyframes)
    anim.DangerousCreatePointerChannel<T>("/nodes/0/rotation", keyframes)
    float anim.Duration
  Keyframe dictionaries map time in seconds to value. Cubic-spline overloads
  take (inTangent, value, outTangent) tuples.

Extensions
----------
  Built-in extensions are registered automatically; there is nothing to
  enable. Supported: KHR_materials_* (clearcoat, transmission, volume,
  sheen, specular, ior, iridescence, anisotropy, dispersion,
  emissive_strength, diffuse_transmission, unlit, pbrSpecularGlossiness),
  KHR_texture_transform, KHR_texture_basisu (KTX2), EXT_texture_webp,
  MSFT_texture_dds, EXT_texture_astc, KHR_lights_punctual,
  KHR_mesh_quantization, EXT_mesh_gpu_instancing, KHR_animation_pointer,
  KHR_node_visibility, KHR_xmp_json_ld, KHR_gaussian_splatting.
    ExtensionsFactory.SupportedExtensions            // names
    obj.GetExtension<T>() / obj.UseExtension<T>()    // on any glTF object
    ExtensionsFactory.RegisterExtension<TParent, TExt>(name, factory)
  Images and textures in KTX2, WebP and DDS are carried as bytes and written
  back as-is; this library does not decode or transcode pixels.

Memory
------
    MemoryImage: wraps image bytes (PNG, JPG, KTX2, WebP, DDS, ...).
      new MemoryImage(string filePath | byte[] | ArraySegment<byte>);
      implicit from string, byte[] and ArraySegment<byte>
      IsPng, IsJpg, IsKtx2, IsWebp, IsDds, MimeType, FileExtension,
      SourcePath, Content, SaveToFile(path), Open()


TOOLKIT: MATERIALS (CodeBrix.Graphics3D.Gltf2.Materials)
========================================================
    var material = new MaterialBuilder("name")
        .WithDoubleSide(true)
        .WithMetallicRoughnessShader()        // or WithUnlitShader()
        .WithBaseColor(new Vector4(r, g, b, a))
        .WithChannelImage(KnownChannel.BaseColor, "path/to/image.png")
        .WithMetallicRoughness(metallic, roughness)
        .WithAlpha(AlphaMode.BLEND, cutoff);

  MaterialBuilder.CreateDefault() returns a plain white material.
  WithChannelImage / WithBaseColor / WithNormal / WithOcclusion /
  WithEmissive accept an ImageBuilder, which converts implicitly from a file
  path string, byte[] or ArraySegment<byte>.
  KnownChannel: Normal, Occlusion, Emissive, BaseColor, MetallicRoughness,
    ClearCoat, ClearCoatNormal, ClearCoatRoughness, Transmission, SheenColor,
    SheenRoughness, SpecularColor, SpecularFactor, VolumeThickness,
    VolumeAttenuation, Iridescence, IridescenceThickness, Anisotropy,
    DiffuseTransmissionColor, DiffuseTransmissionFactor
    (Diffuse and SpecularGlossiness are OBSOLETE - see pitfalls)
  Per-channel control: material.UseChannel(KnownChannel.X).UseTexture()
    .WithPrimaryImage(image).WithCoordinateSet(n).WithTransform(...)
  MaterialBuilder.AreEqualByContent(a, b) compares by content.


TOOLKIT: MESHES (CodeBrix.Graphics3D.Gltf2.Geometry)
====================================================
A mesh builder is generic over three vertex "fragments":

    MeshBuilder<TvG, TvM, TvS>   geometry, material, skinning
    MeshBuilder<TvG, TvM>        no skinning
    MeshBuilder<TvG>             geometry only

  Geometry (TvG): VertexPosition, VertexPositionNormal,
                  VertexPositionNormalTangent
  Material (TvM): VertexEmpty, VertexColor1, VertexColor2, VertexTexture1,
                  VertexTexture2, VertexColor1Texture1, VertexColor1Texture2,
                  VertexColor2Texture2
  Skinning (TvS): VertexEmpty, VertexJoints4, VertexJoints8

  Primitives are grouped by material:

    var mesh = new MeshBuilder<VertexPositionNormal, VertexTexture1>("mesh");
    var prim = mesh.UsePrimitive(materialBuilder);
    prim.AddTriangle(a, b, c);
    prim.AddQuadrangle(a, b, c, d);
    prim.AddLine(a, b); prim.AddPoint(a);

  Vertices are VertexBuilder<TvG, TvM, TvS>. Implicit conversions:
    Vector3 -> VertexPosition
    (Vector3 position, Vector3 normal) -> VertexPositionNormal
    TvG -> VertexBuilder<TvG, TvM, TvS>
    (TvG, TvM), (TvG, TvS), (TvG, TvM, TvS) tuples -> VertexBuilder
  so a MeshBuilder<VertexPosition> accepts VertexPosition values directly.
  Declaring a `using VERTEX = VertexBuilder<...>;` alias (fully qualified)
  keeps explicit vertex construction readable.

  A degenerate triangle is not added: AddTriangle returns (-1, -1, -1).
  Invalid values (NaN, non-unit normals, out-of-range colors or weights)
  throw ArgumentException or ArgumentOutOfRangeException from AddTriangle.

  Batch conversion into a document: model.CreateMeshes(mesh1, mesh2, ...)
  packs every compatible mesh into shared vertex and index buffers.


TOOLKIT: SCENES (CodeBrix.Graphics3D.Gltf2.Scenes)
==================================================
SceneBuilder describes WHAT to render and HOW; nodes are created for you:

    var scene = new SceneBuilder();
    scene.AddRigidMesh(mesh, Matrix4x4 or AffineTransform or NodeBuilder);
    scene.AddSkinnedMesh(mesh, meshWorldMatrix, params NodeBuilder[] joints);
    scene.AddScene(otherScene, Matrix4x4 transform);
    scene.AddCamera(cameraBuilder, ...); scene.AddLight(lightBuilder, ...);
    scene.AddNode(nodeBuilder);
    ModelRoot model = scene.ToGltf2();
    ModelRoot model = scene.ToGltf2(SceneBuilderSchema2Settings.WithGpuInstancing);

  Each Add* returns an InstanceBuilder: .WithName(string),
  .WithExtras(JsonNode).
  Loading into a builder: SceneBuilder.LoadDefaultScene(path),
  SceneBuilder.Load(path), SceneBuilder.CreateFrom(scene),
  or scene.ToSceneBuilder() on a Schema2 Scene.

NodeBuilder is a standalone node hierarchy - an armature or a transform chain:
    var root = new NodeBuilder("root");
    var child = root.CreateNode("child")
        .WithLocalTranslation(new Vector3(0, 2, 0));
    child.UseRotation("trackName")           // animation curve for a track
        .WithPoint(0f, Quaternion.Identity)
        .WithPoint(1f, rotation);
    child.WithLocalRotation("trackName", IReadOnlyDictionary<float, Quaternion>);
  Properties: Name, Parent, Root, VisualChildren, LocalTransform, LocalMatrix,
  WorldMatrix, Translation, Rotation, Scale, Visibility, HasAnimations.

SceneBuilderSchema2Settings (struct): Default, WithGpuInstancing,
  GpuMeshInstancingMinCount, MergeBuffers, UseStridedBuffers,
  CompactVertexWeights, AllowArmatureDuplicatedNames.

Conversions between layers (Schema2 extension methods):
    scene.ToSceneBuilder()                  Scene     -> SceneBuilder
    mesh.ToMeshBuilder()                    Mesh      -> IMeshBuilder<MaterialBuilder>
    material.ToMaterialBuilder()            Material  -> MaterialBuilder
    model.CreateMeshes(meshBuilders...)     builders  -> Schema2 meshes
    model.CreateMaterial(materialBuilder)   builder   -> Schema2 material
    scene.EvaluateTriangles<TvG, TvM>(options, animation, time)
                                            world-space triangles
    evaluatedTriangles.ToMeshBuilder()      triangles -> MeshBuilder
    model.SaveAsWavefront(path [, animation, time])   Wavefront OBJ export


RUNTIME HELPERS (CodeBrix.Graphics3D.Gltf2.Runtime)
===================================================
    SceneTemplate template = SceneTemplate.Create(model.DefaultScene,
                                                  RuntimeOptions options = null);
    SceneInstance instance = template.CreateInstance();

  A SceneTemplate is immutable and does not keep the source ModelRoot alive
  when RuntimeOptions.IsolateMemory is true. Create any number of
  SceneInstances from one template; each animates independently.

    ArmatureInstance arm = instance.Armature;
    arm.AnimationTracks                     // name + duration per animation
    arm.SetAnimationFrame(int trackIndex, float time, bool looped = true)
    arm.SetLocalMatrix(nodeName, Matrix4x4); arm.SetModelMatrix(nodeName, m)
    arm.SetPoseTransforms()                 // back to the rest pose

    foreach (DrawableInstance d in instance)   // SceneInstance is enumerable
    {
        int meshIndex = d.Template.LogicalMeshIndex;  // into LogicalMeshes
        IGeometryTransform xform = d.Transform;       // see below
        int count = d.InstanceCount;
    }

  Transform kinds (namespace ...Transforms) - test InstancingTransform FIRST:
    InstancingTransform  WorldTransforms (IReadOnlyList<RigidTransform>)
                         - derives from RigidTransform
    SkinnedTransform     SkinMatrices (IReadOnlyList<Matrix4x4>)
    RigidTransform       WorldMatrix (Matrix4x4)
  All three expose TransformPosition / TransformNormal / TransformTangent and
  Visible / FlipFaces.

MeshDecoder - decoded, renderer-friendly vertex streams:
    IMeshDecoder<Material>[] meshes = model.LogicalMeshes.Decode();
    foreach (var prim in meshes[meshIndex].Primitives)
    {
        foreach (var (a, b, c) in prim.TriangleIndices)
        {
            Vector3 p = prim.GetPosition(a, drawable.Transform);   // world space
        }
    }
  Also GetNormal, GetTangent, GetColor, GetTextureCoord, and
  scene.EvaluateBoundingBox() / scene.EvaluateBoundingSphere().

RuntimeOptions { IsolateMemory, GpuMeshInstancing, ExtrasConverterCallback }


VALIDATION AND ERRORS
=====================
  Exceptions (namespace ...Validation), all derived from ModelException:
    SchemaException   malformed JSON or a schema violation
    LinkException     broken references between objects (bad indices,
                      a node that is both a scene root and a child, ...)
    DataException     invalid buffer or accessor contents
  ModelException.Message names the offending object and the generator that
  wrote the file.

  Load(path) of a path that does not exist throws ArgumentException (not
  FileNotFoundException). A missing satellite file (.bin, image) throws
  FileNotFoundException.

  ValidationMode.TryFix repairs common exporter mistakes while loading;
  ValidationMode.Skip disables validation (fast, but a malformed file can then
  fail later and less clearly).


COMPLETE EXAMPLES
=================

Example 1 - load, inspect and convert:

    using System;
    using CodeBrix.Graphics3D.Gltf2.Schema2;
    using CodeBrix.Graphics3D.Gltf2.Validation;

    var model = ModelRoot.Load("model.gltf", ValidationMode.TryFix);

    Console.WriteLine($"generator: {model.Asset.Generator}");
    foreach (var mesh in model.LogicalMeshes)
    {
        foreach (var prim in mesh.Primitives)
        {
            var positions = prim.GetVertexAccessor("POSITION").AsVector3Array();
            Console.WriteLine($"{mesh.Name}: {positions.Count} vertices, " +
                              $"{prim.GetIndices().Count} indices");
        }
    }

    model.SaveGLB("model.glb");

Example 2 - build a textured quad with the toolkit:

    using System.Numerics;
    using CodeBrix.Graphics3D.Gltf2.Geometry;
    using CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes;
    using CodeBrix.Graphics3D.Gltf2.Materials;
    using CodeBrix.Graphics3D.Gltf2.Scenes;
    using CodeBrix.Graphics3D.Gltf2.Schema2;
    using VERTEX = CodeBrix.Graphics3D.Gltf2.Geometry.VertexBuilder<
        CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexPositionNormal,
        CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexTexture1,
        CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexEmpty>;

    var material = new MaterialBuilder("textured")
        .WithMetallicRoughnessShader()
        .WithChannelImage(KnownChannel.BaseColor, "texture.png");

    var mesh = new MeshBuilder<VertexPositionNormal, VertexTexture1>("quad");
    var n = Vector3.UnitZ;
    mesh.UsePrimitive(material).AddQuadrangle(
        new VERTEX(new VertexPositionNormal(new Vector3(-1, -1, 0), n), new VertexTexture1(new Vector2(0, 1))),
        new VERTEX(new VertexPositionNormal(new Vector3( 1, -1, 0), n), new VertexTexture1(new Vector2(1, 1))),
        new VERTEX(new VertexPositionNormal(new Vector3( 1,  1, 0), n), new VertexTexture1(new Vector2(1, 0))),
        new VERTEX(new VertexPositionNormal(new Vector3(-1,  1, 0), n), new VertexTexture1(new Vector2(0, 0))));

    var scene = new SceneBuilder();
    scene.AddRigidMesh(mesh, Matrix4x4.CreateTranslation(0, 0, -5));

    ModelRoot model = scene.ToGltf2();
    model.SaveGLB("textured.glb");
    model.SaveGLTF("textured.gltf", new WriteSettings { JsonIndented = true });

  (The `using VERTEX = ...;` alias must be fully qualified and written on one
  logical line; it is wrapped here only to fit 80 columns.)

Example 3 - a skinned, animated mesh:

    using System.Numerics;
    using CodeBrix.Graphics3D.Gltf2.Geometry;
    using CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes;
    using CodeBrix.Graphics3D.Gltf2.Materials;
    using CodeBrix.Graphics3D.Gltf2.Scenes;
    using CodeBrix.Graphics3D.Gltf2.Schema2;
    using SKINNED = CodeBrix.Graphics3D.Gltf2.Geometry.VertexBuilder<
        CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexPosition,
        CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexEmpty,
        CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes.VertexJoints4>;

    var root = new NodeBuilder("root");
    var joint0 = root.CreateNode("joint0");
    var joint1 = joint0.CreateNode("joint1").WithLocalTranslation(new Vector3(0, 2, 0));
    joint1.UseRotation("bend")
        .WithPoint(0, Quaternion.Identity)
        .WithPoint(1, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.8f));

    var mesh = new MeshBuilder<VertexPosition, VertexEmpty, VertexJoints4>("arm");
    mesh.UsePrimitive(MaterialBuilder.CreateDefault()).AddQuadrangle(
        new SKINNED(new VertexPosition(-0.5f, 0, 0), new VertexJoints4(0)),
        new SKINNED(new VertexPosition( 0.5f, 0, 0), new VertexJoints4(0)),
        new SKINNED(new VertexPosition( 0.5f, 4, 0), new VertexJoints4(1)),
        new SKINNED(new VertexPosition(-0.5f, 4, 0), new VertexJoints4(1)));

    var scene = new SceneBuilder();
    scene.AddSkinnedMesh(mesh, Matrix4x4.Identity, joint0, joint1);  // joint
                                                                     // order =
                                                                     // indices
    scene.ToGltf2().SaveGLB("skinned.glb");

  VertexJoints4(0) binds a vertex fully to joint index 0 of the joints passed
  to AddSkinnedMesh.

Example 4 - low-level authoring with the document model:

    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using CodeBrix.Graphics3D.Gltf2.Schema2;

    var model = ModelRoot.CreateModel();
    var material = model.CreateMaterial("green").WithDefault(new Vector4(0, 1, 0, 1));

    var mesh = model.CreateMesh("tri");
    mesh.CreatePrimitive()
        .WithVertexAccessor("POSITION", new[] { new Vector3(0, 1, 0), new Vector3(-1, -1, 0), new Vector3(1, -1, 0) })
        .WithIndicesAccessor(PrimitiveType.TRIANGLES, new[] { 0, 1, 2 })
        .WithMaterial(material);

    var node = model.UseScene("Default").CreateNode("root").WithMesh(mesh);

    var anim = model.CreateAnimation("spin");
    anim.CreateRotationChannel(node, new Dictionary<float, Quaternion>
    {
        [0] = Quaternion.Identity,
        [1] = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI),
    });

    model.SaveGLB("raw.glb");

Example 5 - render loop data for a game engine:

    using System.Numerics;
    using CodeBrix.Graphics3D.Gltf2.Runtime;
    using CodeBrix.Graphics3D.Gltf2.Schema2;
    using CodeBrix.Graphics3D.Gltf2.Transforms;

    var model = ModelRoot.Load("character.glb");

    // upload once: one GPU mesh per logical mesh
    var decoded = model.LogicalMeshes.Decode();

    var template = SceneTemplate.Create(model.DefaultScene);
    var player = template.CreateInstance();
    var enemy = template.CreateInstance();

    player.Armature.SetAnimationFrame(0, 0.25f);
    enemy.Armature.SetAnimationFrame(0, 1.75f);

    foreach (var drawable in player)
    {
        var gpuMesh = decoded[drawable.Template.LogicalMeshIndex];

        switch (drawable.Transform)
        {
            case InstancingTransform instancing:
                // draw gpuMesh once per instancing.WorldTransforms[i].WorldMatrix
                break;
            case SkinnedTransform skinned:
                // upload skinned.SkinMatrices as the bone palette
                break;
            case RigidTransform rigid:
                // draw gpuMesh with rigid.WorldMatrix
                break;
        }
    }

Example 6 - in-memory, streams and zip archives:

    using System;
    using System.Collections.Generic;
    using System.IO;
    using CodeBrix.Graphics3D.Gltf2.IO;
    using CodeBrix.Graphics3D.Gltf2.Schema2;

    var model = ModelRoot.Load("model.gltf");

    var files = new Dictionary<string, ArraySegment<byte>>();
    model.Save("copy.gltf", WriteContext.CreateFromDictionary(files));
    var copy = ModelRoot.Load("copy.gltf", ReadContext.CreateFromDictionary(files));

    ArraySegment<byte> glb = model.WriteGLB();
    var fromBytes = ModelRoot.ParseGLB(glb);

    using (var zip = new ZipWriter("models.zip")) { zip.AddModel("model.gltf", model); }
    var fromZip = ZipReader.LoadModelFromZip("models.zip");

  ZipWriter(path) creates a NEW archive and throws IOException if the file
  already exists; delete it first when overwriting.


MINIMUM VIABLE PROJECT TEMPLATE
===============================
GltfDemo.csproj:

    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="CodeBrix.Graphics3D.Gltf2.MitLicenseForever" Version="*" />
      </ItemGroup>
    </Project>

Program.cs:

    using System;
    using CodeBrix.Graphics3D.Gltf2.Schema2;

    var model = ModelRoot.Load(args[0]);
    Console.WriteLine($"{model.LogicalMeshes.Count} meshes, " +
                      $"{model.LogicalNodes.Count} nodes, " +
                      $"{model.LogicalAnimations.Count} animations");
    model.SaveGLB(System.IO.Path.ChangeExtension(args[0], ".converted.glb"));

(Pin a concrete package version instead of "*" in real projects.)


PERFORMANCE TIPS
================
  * Create a SceneTemplate once per model and many SceneInstances from it;
    instances are cheap, templates are not.
  * Decode meshes (LogicalMeshes.Decode()) once and cache the result; do not
    call EvaluateTriangles in a render loop - it allocates per triangle and is
    meant for export and inspection.
  * Pass RuntimeOptions { IsolateMemory = true } to SceneTemplate.Create when
    the ModelRoot should be garbage-collectable after the template is built.
  * Build many meshes into one document with model.CreateMeshes(a, b, c) in a
    single call so they share vertex and index buffers.
  * For many copies of one mesh, use
    ToGltf2(SceneBuilderSchema2Settings.WithGpuInstancing) to emit
    EXT_mesh_gpu_instancing instead of duplicate nodes.
  * ValidationMode.Skip speeds up loading trusted files.
  * WriteSettings.MergeBuffers (default true) keeps .gltf output to one .bin.


COMMON PITFALLS TO AVOID
========================
  * Using the upstream namespaces. `using SharpGLTF.Schema2;` does not compile
    against this package - the root is CodeBrix.Graphics3D.Gltf2.
  * Testing RigidTransform before InstancingTransform. InstancingTransform
    DERIVES from RigidTransform, so `case RigidTransform` first makes the
    instancing case unreachable (the compiler reports CS8120). Order the switch
    InstancingTransform, SkinnedTransform, RigidTransform.
  * Assuming Load() throws FileNotFoundException for a missing model file - it
    throws ArgumentException. Check File.Exists first if you need a specific
    message.
  * Expecting to remove objects from a ModelRoot. The document model is
    effectively append-only; to edit, convert with ToSceneBuilder(), change the
    SceneBuilder, and write a new model with ToGltf2().
  * Using KnownChannel.Diffuse / KnownChannel.SpecularGlossiness,
    WithSpecularGlossinessShader() or WithPBRSpecularGlossiness(). They target
    KHR_materials_pbrSpecularGlossiness, which Khronos has deprecated; they are
    marked [Obsolete] and produce CS0618. Use BaseColor with
    SpecularColor / SpecularFactor instead.
  * A few APIs are marked [Experimental] and raise diagnostics GLTFRT1000,
    GLTFRT1001 or GLTFRT1002 when used (gaussian-splat primitives and parts of
    the runtime template API). Suppress the specific ID in your project only
    if you accept that the API may change.
  * Ambiguous AlphaMode. Both ...Materials and ...Schema2 declare an enum named
    AlphaMode (OPAQUE, MASK, BLEND). MaterialBuilder.WithAlpha takes the
    Materials one; with both namespaces imported, fully qualify it
    (CodeBrix.Graphics3D.Gltf2.Materials.AlphaMode.BLEND) or add a using alias.
  * Writing a `using` alias with relative names. `using V = VertexBuilder<...>`
    must be fully qualified - aliases ignore the other using directives.
  * Expecting ImageWriting to embed buffers. WriteSettings.ImageWriting only
    affects images; save as .glb for a single self-contained file.
  * Loading files from exporters that write invalid node graphs (for example a
    node listed as a scene root that is also a child) throws LinkException
    under both Strict and TryFix validation. ValidationMode.Skip loads such a
    file, but the resulting document is not guaranteed to be consistent.


WHAT THIS PACKAGE DOES NOT DO
=============================
  * Render anything. It prepares data for a renderer; drawing is up to you.
  * Decode or encode image pixels. Images are opaque byte payloads.
  * Draco (KHR_draco_mesh_compression) or meshopt (EXT_meshopt_compression)
    compressed geometry - such files cannot be loaded.
  * KHR_materials_variants, MSFT_lod, EXT_lights_image_based,
    KHR_techniques_webgl.
  * The AGI (AGI_articulations, AGI_stk_metadata) and Cesium 3D Tiles
    (EXT_mesh_features, EXT_structural_metadata, ...) vendor extensions.
  * Read glTF 1.0 or other 3D formats (FBX, OBJ import, ...). Wavefront OBJ is
    export-only.
  * Download anything: URIs are resolved against local files, dictionaries or
    zip archives only.


WORKING EXAMPLES ON GITHUB
==========================
The test project exercises every feature area:
https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/tree/main/tests/CodeBrix.Graphics3D.Gltf2.Tests

  Loading and saving       Schema2/LoadAndSave/LoadSampleTests.cs,
                           Schema2/LoadAndSave/LoadSpecialModelsTest.cs,
                           Schema2/LoadAndSave/ReadContextTests.cs,
                           Schema2/LoadAndSave/RegressionTests.cs,
                           IO/ZipTests.cs
  Document-model authoring Schema2/Authoring/BasicSceneCreationTests.cs,
                           Schema2/Authoring/AdvancedCreationTests.cs,
                           Schema2/Authoring/ExtensionsCreationTests.cs
  Scene builder            Scenes/SceneBuilderTests.cs
  Mesh builder             Geometry/MeshBuilderTests.cs,
                           Geometry/MeshBuilderAdvancedTests.cs,
                           Geometry/VertexTypes/*.cs
  Material builder         Materials/MaterialBuilderTests.cs,
                           Materials/ContentSharingTests.cs
  Animation curves         Animations/AnimationSamplingTests.cs,
                           Animations/CurveBuilderTests.cs
  Runtime helpers          Runtime/SceneTemplateTests.cs,
                           Runtime/ExtensionTests.cs
  Transforms               Transforms/AffineTransformMatrixTests.cs,
                           Transforms/SparseWeight8Tests.cs
  Memory and accessors     Memory/MemoryAccessorTests.cs,
                           Memory/MemoryArrayTests.cs,
                           Memory/MemoryImageTests.cs
  Reflection / pointers    Reflection/ReflectionTests.cs
  Wavefront export         IO/WavefrontWriterTest.cs
  Real-world scenarios     ThirdParty/*.cs


QUICK REFERENCE CARD
====================
  Load .......... ModelRoot.Load(path [, ValidationMode.TryFix | ReadSettings])
  Save .......... model.SaveGLB(path) / model.SaveGLTF(path, settings)
  Bytes ......... model.WriteGLB() / ModelRoot.ParseGLB(bytes)
  New ........... ModelRoot.CreateModel(); model.UseScene("name").CreateNode()
  Traverse ...... model.DefaultScene.VisualChildren / model.Logical*
  Mesh data ..... prim.GetVertexAccessor("POSITION").AsVector3Array()
  Material ...... new MaterialBuilder().WithMetallicRoughnessShader()
                    .WithBaseColor(v4).WithChannelImage(KnownChannel.X, path)
  Mesh .......... new MeshBuilder<TvG, TvM, TvS>().UsePrimitive(mat)
                    .AddTriangle(a, b, c)
  Scene ......... new SceneBuilder().AddRigidMesh(mesh, matrix).ToGltf2()
  Skinning ...... scene.AddSkinnedMesh(mesh, meshMatrix, joints...)
  Animate ....... nodeBuilder.UseRotation("track").WithPoint(t, q)
  Edit .......... model.DefaultScene.ToSceneBuilder() ... .ToGltf2()
  Render prep ... SceneTemplate.Create(scene).CreateInstance();
                    instance.Armature.SetAnimationFrame(i, t);
                    foreach (var d in instance) { d.Template, d.Transform }
  Decode ........ model.LogicalMeshes.Decode()
  Triangles ..... scene.EvaluateTriangles<VertexPosition, VertexEmpty>()
  OBJ ........... model.SaveAsWavefront(path)
  Errors ........ ModelException > SchemaException | LinkException |
                  DataException

================================================================================
END OF AGENT-README
================================================================================
