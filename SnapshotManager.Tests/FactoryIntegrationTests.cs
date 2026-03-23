using System.Collections.Generic;
using SnapshotManager.Core;
using SnapshotManager.Models;
using Xunit;
using System.Linq;

namespace SnapshotManager.Tests
{
    public class FactoryIntegrationTests
    {
        [Fact]
        public void ListManager_ShouldHandleSimpleIntList()
        {
            // Arrange
            // 1. 使用工厂创建针对 List<int> 的管理器
            var manager = ContainerSnapshotManagerFactory.CreateListManager<int>();
            
            // 2. 准备初始数据
            var initialData = new List<int> { 1, 2, 3 };
            
            // 3. 创建快照 (需要包装为 PrimitiveListElement)
            var snapKey1 = manager.TakeSnapshot(new PrimitiveListElement<int>(initialData));
            
            // 4. 修改数据
            initialData.Add(4);       // 新增
            initialData[0] = 99;      // 修改

            // Act
            // 5. 与当前数据进行对比
            var diff = manager.DiffWith(snapKey1, new PrimitiveListElement<int>(initialData));

            // Assert
            Assert.True(diff.HasDifference);
            
            // 验证索引 0 的修改
            var modNode = diff.Children.FirstOrDefault(c => c.Name == "Index[0]");
            Assert.NotNull(modNode);
            Assert.Equal(DiffType.Modified, modNode.Type);
            Assert.Equal(1, modNode.OldValue);
            Assert.Equal(99, modNode.NewValue);

            // 验证索引 3 的新增
            var addNode = diff.Children.FirstOrDefault(c => c.Name == "Index[3]");
            Assert.NotNull(addNode);
            Assert.Equal(DiffType.Added, addNode.Type);
            Assert.Equal(4, addNode.NewValue);
        }

        [Fact]
        public void DictionaryManager_ShouldHandleStringIntDictionary()
        {
            // Arrange
            // 1. 使用工厂创建针对 Dictionary<string, int> 的管理器
            var manager = ContainerSnapshotManagerFactory.CreateDictionaryManager<string, int>();
            
            // 2. 准备初始数据
            var initialData = new Dictionary<string, int> 
            { 
                { "A", 10 }, 
                { "B", 20 } 
            };

            // 3. 创建快照
            var snapKey1 = manager.TakeSnapshot(new DictionaryElement<string, int>(initialData));

            // 4. 修改数据
            initialData["A"] = 15;   // 修改
            initialData.Remove("B"); // 删除
            initialData["C"] = 30;   // 新增

            // Act
            var diff = manager.DiffWith(snapKey1, new DictionaryElement<string, int>(initialData));

            // Assert
            Assert.True(diff.HasDifference);

            // 验证 Key[A] 修改
            var modNode = diff.Children.FirstOrDefault(c => c.Name == "Key[A]");
            Assert.NotNull(modNode);
            Assert.Equal(DiffType.Modified, modNode.Type);
            Assert.Equal(10, modNode.OldValue);
            Assert.Equal(15, modNode.NewValue);

            // 验证 Key[B] 删除
            var remNode = diff.Children.FirstOrDefault(c => c.Name == "Key[B]");
            Assert.NotNull(remNode);
            Assert.Equal(DiffType.Removed, remNode.Type);

            // 验证 Key[C] 新增
            var addNode = diff.Children.FirstOrDefault(c => c.Name == "Key[C]");
            Assert.NotNull(addNode);
            Assert.Equal(DiffType.Added, addNode.Type);
        }

        [Fact]
        public void ElementManager_ShouldHandleComplexMatrix()
        {
            // Arrange
            // 1. 使用工厂创建针对 MatrixElement 的管理器
            var manager = ElementSnapshotManagerFactory.Create();
            
            // 2. 准备初始数据 (2x2 矩阵)
            var row1 = new List<ElementBase> { new ValueElement<string>("r1c1"), new ValueElement<string>("r1c2") };
            var row2 = new List<ElementBase> { new ValueElement<string>("r2c1"), new ValueElement<string>("r2c2") };
            var matrixData = new List<List<ElementBase>> { row1, row2 };

            // 3. 创建快照
            var snapKey1 = manager.TakeSnapshot(new MatrixElement(matrixData));

            // 4. 修改数据
            // 修改 (1,1) 的值: r2c2 -> modified
            ((ValueElement<string>)matrixData[1][1]).Value = "modified";
            
            // 新增一行
            matrixData.Add(new List<ElementBase> { new ValueElement<string>("r3c1") });

            // Act
            var diff = manager.DiffWith(snapKey1, new MatrixElement(matrixData));

            // Assert
            Assert.True(diff.HasDifference);

            // 验证 Row[1] -> Col[1] -> Value 的修改
            // 注意：MatrixElementDiff 内部使用 ElementDiff，ElementDiff 通过反射比较属性
            var row1Node = diff.Children.FirstOrDefault(c => c.Name == "Row[1]");
            Assert.NotNull(row1Node);
            
            var col1Node = row1Node.Children.FirstOrDefault(c => c.Name == "Col[1]");
            Assert.NotNull(col1Node);
            
            var valNode = col1Node.Children.FirstOrDefault(c => c.Name == "Value");
            Assert.NotNull(valNode);
            Assert.Equal(DiffType.Modified, valNode.Type);
            Assert.Equal("r2c2", valNode.OldValue);
            Assert.Equal("modified", valNode.NewValue);

            // 验证 Row[2] 的新增
            var row2Node = diff.Children.FirstOrDefault(c => c.Name == "Row[2]");
            Assert.NotNull(row2Node);
            Assert.Equal(DiffType.Added, row2Node.Type);
        }

        [Fact]
        public void HashSetManager_ShouldHandleSimpleIntSet()
        {
            // Arrange
            // 1. 使用工厂创建针对 HashSet<int> 的管理器
            var manager = ContainerSnapshotManagerFactory.CreateHashSetManager<int>();

            // 2. 准备初始数据
            var initialData = new HashSet<int> { 1, 2, 3 };

            // 3. 创建快照
            var snapKey1 = manager.TakeSnapshot(new HashSetElement<int>(initialData));

            // 4. 修改数据
            initialData.Add(4);       // 新增
            initialData.Remove(2);    // 删除

            // Act
            var diff = manager.DiffWith(snapKey1, new HashSetElement<int>(initialData));

            // Assert
            Assert.True(diff.HasDifference);

            // 验证 Item[2] 删除
            var remNode = diff.Children.FirstOrDefault(c => c.Name == "Item[2]");
            Assert.NotNull(remNode);
            Assert.Equal(DiffType.Removed, remNode.Type);

            // 验证 Item[4] 新增
            var addNode = diff.Children.FirstOrDefault(c => c.Name == "Item[4]");
            Assert.NotNull(addNode);
            Assert.Equal(DiffType.Added, addNode.Type);
        }
    }
}
