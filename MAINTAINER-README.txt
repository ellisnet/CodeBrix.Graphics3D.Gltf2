================================================================================
MAINTAINER-README: CodeBrix.Graphics3D.Gltf2
Notes for people and agents MAINTAINING this repository - not for package
consumers
================================================================================

If you are CONSUMING the NuGet package, stop reading and open AGENT-README.txt
instead. Everything below is about the repository itself: how it is laid out,
how it builds, how it is tested, how it is packaged, and the conventions the
source follows.


PURPOSE AND SCOPE
=================

This repository produces exactly one NuGet package:

    PackageId  : CodeBrix.Graphics3D.Gltf2.MitLicenseForever
    Project    : src/CodeBrix.Graphics3D.Gltf2/CodeBrix.Graphics3D.Gltf2.csproj
    Assembly   : CodeBrix.Graphics3D.Gltf2
    Namespace  : CodeBrix.Graphics3D.Gltf2 (root) plus .Animations,
                 .Collections, .Diagnostics, .Geometry(.VertexTypes, ...), .IO,
                 .Materials, .Memory, .Reflection, .Runtime, .Scenes, .Schema2,
                 .Transforms, .Validation
    License    : MIT
    Documented : AGENT-README.txt (repo root) -- packed INTO the nupkg, so it
                 must stay accurate

The library is a port of SharpGLTF that merges the upstream SharpGLTF.Core,
SharpGLTF.Runtime and SharpGLTF.Toolkit packages into one assembly with an
identical public API under the CodeBrix.Graphics3D.Gltf2 namespace root. Keeping
it a drop-in replacement is a design goal: the test
AssemblyAPITests.public_api_contains_every_signature_of_the_ported_packages
fails if any public signature of the ported packages goes missing (see
TESTING).

Nothing else in the repository ships.


REPOSITORY LAYOUT
=================

    CodeBrix.Graphics3D.Gltf2.slnx  library + tests. Its Solution Items folder
                                    carries .gitignore, AGENT-README.txt,
                                    EXTRAS-README.txt, global.json,
                                    icon-codebrix-128.png, LICENSE,
                                    MAINTAINER-README.txt, README-INDEX.txt,
                                    README.md and THIRD-PARTY-NOTICES.txt; its
                                    Tests folder carries the test project.
    global.json                     selects the Microsoft.Testing.Platform test
                                    runner; pins no SDK (see TESTING)
    AGENT-README.txt                consumer documentation (packed into the
                                    nupkg)
    MAINTAINER-README.txt           this file
    EXTRAS-README.txt               non-package content (the test project)
    README-INDEX.txt                map of the README files
    README.md                       human-facing overview (GitHub + nuget.org)
    LICENSE                         MIT
    THIRD-PARTY-NOTICES.txt         upstream attribution, baseline, and the
                                    license of every test asset
    icon-codebrix-128.png           package icon
    AGENTS.md, CLAUDE.md, .clinerules, .cursorrules, .windsurfrules,
    .cursor/rules/agent-readme.mdc, .github/copilot-instructions.md,
    .junie/guidelines.md            AI-assistant pointer stubs -> README-INDEX.txt
                                    (byte-for-byte family canonical; never edit)

    src/CodeBrix.Graphics3D.Gltf2/
        InternalsVisibleTo.cs       grants CodeBrix.Graphics3D.Gltf2.Tests access
        BaseBuilder.cs, Guard.cs, _Extensions.cs
                                    root-namespace helpers
        Animations/                 curve samplers and animation helpers
        Collections/                child lists/dictionaries, vertex lists
        Diagnostics/                debugger proxies (DebugViews.cs from the
                                    upstream Core, DebugViews.Toolkit.cs from
                                    the upstream Toolkit)
        Geometry/                   MeshBuilder, PrimitiveBuilder, VertexBuilder,
                                    VertexTypes/, Packed/, ...
        IO/                         JSON serialization base, ZipReader/Writer,
                                    WavefrontWriter
        Materials/                  MaterialBuilder, ChannelBuilder,
                                    TextureBuilder, ImageBuilder
        Memory/                     MemoryImage and the typed accessor arrays
        Reflection/                 FieldInfo / pointer-path reflection
        Runtime/                    SceneTemplate, SceneInstance, MeshDecoder
        Scenes/                     SceneBuilder, NodeBuilder, content types
        Schema2/                    the glTF document model, serialization,
                                    extensions, and the Toolkit extension
                                    methods; Generated/ holds the schema classes
        Transforms/                 AffineTransform, Rigid/Skinned/Instancing
                                    transforms, SparseWeight8, Matrix4x4Double
        Validation/                 ValidationContext and the exceptions

      Every folder matches its namespace (the upstream layout was kept).

    tests/CodeBrix.Graphics3D.Gltf2.Tests/
        mirrors the library folders (Animations/, Collections/, Geometry/, IO/,
        Materials/, Memory/, Reflection/, Runtime/, Scenes/, Schema2/,
        Transforms/, Validation/), plus:
        ThirdParty/     real-world regression scenarios
        TestSupport/    helpers: TestFiles, ResourceInfo, AttachmentInfo,
                        GltfTestUtils, NumericsAssert, DumpAssemblyAPI, ...
        Assets/         test input files (see TEST ASSETS)


