using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBrix.Graphics3D.Gltf2.Memory;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Schema2.LoadAndSave; //was previously: SharpGLTF.Schema2.LoadAndSave;

public class ReadContextTests
{
    [Fact]
    public void TestLoadWithReadContext()
    {
        var modelPath = ResourceInfo.From("SpecialCases/RelativePaths.gltf");

        var baseDirectory = modelPath.File.Directory;

        ArraySegment<byte> _fileReader(string assetName)
        {
            assetName = Uri.UnescapeDataString(assetName).Replace('\\', '/');

            var filePath = System.IO.Path.Combine(baseDirectory.FullName, assetName);
            var finfo = new System.IO.FileInfo(filePath);

            if (finfo.Exists)
            {
                var data = System.IO.File.ReadAllBytes(filePath);

                return new ArraySegment<byte>(data);
            }

            throw new System.IO.FileNotFoundException(filePath);
        }

        var context = ReadContext.Create(_fileReader);

        var model = ModelRoot.Load("RelativePaths.gltf", context);
    }

    [Fact]
    public void TestLoadWithReadContextAndTextureLoadSkip()
    {
        var modelPath = ResourceInfo.From("SpecialCases/RelativePaths.gltf");

        var baseDirectory = modelPath.File.Directory;

        // we store the uris for the tests at the end.
        var imageUriCache = new HashSet<string>();

        // this is a very tiny, placeholder PNG image to be used as replacement
        // of the actual image. This ensures a valid image is passed to the model for
        // the tests to pass.
        var placeHolderImage = new ArraySegment<byte>(PngFormatInfo.DefaultPngImage);

        ArraySegment<byte> _fileReader(string assetName)
        {
            assetName = Uri.UnescapeDataString(assetName);

            var filePath = System.IO.Path.Combine(baseDirectory.FullName, assetName);
            var finfo = new System.IO.FileInfo(filePath);

            // skip image loading by replacing it with a tiny placeholder
            if (finfo.Extension == ".jpg" || finfo.Extension == ".png")
            {
                imageUriCache.Add(assetName);
                return placeHolderImage;
            }

            if (finfo.Exists)
            {
                var data = System.IO.File.ReadAllBytes(filePath);

                return new ArraySegment<byte>(data);
            }

            throw new System.IO.FileNotFoundException(filePath);
        }

        // load model and ensure the placeholder images contain proper paths

        var context = ReadContext.Create(_fileReader);

        var model = ModelRoot.Load("RelativePaths.gltf", context);

        foreach(var img in model.LogicalImages)
        {
            var bitmap = img.Content; // Content is a MemoryImage object that countains a bitmap SourcePath.

            bitmap.SourcePath.Should().NotBeNull();

            var srcPath = Uri.UnescapeDataString(bitmap.SourcePath);

            imageUriCache.Contains(srcPath).Should().BeTrue();
        }
    }

    [Fact]
    public void CreateFromDirectory_resolves_backslash_separated_uris()
    {
        //Arrange
        var modelPath = ResourceInfo.From("SpecialCases/RelativePaths.gltf");
        var context = ReadContext.CreateFromDirectory(modelPath.File.Directory);

        //Act
        var forward = context.ReadAllBytesToEnd("Textures/Texture1.jpg");
        var backslash = context.ReadAllBytesToEnd("Textures\\Texture1.jpg");
        var escaped = context.ReadAllBytesToEnd("Textures%5CTexture1.jpg");

        //Assert
        forward.Count.Should().BeGreaterThan(0);
        backslash.Should().Equal(forward);
        escaped.Should().Equal(forward);
    }
}
