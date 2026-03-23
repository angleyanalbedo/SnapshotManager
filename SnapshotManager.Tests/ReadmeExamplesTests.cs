using System;
using System.Collections.Generic;
using System.Linq;
using SnapshotManager.Core;
using SnapshotManager.Extensions;
using SnapshotManager.Models;
using Xunit;

namespace SnapshotManager.Tests
{
    public class ReadmeExamplesTests
    {
        [Fact]
        public void ListManager_ReadmeExample()
        {
            // 1. 创建管理器
            var listManager = ContainerSnapshotManagerFactory.CreateListManager<int>();

            // 2. 准备数据
            var myList = new List<int> { 1, 2, 3 };

            // 3. 创建快照 (注意：需要使用 PrimitiveListElement 包装)
            // TakeSnapshot 返回生成的快照 Key
            string snapKey = listManager.TakeSnapshot(new PrimitiveListElement<int>(myList));

            // 4. 修改数据
            myList.Add(4);
            myList[0] = 99;

            // 5. 实时对比
            var diff = listManager.DiffWith(snapKey, new PrimitiveListElement<int>(myList));

            // Assertions to verify README behavior
            Assert.True(diff.HasDifference);
            
            // Index[0] Modified (1 -> 99)
            var idx0 = diff.Children.FirstOrDefault(c => c.Name == "Index[0]");
            Assert.NotNull(idx0);
            Assert.Equal(DiffType.Modified, idx0.Type);
            Assert.Equal(1, idx0.OldValue);
            Assert.Equal(99, idx0.NewValue);

            // Index[3] Added (4)
            var idx3 = diff.Children.FirstOrDefault(c => c.Name == "Index[3]");
            Assert.NotNull(idx3);
            Assert.Equal(DiffType.Added, idx3.Type);
            Assert.Equal(4, idx3.NewValue);
        }

        [Fact]
        public void DictionaryManager_ReadmeExample()
        {
            // 1. 创建管理器
            var dictManager = ContainerSnapshotManagerFactory.CreateDictionaryManager<string, string>();

            // 2. 准备数据
            var myDict = new Dictionary<string, string> { { "A", "Hello" }, { "B", "World" } };

            // 3. 创建快照 (注意：使用 DictionaryElement 包装)
            string snapKey = dictManager.TakeSnapshot(new DictionaryElement<string, string>(myDict));

            // 4. 修改数据
            myDict["A"] = "Hi";
            myDict.Remove("B");

            // 5. 实时对比
            var diff = dictManager.DiffWith(snapKey, new DictionaryElement<string, string>(myDict));

            // Assertions
            Assert.True(diff.HasDifference);

            // Key[A] Modified
            var keyA = diff.Children.FirstOrDefault(c => c.Name == "Key[A]");
            Assert.NotNull(keyA);
            Assert.Equal(DiffType.Modified, keyA.Type);
            Assert.Equal("Hello", keyA.OldValue);
            Assert.Equal("Hi", keyA.NewValue);

            // Key[B] Removed
            var keyB = diff.Children.FirstOrDefault(c => c.Name == "Key[B]");
            Assert.NotNull(keyB);
            Assert.Equal(DiffType.Removed, keyB.Type);
        }

        [Fact]
        public void MatrixManager_ReadmeExample()
        {
            // 1. 创建管理器
            var matrixManager = ElementSnapshotManagerFactory.Create();

            // 2. 准备数据 (List<List<ElementBase>>)
            var rows = new List<List<ElementBase>>
            {
                new() { new ValueElement<int>(1), new ValueElement<int>(2) },
                new() { new ValueElement<int>(3), new ValueElement<int>(4) }
            };

            // 3. 创建快照 (使用 MatrixElement 包装)
            string snapKey = matrixManager.TakeSnapshot(new MatrixElement(rows));

            // 4. 修改数据
            ((ValueElement<int>)rows[0][0]).Value = 999;

            // 5. 对比
            var diff = matrixManager.DiffWith(snapKey, new MatrixElement(rows));

            // Assertions
            Assert.True(diff.HasDifference);
            
            // Row[0] -> Col[0] -> Value
            var row0 = diff.Children.FirstOrDefault(c => c.Name == "Row[0]");
            Assert.NotNull(row0);
            var col0 = row0.Children.FirstOrDefault(c => c.Name == "Col[0]");
            Assert.NotNull(col0);
            var val = col0.Children.FirstOrDefault(c => c.Name == "Value");
            Assert.NotNull(val);
            
            Assert.Equal(DiffType.Modified, val.Type);
            Assert.Equal(1, val.OldValue);
            Assert.Equal(999, val.NewValue);
        }

        // --- Advanced Usage Classes ---

        public class UserProfile : ElementBase
        {
            public string UserName { get; set; } = "";
            public int Level { get; set; }

            // 必须实现深拷贝，确保快照数据的独立性
            public override ElementBase DeepClone()
            {
                return new UserProfile { UserName = UserName, Level = Level };
            }
        }

        public class UserSnapshot : Snapshot<UserProfile>
        {
            public UserSnapshot(string name, UserProfile data) : base(name, "User Snapshot", data) { }
        }

        [Fact]
        public void AdvancedUsage_ReadmeExample()
        {
            // 初始化管理器
            // 显式传入比较逻辑（这里使用内置的 ElementDiff 进行反射比较）
            var userManager = new SnapshotManager<UserSnapshot, UserProfile>(
                (a, b) => new ElementDiff().Diff(a, b),
                (key, data) => new UserSnapshot(key, data) // 注入快照创建工厂
            );

            // 使用
            var user = new UserProfile { UserName = "Admin", Level = 1 };
            var key = userManager.TakeSnapshot(user);

            user.Level = 2;
            
            var diff = userManager.DiffWith(key, user);

            // Assertions
            Assert.True(diff.HasDifference);
            var levelNode = diff.Children.FirstOrDefault(c => c.Name == "Level");
            Assert.NotNull(levelNode);
            Assert.Equal(DiffType.Modified, levelNode.Type);
            Assert.Equal(1, levelNode.OldValue);
            Assert.Equal(2, levelNode.NewValue);
        }
    }
}