BUILDING
========

    dotnet restore CodeBrix.Graphics3D.Gltf2.slnx
    dotnet build   CodeBrix.Graphics3D.Gltf2.slnx

Target framework is net10.0 only. A clean build is 0 warnings and 0 errors --
fix warnings at the source, never with a project-wide <NoWarn>.

ONE SANCTIONED EXCEPTION: the library csproj sets <NoWarn>1591</NoWarn>. The
upstream build suppresses CS1591 too, a large part of the ported public surface
carries no upstream doc comment, and the port keeps upstream doc comments
exactly as they are rather than retrofitting new ones. The project owner
granted this exception for this repository only. GenerateDocumentationFile
stays on, so the upstream comments that do exist ship as IntelliSense. Every OTHER documentation warning (CS1573, CS1584, CS1711,
...) is still fixed at source.

<AllowUnsafeBlocks> is on (the memory accessors use unsafe code) and
<IsAotCompatible> is on, which enables the trim/AOT analyzers. Three
reflection-based diagnostic helpers (debugger display text and exception
messages) carry a justified [UnconditionalSuppressMessage("Trimming", "IL2075")]
because they fall back gracefully when trimming removes the member they look
up; any NEW IL2xxx/IL3xxx warning must be fixed, not suppressed.

GeneratePackageOnBuild is TRUE, so every build also produces a .nupkg.


TESTING
=======

    dotnet test --solution CodeBrix.Graphics3D.Gltf2.slnx

THE TEST RUNNER IS Microsoft.Testing.Platform (MTP), selected by global.json at
the repo root:

    { "test": { "runner": "Microsoft.Testing.Platform" } }

Do not delete that file; without it `dotnet test` falls back to the VSTest
bridge, which xUnit v3 no longer supports on the .NET 10 SDK. MTP output ends in
a "Test run summary:" block. If `dotnet test --solution` reports zero tests,
build and run the test assembly directly:

    dotnet build tests/CodeBrix.Graphics3D.Gltf2.Tests
    dotnet tests/CodeBrix.Graphics3D.Gltf2.Tests/bin/Debug/net10.0/CodeBrix.Graphics3D.Gltf2.Tests.dll

The suite is xUnit v3 + SilverAssertions. The test project references no
coverage collector.

