using SnapshotManager.core.@interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core
{
    public enum DiffKind
    {
        Added,
        Removed,
        Modified
    }
    /// <summary>
    /// 树状 Diff 结果
    /// </summary>
    public class DiffNode
    {
        public string Name { get; set; } = "";
        public DiffType Type { get; set; } = DiffType.None;

        public object? OldValue { get; set; }
        public object? NewValue { get; set; }

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

    public class DiffItem
    {
        /// <summary>
        /// 差异类型（新增、删除、修改）
        /// </summary>
        public DiffKind Kind { get; set; }

        /// <summary>
        /// 差异的路径（如："Row[3].Column[2]" 或 "Element.Address"）
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 旧值
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// 新值
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// 额外数据（可扩展，有些场景非常有用）
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        public DiffItem()
        {
            Metadata = new Dictionary<string, object>();
            Path = string.Empty;
            Kind = DiffKind.Modified;
            NewValue = null;
            OldValue = null;
        }
        public override string ToString()
        {
            return $"{Kind} @ {Path}: {OldValue} → {NewValue}";
        }
    }

    public class DiffResult
    {
        public bool HasDifference => Items.Count > 0;

        public List<DiffItem> Items { get; } = new();

        public void Add(DiffItem item)
        {
            Items.Add(item);
        }

        public void Add(DiffKind kind, string path, object oldValue, object newValue)
        {
            Items.Add(new DiffItem()
            {
                Kind = kind,
                Path = path,
                OldValue = oldValue,
                NewValue = newValue
            });
        }

        public void Add(string description)
        {
            Items.Add(new DiffItem()
            {
                Kind = DiffKind.Modified,
                Path = "",
                OldValue = null,
                NewValue = description
            });
        }
    }

    public abstract class DiffBase<T> : ICompare<T>
    {
        public abstract DiffResult Compare(T oldValue, T newValue);

        protected void AddIfDifferent(DiffResult result, string name, object a, object b)
        {
            if (!Equals(a, b))
                result.Add($"{name}: {a} → {b}");
        }
    }

    public abstract class Diff<T> : ICompare<T>
    {
        public abstract DiffResult Compare(T oldValue, T newValue);

        protected void AddIfDifferent(DiffResult result, string name, object a, object b)
        {
            if (!Equals(a, b))
                result.Add($"{name}: {a} → {b}");
        }
    }

    public class ListDiff<T> : DiffBase<List<T>>
    {
        private readonly ICompare<T> _elementDiff;

        public ListDiff(ICompare<T> elementDiff)
        {
            _elementDiff = elementDiff;
        }

        public override DiffResult Compare(List<T> oldList, List<T> newList)
        {
            var result = new DiffResult();

            int max = Math.Max(oldList.Count, newList.Count);

            for (int i = 0; i < max; i++)
            {
                T oldItem = i < oldList.Count ? oldList[i] : default;
                T newItem = i < newList.Count ? newList[i] : default;

                var r = _elementDiff.Compare(oldItem, newItem);
                foreach (var item in r.Items)
                    result.Add($"[Index {i}] {item}");
            }

            return result;
        }
    }
    public class MatrixDiff<T> : DiffBase<List<List<T>>>
    {
        private readonly ICompare<T> _elementDiff;

        public MatrixDiff(ICompare<T> elementDiff)
        {
            _elementDiff = elementDiff;
        }

        public override DiffResult Compare(List<List<T>> oldMatrix, List<List<T>> newMatrix)
        {
            var result = new DiffResult();

            int maxRows = Math.Max(oldMatrix.Count, newMatrix.Count);

            for (int i = 0; i < maxRows; i++)
            {
                var rowA = i < oldMatrix.Count ? oldMatrix[i] : null;
                var rowB = i < newMatrix.Count ? newMatrix[i] : null;

                if (rowA == null || rowB == null)
                {
                    result.Add($"Row {i}: Added/Removed");
                    continue;
                }

                int maxCols = Math.Max(rowA.Count, rowB.Count);

                for (int j = 0; j < maxCols; j++)
                {
                    var a = j < rowA.Count ? rowA[j] : default;
                    var b = j < rowB.Count ? rowB[j] : default;

                    var r = _elementDiff.Compare(a, b);
                    foreach (var item in r.Items)
                        result.Add($"({i},{j}) {item}");
                }
            }

            return result;
        }
    }

    public static class ElementArrayDiff
    {
        public static DiffNode Diff(
            List<List<ElementBase>> oldArr,
            List<List<ElementBase>> newArr)
        {
            var root = new DiffNode { Name = "ElementArray" };

            int maxRow = Math.Max(oldArr.Count, newArr.Count);
            for (int r = 0; r < maxRow; r++)
            {
                if (r >= oldArr.Count)
                {
                    root.Children.Add(new DiffNode
                    {
                        Name = $"Row[{r}]",
                        Type = DiffType.Added,
                    });
                    continue;
                }

                if (r >= newArr.Count)
                {
                    root.Children.Add(new DiffNode
                    {
                        Name = $"Row[{r}]",
                        Type = DiffType.Removed,
                    });
                    continue;
                }

                // 比较每行
                var rowNode = DiffRow(oldArr[r], newArr[r], r);
                if (rowNode.HasDifference)
                    root.Children.Add(rowNode);
            }

            return root;
        }

        private static DiffNode DiffRow(
            List<ElementBase> oldRow,
            List<ElementBase> newRow,
            int rowIndex)
        {
            var rowNode = new DiffNode { Name = $"Row[{rowIndex}]" };

            int maxCol = Math.Max(oldRow.Count, newRow.Count);

            for (int c = 0; c < maxCol; c++)
            {
                if (c >= oldRow.Count)
                {
                    rowNode.Children.Add(new DiffNode
                    {
                        Name = $"Col[{c}]",
                        Type = DiffType.Added
                    });
                    continue;
                }

                if (c >= newRow.Count)
                {
                    rowNode.Children.Add(new DiffNode
                    {
                        Name = $"Col[{c}]",
                        Type = DiffType.Removed
                    });
                    continue;
                }

                var cellNode = oldRow[c].Diff(oldRow[c], newRow[c]);
                cellNode.Name = $"Col[{c}]";

                if (cellNode.HasDifference)
                    rowNode.Children.Add(cellNode);
            }

            return rowNode;
        }
    }


}
