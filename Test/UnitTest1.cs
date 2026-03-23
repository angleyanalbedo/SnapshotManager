using SnapshotManager.core;
using SnapshotManager.core.@interface;
using Xunit.Abstractions;
using System.Collections.Generic;
using Xunit;
using System.Linq;

namespace SnapshotManager.Tests
{
    public class SnapshotManagerTests
    {
        private readonly ITestOutputHelper _output;

        // 辅助类：用于测试的 Element
        public class MyElement : ElementBase
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }

            public override ElementBase DeepClone()
            {
                return new MyElement
                {
                    Name = this.Name,
                    Value = this.Value
                };
            }
        }

        public SnapshotManagerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Diff_IdenticalSnapshots_ShouldHaveNoDifference()
        {
            // Arrange
            var elements = new List<List<ElementBase>>
            {
                new() { new MyElement { Name = "A", Value = 1 } }
            };
            
            var snapshot1 = new ElementArraySnapshot("s1", "Original", elements);
            // 模拟完全相同的数据 (引用相同)
            var snapshot2 = new ElementArraySnapshot("s2", "Copy", elements); 

            var manager = ElementSnapshotManagerFactory.Create();
            manager.AddSnapshot(snapshot1);
            manager.AddSnapshot(snapshot2);

            // Act
            var diff = manager.Diff("s1", "s2");
            
            // Assert
            Assert.False(diff.HasDifference, "完全相同的快照不应检测出差异");
        }

        [Fact]
        public void Diff_ValueChange_ShouldBeDetected()
        {
            // Arrange
            var elements1 = new List<List<ElementBase>> { new() { new MyElement { Value = 10 } } };
            var elements2 = new List<List<ElementBase>> { new() { new MyElement { Value = 99 } } };

            var manager = ElementSnapshotManagerFactory.Create();
            manager.AddSnapshot(new ElementArraySnapshot("s1", "v1", elements1));
            manager.AddSnapshot(new ElementArraySnapshot("s2", "v2", elements2));

            // Act
            var diff = manager.Diff("s1", "s2");
            
            // Assert
            Assert.True(diff.HasDifference);
            
            // 验证路径: Matrix -> Row[0] -> Col[0] -> Value
            var rowNode = diff.Children.FirstOrDefault(c => c.Name == "Row[0]");
            Assert.NotNull(rowNode);
            
            var colNode = rowNode.Children.FirstOrDefault(c => c.Name == "Col[0]");
            Assert.NotNull(colNode);

            var valNode = colNode.Children.FirstOrDefault(c => c.Name == "Value");
            Assert.NotNull(valNode);
            
            Assert.Equal(DiffType.Modified, valNode.Type);
            Assert.Equal(10, valNode.OldValue);
            Assert.Equal(99, valNode.NewValue);
        }

        [Fact]
        public void Diff_StructureChange_AddRow_ShouldBeDetected()
        {
            // Arrange
            var elements1 = new List<List<ElementBase>>
            {
                new() { new MyElement { Name = "Row1" } }
            };

            var elements2 = new List<List<ElementBase>>
            {
                new() { new MyElement { Name = "Row1" } },
                new() { new MyElement { Name = "Row2_Added" } } // 新增行
            };

            var manager = ElementSnapshotManagerFactory.Create();
            manager.AddSnapshot(new ElementArraySnapshot("base", "base", elements1));
            manager.AddSnapshot(new ElementArraySnapshot("added", "added", elements2));

            // Act
            var diff = manager.Diff("base", "added");

            // Assert
            Assert.True(diff.HasDifference);
            var addedRow = diff.Children.FirstOrDefault(c => c.Name == "Row[1]");
            Assert.NotNull(addedRow);
            Assert.Equal(DiffType.Added, addedRow.Type);
        }

        [Fact]
        public void Diff_StructureChange_ColumnMismatch_ShouldBeDetected()
        {
            // Arrange
            // s1: 1行1列
            var s1Data = new List<List<ElementBase>> 
            { 
                new() { new MyElement { Value = 1 } }
            };

            // s2: 1行2列 (新增一列)
            var s2Data = new List<List<ElementBase>>
            {
                new() { new MyElement { Value = 1 }, new MyElement { Value = 99 } }
            };

            var manager = ElementSnapshotManagerFactory.Create();
            manager.AddSnapshot(new ElementArraySnapshot("s1", "", s1Data));
            manager.AddSnapshot(new ElementArraySnapshot("s2", "", s2Data));

            // Act
            var diff = manager.Diff("s1", "s2");

            // Assert
            Assert.True(diff.HasDifference);

            // 检查 Row[0] -> Col[1] (Added)
            var row0 = diff.Children.First(c => c.Name == "Row[0]");
            var col1 = row0.Children.FirstOrDefault(c => c.Name == "Col[1]");
            
            Assert.NotNull(col1);
            Assert.Equal(DiffType.Added, col1.Type);
        }
    }
}
