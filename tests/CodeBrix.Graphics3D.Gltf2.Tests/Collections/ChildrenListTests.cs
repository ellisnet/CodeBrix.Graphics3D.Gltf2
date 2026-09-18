using System;
using System.Collections.Generic;
using System.Text;
using SilverAssertions;
using Xunit;

namespace CodeBrix.Graphics3D.Gltf2.Collections; //was previously: SharpGLTF.Collections;

public class ChildrenListTests
{
    class TestChild : IChildOfList<ChildrenListTests>
    {
        public ChildrenListTests LogicalParent { get; private set; }

        public int LogicalIndex { get; private set; } = -1;

        void IChildOfList<ChildrenListTests>.SetLogicalParent(ChildrenListTests parent, int index)
        {
            LogicalParent = parent;
            LogicalIndex = index;
        }
    }

    [Fact]
    public void TestChildCollectionList1()
    {
        var list = new ChildrenList<TestChild, ChildrenListTests>(this);

        Assert.Throws<ArgumentNullException>(() => list.Add(null));

        var item1 = new TestChild();
        item1.LogicalParent.Should().BeNull();
        item1.LogicalIndex.Should().Be(-1);

        var item2 = new TestChild();
        item2.LogicalParent.Should().BeNull();
        item2.LogicalIndex.Should().Be(-1);

        list.Add(item1);
        item1.LogicalParent.Should().BeSameAs(this);
        item1.LogicalIndex.Should().Be(0);

        Assert.Throws<ArgumentException>(() => list.Add(item1));

        list.Remove(item1);
        item1.LogicalParent.Should().BeNull();
        item1.LogicalIndex.Should().Be(-1);

        list.Add(item1);
        item1.LogicalParent.Should().BeSameAs(this);
        item1.LogicalIndex.Should().Be(0);

        list.Insert(0, item2);
        item2.LogicalParent.Should().BeSameAs(this);
        item2.LogicalIndex.Should().Be(0);
        item1.LogicalParent.Should().BeSameAs(this);
        item1.LogicalIndex.Should().Be(1);

        list.RemoveAt(0);
        item2.LogicalParent.Should().BeNull();
        item2.LogicalIndex.Should().Be(-1);
        item1.LogicalParent.Should().BeSameAs(this);
        item1.LogicalIndex.Should().Be(0);

    }

}
