================================================================================
EXTRAS-README: CodeBrix.Graphics3D.Gltf2
Samples, tools and other content in this repository that is not part of a NuGet
package
================================================================================

This repository ships no sample applications, demos or command-line tools.
Exactly one project is packable - src/CodeBrix.Graphics3D.Gltf2 - and everything
else listed below exists only to test or document it. None of it is included in
the CodeBrix.Graphics3D.Gltf2.MitLicenseForever package.

For runnable, compilable usage of the library, read the test project: the
"WORKING EXAMPLES ON GITHUB" section of AGENT-README.txt maps each feature area
to the test file that exercises it.


TEST PROJECT
============
    tests/CodeBrix.Graphics3D.Gltf2.Tests/

The only non-package project in the solution. xUnit v3; run it as described in
MAINTAINER-README.txt (TESTING).


TEST ASSETS
===========
    tests/CodeBrix.Graphics3D.Gltf2.Tests/Assets/

Test input files - a pinned subset of the Khronos glTF sample models, one
Babylon.js sample, CC0 textures from Poly Haven, one CC0 Kenney model, a few
hand-authored glTF files, and the upstream public-API reference listing. They
are copied to the test output directory. The license of every file is recorded
in THIRD-PARTY-NOTICES.txt; the rules for adding assets are in
MAINTAINER-README.txt (TEST ASSETS).
