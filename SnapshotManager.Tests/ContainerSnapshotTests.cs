using System.Collections.Generic;
using SnapshotManager.Core;
using SnapshotManager.Models;
using Xunit;

namespace SnapshotManager.Tests
{
    public class ContainerSnapshotTests
    {
        [Fact]
        public void PrimitiveListSnapshot_Diff_ShouldDetectChanges()
        {
            // Arrange
            var list1 = new List<int> { 1, 2, 3 };
            var list2 = new List<int> { 1, 4, 3 }; // 2 -> 4

            var snap1 = new PrimitiveListSnapshot<int>("snap1", "desc", list1);
            var snap2 = new PrimitiveListSnapshot<int>("snap2", "desc", list2);

            var differ = new PrimitiveListElementDiff<int>();

            // Act
            var diff = differ.Diff(snap1.Data, snap2.Data);

            // Assert
            Assert.True(diff.HasDifference);
            Assert.Equal("List", diff.Name);
            
            // Index[1] changed
            Assert.Contains(diff.Children, c => c.Name == "Index[1]" && c.Type == DiffType.Modified);
            Assert.Equal(2, diff.Children.Find(c => c.Name == "Index[1]")?.OldValue);
            Assert.Equal(4, diff.Children.Find(c => c.Name == "Index[1]")?.NewValue);
        }

        [Fact]
        public void DictionarySnapshot_Diff_ShouldDetectChanges()
        {
            // Arrange
            var dict1 = new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } };
            var dict2 = new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2_mod" }, { "k3", "v3" } };

            var snap1 = new DictionarySnapshot<string, string>("snap1", "desc", dict1);
            var snap2 = new DictionarySnapshot<string, string>("snap2", "desc", dict2);

            var differ = new DictionaryElementDiff<string, string>();

            // Act
            var diff = differ.Diff(snap1.Data, snap2.Data);

            // Assert
            Assert.True(diff.HasDifference);
            Assert.Equal("Dictionary", diff.Name);
            
            // k2 modified
            Assert.Contains(diff.Children, c => c.Name == "Key[k2]" && c.Type == DiffType.Modified);
            Assert.Equal("v2", diff.Children.Find(c => c.Name == "Key[k2]")?.OldValue);
            Assert.Equal("v2_mod", diff.Children.Find(c => c.Name == "Key[k2]")?.NewValue);

            // k3 added
            Assert.Contains(diff.Children, c => c.Name == "Key[k3]" && c.Type == DiffType.Added);
            Assert.Equal("v3", diff.Children.Find(c => c.Name == "Key[k3]")?.NewValue);
        }
    }
}
