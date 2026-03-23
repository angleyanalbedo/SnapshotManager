using System;
using System.Collections.Generic;
using System.Linq;
using SnapshotManager.Abstruactions;

namespace SnapshotManager.Models
{
    /// <summary>
    /// 快照基类。
    /// 封装了特定时间点的数据状态，并包含元数据（名称、描述、时间戳）。
    /// </summary>
    /// <typeparam name="T">快照存储的数据类型。</typeparam>
    public class Snapshot<T>
    {
        /// <summary>
        /// 快照创建的时间戳。
        /// </summary>
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        /// <summary>
        /// 快照的唯一名称或键。
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 快照的描述信息。
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 内部存储的数据副本。
        /// 受保护成员，子类可直接访问，但外部无法修改。
        /// </summary>
        protected T _snap;

        /// <summary>
        /// 初始化快照基类。
        /// </summary>
        /// <param name="name">快照名称。</param>
        /// <param name="description">快照描述。</param>
        /// <param name="data">要存储的数据（注意：派生类应负责深拷贝）。</param>
        public Snapshot(string name, string description, T data)
        {
            Name = name;
            Description = description;
            _snap = data;
        }

        /// <summary>
        /// 获取快照中存储的数据。
        /// </summary>
        /// <returns>数据的副本或引用（取决于具体实现）。</returns>
        public T GetData() => _snap;

        /// <summary>
        /// 获取快照数据的属性访问器。
        /// </summary>
        public T Data { get => _snap; }
    }

    /// <summary>
    /// 针对列表类型的快照实现。
    /// 在创建时会自动对列表中的元素进行深拷贝。
    /// </summary>
    /// <typeparam name="T">列表元素类型，必须实现 IDeepCloneable。</typeparam>
    public class ListSnapshot<T> : Snapshot<List<T>>
    where T : IDeepCloneable<T>
    {
        /// <summary>
        /// 创建列表快照。
        /// </summary>
        /// <param name="name">快照名称。</param>
        /// <param name="description">快照描述。</param>
        /// <param name="source">源列表数据。</param>
        public ListSnapshot(string name, string description, List<T> source)
            : base(name, description, Clone(source))
        {
        }

        private static List<T> Clone(List<T> src)
        {
            if (src == null) return null;

            // 优化：预分配 Capacity
            var result = new List<T>(src.Count);
            foreach (var item in src)
                result.Add(item.DeepClone());
            return result;
        }
    }

    /// <summary>
    /// 针对二维矩阵（列表的列表）类型的快照实现。
    /// 在创建时会自动对矩阵中的元素进行深拷贝。
    /// </summary>
    /// <typeparam name="T">矩阵元素类型，必须实现 IDeepCloneable。</typeparam>
    public class MatrixSnapshot<T> : Snapshot<List<List<T>>>
    where T : IDeepCloneable<T>
    {
        /// <summary>
        /// 创建矩阵快照。
        /// </summary>
        /// <param name="name">快照名称。</param>
        /// <param name="description">快照描述。</param>
        /// <param name="src">源矩阵数据。</param>
        public MatrixSnapshot(string name, string description, List<List<T>> src)
            : base(name, description, Clone(src))
        {
        }

        private static List<List<T>> Clone(List<List<T>> src)
        {
            if (src == null) return null;

            // 优化：预分配外层 List Capacity
            var result = new List<List<T>>(src.Count);

            foreach (var row in src)
            {
                // 优化：预分配内层 List Capacity
                var newRow = new List<T>(row.Count);
                foreach (var cell in row)
                    newRow.Add(cell.DeepClone());
                result.Add(newRow);
            }

            return result;
        }
    }

    /// <summary>
    /// 专门针对 ElementBase 二维矩阵的快照实现。
    /// </summary>
    public class ElementArraySnapshot : Snapshot<List<List<ElementBase>>>
    {
        /// <summary>
        /// 创建 ElementBase 矩阵快照。
        /// </summary>
        /// <param name="name">快照名称。</param>
        /// <param name="description">快照描述。</param>
        /// <param name="src">源矩阵数据。</param>
        public ElementArraySnapshot(
            string name,
            string description,
            List<List<ElementBase>> src)
            : base(name, description, DeepClone2D(src))
        { }

        private static List<List<ElementBase>> DeepClone2D(List<List<ElementBase>> src)
        {
            if (src == null) return null;

            // 优化：移除 LINQ，使用预分配容量的循环
            var result = new List<List<ElementBase>>(src.Count);
            foreach (var row in src)
            {
                var newRow = new List<ElementBase>(row.Count);
                foreach (var item in row)
                {
                    newRow.Add(item.DeepClone());
                }
                result.Add(newRow);
            }
            return result;
        }
    }
}
