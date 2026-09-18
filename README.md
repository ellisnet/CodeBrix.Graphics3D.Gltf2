# CodeBrix.Graphics3D.Gltf2

A fully managed, cross-platform library for reading, writing and building Khronos glTF 2.0 3D models (`.gltf` and `.glb`) in .NET. CodeBrix.Graphics3D.Gltf2 is provided as a .NET 10 library and associated `CodeBrix.Graphics3D.Gltf2.MitLicenseForever` NuGet package.

CodeBrix.Graphics3D.Gltf2 supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## Installation

```
dotnet add package CodeBrix.Graphics3D.Gltf2.MitLicenseForever
```

Note that the NuGet package ID and the namespace are different - there is no package named plain `CodeBrix.Graphics3D.Gltf2`:

* NuGet package ID: `CodeBrix.Graphics3D.Gltf2.MitLicenseForever`
* Assembly and primary namespace: `CodeBrix.Graphics3D.Gltf2` - i.e. `using CodeBrix.Graphics3D.Gltf2.Schema2;` for the document model

XML documentation (IntelliSense) ships alongside the assembly.

CodeBrix.Graphics3D.Gltf2 has no dependencies other than .NET.

## CodeBrix.Graphics3D.Gltf2 supports:

* Loading and saving glTF 2.0 models as `.gltf` (JSON with satellite or embedded resources) and binary `.glb`, from files, streams, byte arrays, in-memory dictionaries and zip archives
* A complete glTF 2.0 document object model - scenes, nodes, meshes, primitives, accessors, buffers, materials, textures, images, samplers, skins, cameras and animations
* The common Khronos and vendor extensions, including the PBR material extensions (clearcoat, transmission, volume, sheen, specular, IOR, iridescence, anisotropy, dispersion, emissive strength, diffuse transmission, unlit), texture transforms, KTX2/Basis Universal, WebP and DDS textures, punctual lights, mesh quantization, GPU mesh instancing, animation pointers, node visibility and XMP metadata
* Validation on load, with strict, try-fix and skip modes
* Mesh, material, node and scene builders for authoring models from code, including skinned meshes, morph targets and animation curves
* Runtime helpers for renderers: scene templates and lightweight animated instances, decoded meshes, and rigid, skinned and GPU-instanced transforms
* Evaluating a scene into world-space triangles, and exporting to Wavefront OBJ

## Sample Code

### Load a model and save it as GLB

```csharp
using System;
using CodeBrix.Graphics3D.Gltf2.Schema2;

var model = ModelRoot.Load("model.gltf");
Console.WriteLine($"{model.LogicalMeshes.Count} meshes, {model.LogicalAnimations.Count} animations");

model.SaveGLB("model.glb");
```

### Build a model from code

```csharp
using System.Numerics;
using CodeBrix.Graphics3D.Gltf2.Geometry;
using CodeBrix.Graphics3D.Gltf2.Geometry.VertexTypes;
using CodeBrix.Graphics3D.Gltf2.Materials;
using CodeBrix.Graphics3D.Gltf2.Scenes;

var material = new MaterialBuilder("red")
    .WithDoubleSide(true)
    .WithMetallicRoughnessShader()
    .WithBaseColor(new Vector4(1, 0, 0, 1));

var mesh = new MeshBuilder<VertexPosition>("triangle");
mesh.UsePrimitive(material).AddTriangle(
    new VertexPosition(-10, 0, 0),
    new VertexPosition(10, 0, 0),
    new VertexPosition(0, 10, 0));

var scene = new SceneBuilder();
scene.AddRigidMesh(mesh, Matrix4x4.Identity);

scene.ToGltf2().SaveGLB("triangle.glb");
```

### Prepare a model for rendering

```csharp
using CodeBrix.Graphics3D.Gltf2.Runtime;
using CodeBrix.Graphics3D.Gltf2.Schema2;
using CodeBrix.Graphics3D.Gltf2.Transforms;

var model = ModelRoot.Load("character.glb");

var template = SceneTemplate.Create(model.DefaultScene);  // immutable, shareable
var instance = template.CreateInstance();                  // lightweight, animatable

instance.Armature.SetAnimationFrame(0, 1.5f);              // animation 0 at 1.5 seconds

foreach (var drawable in instance)
{
    var meshIndex = drawable.Template.LogicalMeshIndex;

    switch (drawable.Transform)
    {
        case InstancingTransform instancing: /* draw instancing.WorldTransforms */ break;
        case SkinnedTransform skinned:       /* draw with skinned.SkinMatrices */ break;
        case RigidTransform rigid:           /* draw with rigid.WorldMatrix */ break;
    }
}
```

## Documentation

The NuGet package includes `AGENT-README.txt`, a complete API reference and usage guide written for AI coding agents - point your agent at that file when it is writing code against this library.

Additional sample code and usage examples are available in the `CodeBrix.Graphics3D.Gltf2.Tests` project:
https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/tree/main/tests/CodeBrix.Graphics3D.Gltf2.Tests

## License

CodeBrix.Graphics3D.Gltf2 is licensed under the MIT License - see the
[LICENSE](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/blob/main/LICENSE) file.

For licensing and provenance information about the open source code included in
this package, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.Graphics3D.Gltf2/blob/main/THIRD-PARTY-NOTICES.txt).
