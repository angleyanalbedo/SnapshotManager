using SnapshotManager.Core;
using Xunit.Abstractions;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System;
using SnapshotManager.Models; // 添加 System 引用
using SnapshotManager.Extensions;
using SnapshotManager.Abstruactions;
using SnapshotManager.Output;

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

    // [新增] 新 API 测试类
    public class SnapshotManagerNewApiTests
    {
        // 简单的测试用 Element
        public class TestElement : ElementBase
        {
            public int Value { get; set; }
            public string Name { get; set; } = "";

            public override ElementBase DeepClone()
            {
                return new TestElement { Value = Value, Name = Name };
            }
        }

        [Fact]
        public void TakeSnapshot_And_DiffWith_ShouldWorkCorrectly()
        {
            // 1. 初始化 Manager
            var manager = ElementSnapshotManagerFactory.Create();

            // 2. 准备初始数据
            var data = new List<List<ElementBase>>
            {
                new() { new TestElement { Value = 1, Name = "A" } },
                new() { new TestElement { Value = 2, Name = "B" } }
            };

            // 3. 测试 TakeSnapshot (自动生成 Key)
            string snapKey1 = manager.TakeSnapshot(new MatrixElement(data));
            Assert.False(string.IsNullOrEmpty(snapKey1));

            // 4. 修改内存数据
            // 修改 (0,0) 的值
            ((TestElement)data[0][0]).Value = 999;

            // 5. 测试 DiffWith (实时对比：快照 vs 当前内存数据)
            // 预期：检测到 (0,0) 的变化
            var diffNode = manager.DiffWith(snapKey1, new MatrixElement(data));
            
            Assert.True(diffNode.HasDifference);
            
            // 验证具体差异路径: Row[0] -> Col[0] -> Value
            var rowNode = diffNode.Children.FirstOrDefault(c => c.Name == "Row[0]");
            Assert.NotNull(rowNode);
            var colNode = rowNode.Children.FirstOrDefault(c => c.Name == "Col[0]");
            Assert.NotNull(colNode);
            var valNode = colNode.Children.FirstOrDefault(c => c.Name == "Value");
            Assert.NotNull(valNode);
            Assert.Equal(DiffType.Modified, valNode.Type);
            Assert.Equal(1, valNode.OldValue);
            Assert.Equal(999, valNode.NewValue);

            // 6. 测试 TakeSnapshot (指定 Key)
            manager.TakeSnapshot("v2", new MatrixElement(data));

            // 7. 测试 Diff (历史对比：v1 vs v2)
            var historyDiff = manager.Diff(snapKey1, "v2");
            Assert.True(historyDiff.HasDifference);
            
            // 8. 验证 v2 和当前数据一致 (DiffWith 应该无差异)
            // 注意：这里需要确保 TestElement 的 Equals 逻辑或者 Diff 逻辑能正确处理相同值
            // 由于 ElementDiff 是基于属性反射对比的，只要属性值一样，Diff 就会返回 None
            var noDiff = manager.DiffWith("v2", new MatrixElement(data));
            Assert.False(noDiff.HasDifference);
        }
    }

    // [新增] 扩展方法测试
    public class SnapshotManagerExtensionsTests
    {
        // 辅助类：用于测试的 Element
        public class MyElement : ElementBase
        {
            public int Value { get; set; }

            public override ElementBase DeepClone()
            {
                return new MyElement { Value = this.Value };
            }
        }

        // Mock Formatter for testing
        private class MockDiffFormatter : IDiffFormatter
        {
            public DiffNode? LastReceivedNode { get; private set; }
            public string Format(DiffNode diffNode)
            {
                LastReceivedNode = diffNode;
                return "Formatted by Mock";
            }
        }

        // Mock Printer for testing
        private class MockDiffPrinter : IDiffPrinter
        {
            public DiffNode? LastReceivedNode { get; private set; }
            public int PrintCallCount { get; private set; } = 0;

            public void Print(DiffNode diffNode)
            {
                LastReceivedNode = diffNode;
                PrintCallCount++;
            }
        }

        [Fact]
        public void DiffAndFormat_CallsFormatterWithCorrectDiff()
        {
            // Arrange
            var elements1 = new List<List<ElementBase>> { new() { new MyElement { Value = 10 } } };
            var elements2 = new List<List<ElementBase>> { new() { new MyElement { Value = 99 } } };

            var manager = ElementSnapshotManagerFactory.Create();
            manager.AddSnapshot(new ElementArraySnapshot("s1", "v1", elements1));
            manager.AddSnapshot(new ElementArraySnapshot("s2", "v2", elements2));

            var mockFormatter = new MockDiffFormatter();

            // Act
            var result = manager.DiffAndFormat("s1", "s2", mockFormatter);

            // Assert
            Assert.Equal("Formatted by Mock", result);
            Assert.NotNull(mockFormatter.LastReceivedNode);
            Assert.True(mockFormatter.LastReceivedNode.HasDifference);
        }

        [Fact]
        public void DiffAndPrint_CallsPrinterWithCorrectDiff()
        {
            // Arrange
            var elements1 = new List<List<ElementBase>> { new() { new MyElement { Value = 10 } } };
            var elements2 = new List<List<ElementBase>> { new() { new MyElement { Value = 99 } } };

            var manager = ElementSnapshotManagerFactory.Create();
            manager.AddSnapshot(new ElementArraySnapshot("s1", "v1", elements1));
            manager.AddSnapshot(new ElementArraySnapshot("s2", "v2", elements2));

            var mockPrinter = new MockDiffPrinter();

            // Act
            manager.DiffAndPrint("s1", "s2", mockPrinter);

            // Assert
            Assert.Equal(1, mockPrinter.PrintCallCount);
            Assert.NotNull(mockPrinter.LastReceivedNode);
            Assert.True(mockPrinter.LastReceivedNode.HasDifference);
        }

        [Fact]
        public void DiffWithAndPrint_CallsPrinterWithCorrectDiff()
        {
            // Arrange
            var elements1 = new List<List<ElementBase>> { new() { new MyElement { Value = 10 } } };
            var manager = ElementSnapshotManagerFactory.Create();
            manager.AddSnapshot(new ElementArraySnapshot("s1", "v1", elements1));

            var elements2 = new List<List<ElementBase>> { new() { new MyElement { Value = 99 } } };

            var mockPrinter = new MockDiffPrinter();

            // Act
            manager.DiffWithAndPrint("s1", new MatrixElement(elements2), mockPrinter);

            // Assert
            Assert.Equal(1, mockPrinter.PrintCallCount);
            Assert.NotNull(mockPrinter.LastReceivedNode);
            Assert.True(mockPrinter.LastReceivedNode.HasDifference);
        }

        [Fact]
        public void DiffWithAndFormat_CallsFormatterWithCorrectDiff()
        {
            // Arrange
            var elements1 = new List<List<ElementBase>> { new() { new MyElement { Value = 10 } } };
            var manager = ElementSnapshotManagerFactory.Create();
            manager.AddSnapshot(new ElementArraySnapshot("s1", "v1", elements1));

            var elements2 = new List<List<ElementBase>> { new() { new MyElement { Value = 99 } } };

            var mockFormatter = new MockDiffFormatter();

            // Act
            var result = manager.DiffWithAndFormat("s1", new MatrixElement(elements2), mockFormatter);

            // Assert
            Assert.Equal("Formatted by Mock", result);
            Assert.NotNull(mockFormatter.LastReceivedNode);
            Assert.True(mockFormatter.LastReceivedNode.HasDifference);
        }
    }

    public class FactoryTests
    {
        [Fact]
        public void ElementSnapshotManagerFactory_Create_ShouldReturnWorkingManager()
        {
            // Arrange
            var manager = ElementSnapshotManagerFactory.Create();
            var data = new List<List<ElementBase>> { new() { new ValueElement<int>(1) } };
            var matrix = new MatrixElement(data);

            // Act
            var key = manager.TakeSnapshot(matrix);
            var snapshot = manager.GetSnapshot(key);

            // Assert
            Assert.NotNull(manager);
            Assert.NotNull(snapshot);
            Assert.IsType<ElementArraySnapshot>(snapshot);
            Assert.Equal(1, ((ValueElement<int>)snapshot.Data.Rows[0][0]).Value);
        }

        [Fact]
        public void ContainerSnapshotManagerFactory_CreateListManager_ShouldReturnWorkingManager()
        {
            // Arrange
            var manager = ContainerSnapshotManagerFactory.CreateListManager<int>();
            var list = new List<int> { 1, 2, 3 };
            var element = new PrimitiveListElement<int>(list);

            // Act
            var key = manager.TakeSnapshot(element);
            var snapshot = manager.GetSnapshot(key);

            // Assert
            Assert.NotNull(manager);
            Assert.NotNull(snapshot);
            Assert.IsType<PrimitiveListSnapshot<int>>(snapshot);
            Assert.Equal(3, snapshot.Data.Items.Count);
        }

        [Fact]
        public void ContainerSnapshotManagerFactory_CreateDictionaryManager_ShouldReturnWorkingManager()
        {
            // Arrange
            var manager = ContainerSnapshotManagerFactory.CreateDictionaryManager<string, int>();
            var dict = new Dictionary<string, int> { { "A", 1 } };
            var element = new DictionaryElement<string, int>(dict);

            // Act
            var key = manager.TakeSnapshot(element);
            var snapshot = manager.GetSnapshot(key);

            // Assert
            Assert.NotNull(manager);
            Assert.NotNull(snapshot);
            Assert.IsType<DictionarySnapshot<string, int>>(snapshot);
            Assert.Equal(1, snapshot.Data.Map["A"]);
        }
    }

}
