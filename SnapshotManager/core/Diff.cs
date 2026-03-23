using SnapshotManager.core.@interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SnapshotManager.core
{
    /// <summary>
    /// 树状 Diff 结果
    /// </summary>
    public class DiffNode
    {
        /// <summary>
        /// 节点名称（如属性名、索引等）。
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 差异类型（新增、删除、修改、无变化）。
        /// </summary>
        public DiffType Type { get; set; } = DiffType.None;

        /// <summary>
        /// 原始值。
        /// </summary>
        public object? OldValue { get; set; }

        /// <summary>
        /// 新值。
        /// </summary>
        public object? NewValue { get; set; }

        /// <summary>
        /// 子节点列表（用于表示嵌套结构的差异）。
        /// </summary>
        public List<DiffNode> Children { get; set; } = new();

        /// <summary>
        /// 检查当前节点或其子节点是否存在差异。
        /// </summary>
        public bool HasDifference =>
            Type != DiffType.None || Children.Any(c => c.HasDifference);
    }

    /// <summary>
    /// 差异类型枚举。
    /// </summary>
    public enum DiffType
    {
        /// <summary>
        /// 无变化。
        /// </summary>
        None,
        /// <summary>
        /// 新增。
        /// </summary>
        Added,
        /// <summary>
        /// 移除。
        /// </summary>
        Removed,
        /// <summary>
        /// 修改。
        /// </summary>
        Modified
    }


    /// <summary>
    /// 列表差异比较器。
    /// </summary>
    /// <typeparam name="T">列表元素类型。</typeparam>
    public class ListDiff<T> : IDiff<List<T>>
    {
        private readonly IDiff<T> _elementDiff;

        /// <summary>
        /// 初始化列表差异比较器。
        /// </summary>
        /// <param name="elementDiff">元素比较器。</param>
        public ListDiff(IDiff<T> elementDiff)
        {
            _elementDiff = elementDiff;
        }

        /// <inheritdoc />
        public DiffNode Diff(List<T>? oldList, List<T>? newList)
        {
            var root = new DiffNode { Name = "List" };
            oldList ??= new List<T>();
            newList ??= new List<T>();

            int max = Math.Max(oldList.Count, newList.Count);

            for (int i = 0; i < max; i++)
            {
                // 处理列表长度变化
                if (i >= oldList.Count)
                {
                    root.Children.Add(new DiffNode { Name = $"Index[{i}]", Type = DiffType.Added, NewValue = newList[i] });
                    continue;
                }
                if (i >= newList.Count)
                {
                    root.Children.Add(new DiffNode { Name = $"Index[{i}]", Type = DiffType.Removed, OldValue = oldList[i] });
                    continue;
                }

                // 比较元素
                T oldItem = oldList[i];
                T newItem = newList[i];

                var childNode = _elementDiff.Diff(oldItem, newItem);
                
                // 如果子节点有差异，加入树中
                if (childNode.HasDifference)
                {
                    childNode.Name = $"Index[{i}]"; // 覆盖名称以显示索引
                    root.Children.Add(childNode);
                }
            }

            return root;
        }
    }
    /// <summary>
    /// 基于反射的 ElementBase 差异比较器。
    /// </summary>
    public class ElementDiff : IDiff<ElementBase>
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        /// <summary>
        /// 自动反射比较（通用的字段级 Diff）
        /// </summary>
        public DiffNode Diff(ElementBase? oldValue, ElementBase? newValue)
        {
            var node = new DiffNode
            {
                Name = oldValue?.GetType().Name ?? newValue?.GetType().Name ?? "Element"
            };

            if (oldValue == null && newValue != null)
            {
                node.Type = DiffType.Added;
                return node;
            }
            if (oldValue != null && newValue == null)
            {
                node.Type = DiffType.Removed;
                return node;
            }
            if (oldValue == null && newValue == null)
            {
                return node;
            }

            var properties = _propertyCache.GetOrAdd(
                oldValue!.GetType(),
                t => t.GetProperties().Where(p => p.CanRead).ToArray());

            foreach (var prop in properties)
            {
                var oldVal = prop.GetValue(oldValue);
                var newVal = prop.GetValue(newValue);

                if (!Equals(oldVal, newVal))
                {
                    node.Children.Add(new DiffNode
                    {
                        Name = prop.Name,
                        Type = DiffType.Modified,
                        OldValue = oldVal,
                        NewValue = newVal
                    });
                }
            }

            return node;
        }
    }

    /// <summary>
    /// 二维矩阵差异比较器。
    /// </summary>
    /// <typeparam name="T">矩阵元素类型。</typeparam>
    public class MatrixDiff<T> : IDiff<List<List<T>>>
    {
        private readonly IDiff<T> _elementDiff;

        /// <summary>
        /// 初始化矩阵差异比较器。
        /// </summary>
        /// <param name="elementDiff">元素比较器。</param>
        public MatrixDiff(IDiff<T> elementDiff)
        {
            _elementDiff = elementDiff;
        }

        /// <inheritdoc />
        public DiffNode Diff(List<List<T>>? oldMatrix, List<List<T>>? newMatrix)
        {
            var root = new DiffNode { Name = "Matrix" };
            oldMatrix ??= new List<List<T>>();
            newMatrix ??= new List<List<T>>();

            int maxRows = Math.Max(oldMatrix.Count, newMatrix.Count);

            for (int r = 0; r < maxRows; r++)
            {
                if (r >= oldMatrix.Count)
                {
                    root.Children.Add(new DiffNode { Name = $"Row[{r}]", Type = DiffType.Added });
                    continue;
                }
                if (r >= newMatrix.Count)
                {
                    root.Children.Add(new DiffNode { Name = $"Row[{r}]", Type = DiffType.Removed });
                    continue;
                }

                var rowNode = DiffRow(oldMatrix[r], newMatrix[r], r);
                if (rowNode.HasDifference)
                {
                    root.Children.Add(rowNode);
                }
            }
            return root;
        }

        private DiffNode DiffRow(List<T> oldRow, List<T> newRow, int rowIndex)
        {
            var rowNode = new DiffNode { Name = $"Row[{rowIndex}]" };
            int maxCols = Math.Max(oldRow.Count, newRow.Count);

            for (int c = 0; c < maxCols; c++)
            {
                if (c >= oldRow.Count)
                {
                    rowNode.Children.Add(new DiffNode { Name = $"Col[{c}]", Type = DiffType.Added, NewValue = newRow[c] });
                    continue;
                }
                if (c >= newRow.Count)
                {
                    rowNode.Children.Add(new DiffNode { Name = $"Col[{c}]", Type = DiffType.Removed, OldValue = oldRow[c] });
                    continue;
                }

                var cellNode = _elementDiff.Diff(oldRow[c], newRow[c]);
                if (cellNode.HasDifference)
                {
                    cellNode.Name = $"Col[{c}]";
                    rowNode.Children.Add(cellNode);
                }
            }
            return rowNode;
        }
    }


}
