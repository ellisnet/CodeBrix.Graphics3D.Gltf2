using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Schema2.LoadAndSave; //was previously: SharpGLTF.Schema2.LoadAndSave;

/// <summary>
/// Test cases for models found in <see href="https://github.com/KhronosGroup/glTF-Blender-Exporter"/>
/// </summary>
public class XmpJsonTests
{
    [Fact]
    public void LoadXmpModel()
    {
        var model = ModelRoot.Load(ResourceInfo.From("XmpJsonLd.gltf"));
        model.Should().NotBeNull();

        var packets = model.GetExtension<XmpPackets>();
        packets.Should().NotBeNull();
        packets.JsonPackets.Count.Should().Be(1);

        var packet = packets.JsonPackets[0];

        model.AttachToCurrentTest("result.gltf");

        var model2 = ModelRoot.CreateModel();
        model2.UseExtension<XmpPackets>().AddPacket(packet);
        model2.Asset.UseExtension<XmpPacketReference>().SetPacket(0);
        model2.AttachToCurrentTest("result2.gltf");

    }
}
