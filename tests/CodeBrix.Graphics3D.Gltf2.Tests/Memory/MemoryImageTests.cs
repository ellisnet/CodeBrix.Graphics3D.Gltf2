using System;
using System.Collections.Generic;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Memory; //was previously: SharpGLTF.Memory;

public class MemoryImageTests
{
    [Fact]
    public void TestImageEquality()
    {
        // two images that are equal byte by byte, loaded from different sources
        // must be considered equal.

        var image1 = new MemoryImage(MemoryImage.DefaultPngImage, "first_reference.png");
        var image2 = new MemoryImage(MemoryImage.DefaultPngImage, "second_reference.png");
        var image3 = MemoryImage.Empty;

        image2.GetHashCode().Should().Be(image1.GetHashCode());
        image2.Should().Be(image1);
        MemoryImage.AreEqual(image1, image2).Should().BeTrue();

        image3.GetHashCode().Should().NotBe(image1.GetHashCode());
        image3.Should().NotBe(image1);
        MemoryImage.AreEqual(image1, image3).Should().BeFalse();
    }
}