What the suite contains:

  * The upstream SharpGLTF test suites for the three ported projects, ported
    from NUnit (see THIRD-PARTY-NOTICES.txt for what was dropped or
    retargeted). Upstream test class and method names were kept, which is why
    they do not follow the family's snake_case test-naming style; tests added
    in this repository do.
  * AssemblyAPITests.public_api_contains_every_signature_of_the_ported_packages
    compares the library's public API against
    Assets/API/API.SharpGLTF.1.0.7.txt -- a dump of the upstream packages'
    public API with the namespace renamed. Adding API is fine; removing or
    changing a listed signature breaks drop-in compatibility and fails this
    test. Do not edit the reference file to make the test pass.

Test outputs: tests that save models write them under
bin/<Configuration>/net10.0/TestResults/<TestClass>/<TestMethod>[_<row>]/ via
AttachmentInfo. Every saved .gltf/.glb is checked with the Khronos glTF
validator (the GltfValidator NuGet package, which carries the validator's
native executable for Linux, macOS and Windows); a validation ERROR fails the
test. The validator is a test-only dependency and runs locally from the
package -- nothing is downloaded at test time.

Other test dependencies: CodeBrix.Imaging (generates one PNG in
ThirdParty/MeltyPlayerTests.cs).

The suite never reaches outside the repository: no network access, no files
outside tests/.../Assets. Keep it that way.

Any call inside a test that accepts a CancellationToken must be passed
TestContext.Current.CancellationToken (xUnit1051).


TEST ASSETS
===========

tests/CodeBrix.Graphics3D.Gltf2.Tests/Assets/ is copied to the test output and
read through ResourceInfo.From("relative/path") and TestFiles:

    KhronosSampleAssets/Models/   a pinned subset of the Khronos glTF sample
                                  models (CC0 / CC-BY-4.0), each folder with
                                  its upstream LICENSE.md and metadata.json
    BabylonJS/                    one Babylon.js sample (CC-BY-4.0)
    PolyHaven/                    CC0 textures, re-encoded to PNG, JPG, WebP
                                  and DXT5 DDS
    Kenney/                       one CC0 Kenney model and its texture
    API/                          the upstream public-API reference listing
    SpecialCases/ and the root    small hand-authored glTF files and
                                  placeholder images

RULE: an asset may be added only if its license is identified and allows
redistribution from an MIT-licensed repository (CC0 or CC-BY-4.0 with
attribution, or authored here), and it must be recorded in
THIRD-PARTY-NOTICES.txt section 3 (or section 2 if authored here). Prefer the
CC0 Poly Haven and Kenney collections for new inputs. Keep the total small --
large model collections are deliberately NOT vendored, and tests that need them
were not ported.

The image assets were produced with CodeBrix.Imaging (PNG, JPG, WebP) and
ImageMagick (DXT5 DDS, which CodeBrix.Imaging cannot encode) outside this
repository; no conversion tooling lives here.


PACKAGING / PUBLISHING
======================

The library project packs itself on every build. Versioning is DATE-STAMPED and
auto-incrementing, computed in the csproj from System.DateTime.UtcNow as
1.<x>.<y>.<z>:

    1  major     pinned to 1 for this library
    x  minor     whole years since _VersionBaseYear (2026 => 0)
    y  build     day of year, UTC, 1-based (Jan 1 = 1)
    z  revision  minute of day, UTC, 0..1439

Every build produces a new version; two builds in the same UTC minute produce
the same version, so never publish two packages from one minute. This is not
SemVer.

The nupkg contains the assembly, its XML documentation file, and:

    icon-codebrix-128.png     PackageIcon
    README.md                 PackageReadmeFile
    AGENT-README.txt          consumer documentation for AI agents
    THIRD-PARTY-NOTICES.txt   upstream attribution

MAINTAINER-README.txt, EXTRAS-README.txt and README-INDEX.txt are NOT packed.

The informational version also carries the git commit (1.x.y.z+<sha>); it is
what the library writes into every saved model's asset.generator
("CodeBrix.Graphics3D.Gltf2 <informational version>").

Publishing to nuget.org is done by the project owner. Git tags are expected to
match the published NuGet version.


