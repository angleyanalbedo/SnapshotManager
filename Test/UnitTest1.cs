using SnapshotManager.core;
using Xunit.Abstractions;

namespace Test
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;
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
        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void Test1()
        {
            var elements = new List<List<ElementBase>>
            {
                new List<ElementBase> { new MyElement { Value = 1 }, new MyElement { Value = 2 } },
                new List<ElementBase> { new MyElement { Value = 3 } }
            };

            var snapshot1 = new ElementArraySnapshot("snapshot1", "初始状态", elements);

            // 修改元素生成新快照 (注意：这里为了演示直接修改了引用，实际场景建议 DeepClone)
            elements[0][0] = new MyElement { Value = 10 };
            var snapshot2 = new ElementArraySnapshot("snapshot2", "修改后状态", elements);

            // 使用 SnapshotManager 管理
            var manager = ElementSnapshotManagerFactory.Create();
            manager.AddSnapshot("s1", snapshot1);
            manager.AddSnapshot("s2", snapshot2);

            // 对比快照 (使用 Diff 而不是 CompareSnapshots)
            var diffNode = manager.Diff("s1", "s2");
            
            // 使用格式化器输出
            var formatter = new StringDiffFormatter();
            _output.WriteLine(formatter.Format(diffNode));
        }
    }
}
