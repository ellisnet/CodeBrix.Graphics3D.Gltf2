using System;
using System.Collections.Generic;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Collections; //was previously: SharpGLTF.Collections;

public class ChildrenDictionaryTests
{
    class TestChild : IChildOfDictionary<ChildrenDictionaryTests>
    {
        public ChildrenDictionaryTests LogicalParent { get; private set; }

        public string LogicalKey { get; private set; }

        void IChildOfDictionary<ChildrenDictionaryTests>.SetLogicalParent(ChildrenDictionaryTests parent, string key)
        {
            LogicalParent = parent;
            LogicalKey = key;
        }
    }

    [Fact]
    public void TestChildCollectionDictionary1()
    {
        var dict = new ChildrenDictionary<TestChild, ChildrenDictionaryTests>(this);

        Assert.Throws<ArgumentNullException>(() => dict.Add("key", null));

        var item1 = new TestChild();
        item1.LogicalParent.Should().BeNull();
        item1.LogicalKey.Should().BeNull();

        var item2 = new TestChild();
        item2.LogicalParent.Should().BeNull();
        item2.LogicalKey.Should().BeNull();

        dict["0"] = item1;
        item1.LogicalParent.Should().BeSameAs(this);
        item1.LogicalKey.Should().Be("0");

        Assert.Throws<ArgumentException>(() => dict.Add("1", item1));

        dict.Remove("0");
        item1.LogicalParent.Should().BeNull();
        item1.LogicalKey.Should().BeNull();

        dict["0"] = item1;
        item1.LogicalParent.Should().BeSameAs(this);
        item1.LogicalKey.Should().Be("0");

        dict["1"] = item2;
        item1.LogicalParent.Should().BeSameAs(this);
        item1.LogicalKey.Should().Be("0");
        item2.LogicalParent.Should().BeSameAs(this);
        item2.LogicalKey.Should().Be("1");
    }

}
