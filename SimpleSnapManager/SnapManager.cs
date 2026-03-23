using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSnapManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
#if !NET45
    using System.Text.Json;
#endif
    using Core = SnapshotManager.core;

    public class SnapshotManager
    {
        // =============================
        //  下面都是单文件内容，不依赖外部类
        // =============================

        private readonly Core.SnapshotManager<List<List<Core.ElementBase>>> _manager =
            new Core.SnapshotManager<List<List<Core.ElementBase>>>(new Core.ElementArrayDiff());
        private readonly List<string> _snapshotKeys = new List<string>();

        // 添加快照（重写实现）
        public void AddSnapshot(string name, List<List<Element>> data)
        {
            // 使用版本号作为内部 key
            var key = $"v{_snapshotKeys.Count}";
            var snapshot = new Core.ElementArraySnapshot(name, "", ToCoreData(data));
            _manager.AddSnapshot(key, snapshot);
            _snapshotKeys.Add(key);
        }

        // 获取还原后的某个版本（重写实现）
        public List<List<Element>> GetSnapshot(int version)
        {
            if (version < 0 || version >= _snapshotKeys.Count)
                throw new Exception("版本不存在");

            var key = _snapshotKeys[version];
            var snapshot = _manager.GetSnapshot(key);
            return FromCoreData(snapshot.GetData());
        }

#if !NET45
        // 导出 JSON（重写实现）
        public string ExportJson(bool allHistory = false, int version = -1)
        {
            // 为了保持 API 兼容，我们动态构建旧的 Snapshot 结构
            if (allHistory)
            {
                var historyForJson = new List<Snapshot>();
                for (int i = 0; i < _snapshotKeys.Count; i++)
                {
                    var coreSnap = _manager.GetSnapshot(_snapshotKeys[i]);
                    historyForJson.Add(new Snapshot
                    {
                        Name = coreSnap.Name,
                        Time = coreSnap.Timestamp,
                        IsFull = true, // 核心库总是存完整快照
                        Data = FromCoreData(coreSnap.GetData()),
                        Diff = null
                    });
                }
                return JsonSerializer.Serialize(historyForJson, new JsonSerializerOptions { WriteIndented = true });
            }

            if (version >= 0 && version < _snapshotKeys.Count)
            {
                var coreSnap = _manager.GetSnapshot(_snapshotKeys[version]);
                var snapForJson = new Snapshot
                {
                    Name = coreSnap.Name,
                    Time = coreSnap.Timestamp,
                    IsFull = true,
                    Data = FromCoreData(coreSnap.GetData()),
                    Diff = null
                };
                return JsonSerializer.Serialize(snapForJson, new JsonSerializerOptions { WriteIndented = true });
            }

            throw new Exception("ExportJson 参数错误");
        }
#endif

        // 返回两版本之间的 diff（重写实现）
        public DiffNode DiffVersion(int a, int b)
        {
            if (a < 0 || a >= _snapshotKeys.Count || b < 0 || b >= _snapshotKeys.Count)
                throw new Exception("版本不存在");

            var keyA = _snapshotKeys[a];
            var keyB = _snapshotKeys[b];

            var coreDiffNode = _manager.Diff(keyA, keyB);

            // 将 Core.DiffNode 映射回 SimpleSnapManager.DiffNode
            return MapDiffNode(coreDiffNode);
        }

        // =====================================================================
        //  适配器和映射逻辑 (新增)
        // =====================================================================

        // 内部适配器类，桥接 Simple.Element 和 Core.ElementBase
        private class MyElement : Core.ElementBase
        {
            public Element Source { get; }

            public MyElement(Element source)
            {
                Source = source;
            }

            public override Core.ElementBase DeepClone()
            {
                return new MyElement(Source.Clone());
            }

            // 使用核心库的默认反射比较机制
            public override Core.DiffNode Diff(Core.ElementBase? oldValue, Core.ElementBase? newValue)
            {
                var oldMy = oldValue as MyElement;
                var newMy = newValue as MyElement;
                // 注意：这里我们比较的是 Source，即原始的 SimpleSnapManager.Element
                return base.Diff(oldMy?.Source, newMy?.Source);
            }
        }

        // 将 Core.DiffNode 递归映射到 SimpleSnapManager.DiffNode
        private DiffNode MapDiffNode(Core.DiffNode coreNode)
        {
            if (coreNode == null) return null;

            var node = new DiffNode
            {
                Name = coreNode.Name,
                Type = (DiffType)coreNode.Type, // 枚举值是对应的
                OldValue = coreNode.OldValue,
                NewValue = coreNode.NewValue,
                Children = coreNode.Children.Select(MapDiffNode).ToList()
            };
            return node;
        }

        // 数据转换
        private List<List<Core.ElementBase>> ToCoreData(List<List<Element>> data)
        {
            return data.Select(row => row.Select(e => e == null ? null : new MyElement(e)).Cast<Core.ElementBase>().ToList()).ToList();
        }

        private List<List<Element>> FromCoreData(List<List<Core.ElementBase>> coreData)
        {
            return coreData.Select(row => row.Select(e => (e as MyElement)?.Source).ToList()).ToList();
        }


        // =====================================================================
        //  Snapshot（完整 or 增量） - 保持不变，用于 ExportJson
        // =====================================================================
        public class Snapshot
        {
            public string Name { get; set; }
            public DateTime Time { get; set; } = DateTime.Now;

            public bool IsFull { get; set; }

            // 完整快照
            public List<List<Element>> Data { get; set; }

            // 增量（树状 diff）
            public DiffNode Diff { get; set; }

            public static Snapshot CreateFull(string name, List<List<Element>> data)
            {
                return new Snapshot
                {
                    Name = name,
                    IsFull = true,
                    Data = Clone2D(data)
                };
            }

            public static Snapshot CreateDelta(string name, DiffNode diff)
            {
                return new Snapshot
                {
                    Name = name,
                    IsFull = false,
                    Diff = diff
                };
            }
        }

        // =====================================================================
        //  Element（你原来的 Element，但加入 DeepClone）
        // =====================================================================
        public class Element
        {
  
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime Time { get; set; }

            public Element Clone()
            {
                return (Element)this.MemberwiseClone();
            }
        }

        // =====================================================================
        //  DiffNode（像 Git 的增量变化树）
        // =====================================================================
        public class DiffNode
        {
            public string Name { get; set; }
            public DiffType Type { get; set; } = DiffType.None;

            public object OldValue { get; set; }
            public object NewValue { get; set; }

            public List<DiffNode> Children { get; set; } = new();

            public bool HasDifference =>
                Type != DiffType.None || Children.Any(c => c.HasDifference);
        }

        public enum DiffType
        {
            None,
            Added,
            Removed,
            Modified
        }

    }

}
