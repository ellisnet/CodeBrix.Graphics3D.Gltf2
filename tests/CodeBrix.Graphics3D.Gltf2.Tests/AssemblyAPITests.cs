using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2; //was previously: SharpGLTF;

public class AssemblyAPITests
{
    public class TestClass
    {
        public int PointerFunc(int x, out int y, ref int z) { y = 0; return x + 7; }

        public void ParamsFunc(int x, params string[] values) { }

        public void MultiArgsFunc(int x, int y=1, int z = 2) { }

        public int[][,][,,] MultiArray;

        public const string Alpha = "Alpha";

        public static readonly string Beta = "Beta";

        public delegate void MyDelegate(int x);

        public MyDelegate DelegateImpl;

        public enum Hello
        {
            World = 7
        }

        public event EventHandler EventX { add { } remove { } }

        public struct SomeStructure
        {
            public int X;
            public int Y { get; }
            public int Z { get; set; }

            public System.Numerics.Vector3 Vector;
        }
    }

    [Fact]
    public void DumpTestAPI()
    {
        var type = typeof(TestClass);

        var API = DumpAssemblyAPI.GetTypeSignature(type.GetTypeInfo()).OrderBy(item => item).ToArray();

        AttachmentInfo.From("TestAPI.txt").WriteTextLines(API);

        foreach (var l in API)
        {
            TestContext.Current.TestOutputHelper?.WriteLine(l);
        }
    }

    [Fact]
    public void DumpCoreAPI()
    {
        var assembly = typeof(Schema2.ModelRoot).Assembly;

        var API = DumpAssemblyAPI
            .GetAssemblySignature(assembly)
            .OrderBy(item => item)
            .ToArray();

        AttachmentInfo
            .From($"API.{Schema2.Asset.AssemblyInformationalVersion}.txt")
            .WriteTextLines(API);
    }

    /// <summary>
    /// The library is a drop-in replacement for the three packages it was ported
    /// from, so every public signature they exposed (namespace renamed) must still
    /// be present. The reference listing in Assets/API was dumped from those
    /// packages' net10.0 assemblies with <see cref="DumpAssemblyAPI"/>.
    /// </summary>
    [Fact]
    public void public_api_contains_every_signature_of_the_ported_packages()
    {
        //Arrange
        var referenceLines = System.IO.File.ReadAllLines(ResourceInfo.From("API/API.SharpGLTF.1.0.7.txt").FilePath);

        //Act
        var currentLines = DumpAssemblyAPI
            .GetAssemblySignature(typeof(Schema2.ModelRoot).Assembly)
            .ToHashSet(StringComparer.Ordinal);

        var missing = referenceLines.Where(l => !currentLines.Contains(l)).ToList();

        foreach (var l in missing) TestContext.Current.TestOutputHelper?.WriteLine($"Missing:  {l}");

        //Assert
        referenceLines.Should().NotBeEmpty();
        missing.Should().BeEmpty("the public API must remain a superset of the ported packages' API");
    }
}
