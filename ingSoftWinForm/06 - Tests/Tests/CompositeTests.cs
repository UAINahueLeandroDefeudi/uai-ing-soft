using BE.Entity;
using Xunit;
using static Tests.Samples;

namespace Tests
{
    public class CompositeTests
    {
        [Fact]
        public void Leaf_Add_Throws()
        {
            var leaf = Simple("A");
            Assert.Throws<InvalidOperationException>(() => leaf.Add(Simple("B")));
        }

        [Fact]
        public void Leaf_Remove_Throws()
        {
            var leaf = Simple("A");
            Assert.Throws<InvalidOperationException>(() => leaf.Remove(Simple("B")));
        }

        [Fact]
        public void Leaf_HasNoChildren()
            => Assert.Empty(Simple("A").GetChildren());

        [Fact]
        public void Compound_Add_IgnoresDuplicates()
        {
            var compound = Compound("C", Simple("A"));
            compound.Add(Simple("A"));

            Assert.Single(compound.GetChildren());
        }

        [Fact]
        public void Compound_Remove_RemovesChild()
        {
            var a = Simple("A");
            var compound = Compound("C", a, Simple("B"));

            compound.Remove(a);

            Assert.Equal("B", Assert.Single(compound.GetChildren()).Code);
        }

        [Fact]
        public void Compound_Add_Self_ThrowsCycle()
        {
            var compound = Compound("C");
            Assert.Throws<InvalidOperationException>(() => compound.Add(compound));
        }

        [Fact]
        public void Compound_Add_Ancestor_ThrowsCycle()
        {
            var inner = Compound("INNER");
            var outer = Compound("OUTER", inner);

            Assert.Throws<InvalidOperationException>(() => inner.Add(outer));
        }

        [Fact]
        public void Grants_FindsDirectAndNestedPermission()
        {
            var tree = Compound("ROOT", Simple("A"), Compound("MID", Simple("DEEP")));

            Assert.True(tree.Grants("ROOT"));
            Assert.True(tree.Grants("A"));
            Assert.True(tree.Grants("DEEP"));
            Assert.False(tree.Grants("OTHER"));
        }

        [Fact]
        public void Flatten_ReturnsOnlyLeaves()
        {
            var tree = Compound("ROOT", Simple("A"), Compound("MID", Simple("B"), Simple("C")));

            var codes = tree.Flatten().Select(p => p.Code).ToList();

            Assert.Equal(new[] { "A", "B", "C" }, codes);
        }
    }
}