CODING CONVENTIONS
==================

  * .cs top-of-file layout: [preserved upstream header,] usings (one
    contiguous block, System.* first, aliases and `using static` last), one
    blank line, file-scoped namespace, one blank line, body. No global usings,
    no #nullable, no `?` on reference types, no null-forgiving `!`.
  * Every ported file's namespace line carries
    `//was previously: <upstream-namespace>;`. New files written for this
    repository do not.
  * Keep the upstream folder layout and type names: the public API must stay a
    superset of the ported packages' API (see TESTING).
  * Tests: xUnit v3 + SilverAssertions fluent assertions. New tests use the
    family naming style (<Member>_<snake_case> or snake_case) with
    //Arrange //Act //Assert comments; ported upstream tests keep their names.
  * Nothing in the library or the tests may access the network, download
    files, or read outside the repository.


PROVENANCE / VENDORED SOURCES
=============================

The whole of src/ is a port; THIRD-PARTY-NOTICES.txt is the authoritative
statement. Short form:

  * Upstream: SharpGLTF, https://github.com/vpenades/SharpGLTF, MIT,
    Copyright (c) 2019 Vicente Penades.
  * Baseline: tag v1.0.7, commit acbc151e70d65ceb8bec24fa95f0067d0e5ed933.
  * src/SharpGLTF.Core, src/SharpGLTF.Runtime, src/SharpGLTF.Toolkit and
    src/Shared -> src/CodeBrix.Graphics3D.Gltf2/ (merged).
  * The matching upstream test projects -> tests/CodeBrix.Graphics3D.Gltf2.Tests/.

The port was produced with conversion scripts that are not part of this
repository; they are not needed to build or maintain the library.


NOTES
=====

Generated schema classes are now maintained BY HAND.
    Schema2/Generated/*.g.cs were produced upstream by a code generator that
    reads the Khronos glTF JSON schemas. The generator is NOT part of this
    repository, so those files keep their upstream "programmatically
    generated; DON'T EDIT" banner but are in fact maintained by hand here. The
    same applies to the two T4 outputs in Geometry/VertexTypes
    (VertexMaterial.Permutations.cs, VertexUtils.Builder.Reflection.cs), whose
    .tt templates were not ported.

    The core glTF 2.0 schema is stable; new functionality arrives as
    extensions. To add support for a new extension:
      1. Write the data/serialization class in Schema2/Generated/, modelled on
         an existing ext.*.g.cs file: a partial class deriving from
         ExtensionBase with the SCHEMANAME constant, private fields for each
         schema property (with defaults), SerializeProperties,
         DeserializeProperty, and the reflection overrides.
      2. Write the public API as the other half of that partial class in
         Schema2/ (see gltf.Node.Visibility.cs for a small example).
      3. Register it in Schema2/gltf.ExtensionsFactory.cs.
      4. Where it makes sense, add toolkit support (for example a new
         KnownChannel and MaterialBuilder methods for a material extension).
      5. Add tests, and record the extension in AGENT-README.txt.
    To revise an existing extension, edit its .g.cs file the same way.

Experimental APIs.
    A few members carry [Experimental("GLTFRT1000" / "GLTFRT1001" /
    "GLTFRT1002")] (gaussian splatting and parts of the runtime template API),
    exactly as upstream. Consumers must opt in by suppressing the ID.

Linux path behaviour.
    ReadContext.CreateFromDirectory normalises '\' to '/' in relative URIs
    (upstream only resolved such URIs on Windows).
    ReadContextTests.CreateFromDirectory_resolves_backslash_separated_uris
    fences it.

Obsolete API.
    KHR_materials_pbrSpecularGlossiness support (KnownChannel.Diffuse,
    KnownChannel.SpecularGlossiness, WithSpecularGlossinessShader, ...) is
    [Obsolete] upstream and here. Tests that still exercise it wrap the calls in
    a local `#pragma warning disable CS0618`.
