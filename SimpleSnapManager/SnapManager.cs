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
    using System.Text.Json;

    public class SnapshotManager
    {
        private static readonly IReadOnlyDictionary<string, PropertyInfo> _elementProperties =
            typeof(Element).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name);
        // =============================
        //  下面都是单文件内容，不依赖外部类
        // =============================

        private List<Snapshot> _history = new();

        // 添加快照（自动选择：完整快照 or 增量快照）
        public void AddSnapshot(string name, List<List<Element>> data)
        {
            if (_history.Count == 0)
            {
                _history.Add(Snapshot.CreateFull(name, data));
                return;
            }

            var last = _history.Last();
            var diff = DiffArrays(last.Data, data);

            if (!diff.HasDifference)
            {
                Console.WriteLine("没有变化，不存储。");
                return;
            }

            // 存增量（只存 DiffNode）
            _history.Add(Snapshot.CreateDelta(name, diff));
        }

        // 获取还原后的某个版本
        public List<List<Element>> GetSnapshot(int version)
        {
            if (version < 0 || version >= _history.Count)
                throw new Exception("版本不存在");

            List<List<Element>> result = null;

            for (int i = 0; i <= version; i++)
            {
                var snap = _history[i];

                if (snap.IsFull)
                {
                    result = Clone2D(snap.Data);
                }
                else
                {
                    ApplyDiff(result, snap.Diff);
                }
            }

            return result;
        }

        // 导出 JSON（可全量或某版本）
        public string ExportJson(bool allHistory = false, int version = -1)
        {
            if (allHistory)
                return JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });

            if (version >= 0)
                return JsonSerializer.Serialize(_history[version], new JsonSerializerOptions { WriteIndented = true });

            throw new Exception("ExportJson 参数错误");
        }

        // 返回两版本之间的 diff
        public DiffNode DiffVersion(int a, int b)
        {
            return DiffArrays(GetSnapshot(a), GetSnapshot(b));
        }

        // =====================================================================
        //  Snapshot（完整 or 增量）
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

        // =====================================================================
        //  Diff 引擎（自动反射字段级别 diff）
        // =====================================================================
        private DiffNode DiffArrays(List<List<Element>> oldArr, List<List<Element>> newArr)
        {
            var root = new DiffNode { Name = "Root" };

            int maxRows = Math.Max(oldArr.Count, newArr.Count);

            for (int r = 0; r < maxRows; r++)
            {
                if (r >= oldArr.Count)
                {
                    root.Children.Add(new DiffNode { Name = $"Row[{r}]", Type = DiffType.Added });
                    continue;
                }
                if (r >= newArr.Count)
                {
                    root.Children.Add(new DiffNode { Name = $"Row[{r}]", Type = DiffType.Removed });
                    continue;
                }

                var rowNode = new DiffNode { Name = $"Row[{r}]" };

                int maxCols = Math.Max(oldArr[r].Count, newArr[r].Count);

                for (int c = 0; c < maxCols; c++)
                {
                    if (c >= oldArr[r].Count)
                    {
                        rowNode.Children.Add(new DiffNode { Name = $"Col[{c}]", Type = DiffType.Added });
                        continue;
                    }
                    if (c >= newArr[r].Count)
                    {
                        rowNode.Children.Add(new DiffNode { Name = $"Col[{c}]", Type = DiffType.Removed });
                        continue;
                    }

                    var elementNode = DiffElement(oldArr[r][c], newArr[r][c]);
                    elementNode.Name = $"Col[{c}]";

                    if (elementNode.HasDifference)
                        rowNode.Children.Add(elementNode);
                }

                if (rowNode.Children.Count > 0)
                    root.Children.Add(rowNode);
            }

            return root;
        }

        private DiffNode DiffElement(Element oldE, Element newE)
        {
            var node = new DiffNode { Name = "Element" };

            foreach (var p in _elementProperties.Values)
            {
                var ov = p.GetValue(oldE);
                var nv = p.GetValue(newE);

                if (!Equals(ov, nv))
                {
                    node.Children.Add(new DiffNode
                    {
                        Name = p.Name,
                        Type = DiffType.Modified,
                        OldValue = ov,
                        NewValue = nv
                    });
                }
            }

            return node;
        }

        // =====================================================================
        //  工具函数：DeepClone 2D 数组
        // =====================================================================
        private static List<List<Element>> Clone2D(List<List<Element>> src)
        {
            return src.Select(row => row.Select(e => e?.Clone()).ToList()).ToList();
        }

        // =====================================================================
        //  增量恢复（基于 DiffNode 应用修改）
        // =====================================================================
        private void ApplyDiff(List<List<Element>> data, DiffNode diff)
        {
            foreach (var rowNode in diff.Children)
            {
                int rowIndex = ExtractIndex(rowNode.Name);

                if (rowNode.Type == DiffType.Added)
                {
                    data.Add(new List<Element>()); // 你可以改
                    continue;
                }

                if (rowNode.Type == DiffType.Removed)
                {
                    data.RemoveAt(rowIndex);
                    continue;
                }

                foreach (var colNode in rowNode.Children)
                {
                    int colIndex = ExtractIndex(colNode.Name);

                    if (colNode.Type == DiffType.Added)
                    {
                        data[rowIndex].Add(new Element());
                        continue;
                    }

                    if (colNode.Type == DiffType.Removed)
                    {
                        data[rowIndex].RemoveAt(colIndex);
                        continue;
                    }

                    // 修改字段
                    foreach (var fieldNode in colNode.Children)
                    {
                        var e = data[rowIndex][colIndex];
                        if (_elementProperties.TryGetValue(fieldNode.Name, out var prop))
                        {
                            prop.SetValue(e, fieldNode.NewValue);
                        }
                    }
                }
            }
        }

        private int ExtractIndex(string name)
        {
            int a = name.IndexOf('[');
            int b = name.IndexOf(']');
            return int.Parse(name.Substring(a + 1, b - a - 1));
        }
    }

}
