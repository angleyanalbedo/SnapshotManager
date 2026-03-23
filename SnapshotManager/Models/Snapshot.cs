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
    /// <typeparam name="T">快照存储的数据类型，必须继承自 ElementBase。</typeparam>
    public class Snapshot<T> where T : ElementBase
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
        /// <param name="data">要存储的数据（会自动调用 DeepClone 进行深拷贝）。</param>
        public Snapshot(string name, string description, T data)
        {
            Name = name;
            Description = description;
            // 强制深拷贝，确保快照数据的独立性
            _snap = (T)data.DeepClone();
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
    /// 专门针对 ElementBase 二维矩阵的快照实现。
    /// </summary>
    public class ElementArraySnapshot : Snapshot<MatrixElement>
    {
        /// <summary>
        /// 创建 ElementBase 矩阵快照。
        /// </summary>
        public ElementArraySnapshot(string name, string description, MatrixElement src)
            : base(name, description, src)
        { }
        
        /// <summary>
        /// 辅助构造函数：从原始 List 创建。
        /// </summary>
        public ElementArraySnapshot(string name, string description, List<List<ElementBase>> src)
            : base(name, description, new MatrixElement(src))
        { }
    }

    /// <summary>
    /// 针对基础类型（int, string, bool 等）列表的快照实现。
    /// </summary>
    /// <typeparam name="T">基础数据类型。</typeparam>
    public class PrimitiveListSnapshot<T> : Snapshot<PrimitiveListElement<T>>
    {
        /// <summary>
        /// 创建基础类型列表快照。
        /// </summary>
        public PrimitiveListSnapshot(string name, string description, PrimitiveListElement<T> source)
            : base(name, description, source)
        {
        }

        /// <summary>
        /// 辅助构造函数：从原始 List 创建。
        /// </summary>
        public PrimitiveListSnapshot(string name, string description, List<T> source)
            : base(name, description, new PrimitiveListElement<T>(source))
        {
        }
    }

    /// <summary>
    /// 针对字典类型的快照实现。
    /// </summary>
    /// <typeparam name="K">键类型。</typeparam>
    /// <typeparam name="V">值类型。</typeparam>
    public class DictionarySnapshot<K, V> : Snapshot<DictionaryElement<K, V>>
    {
        /// <summary>
        /// 创建字典快照。
        /// </summary>
        public DictionarySnapshot(string name, string description, DictionaryElement<K, V> source)
            : base(name, description, source)
        {
        }

        /// <summary>
        /// 辅助构造函数：从原始 Dictionary 创建。
        /// </summary>
        public DictionarySnapshot(string name, string description, Dictionary<K, V> source)
            : base(name, description, new DictionaryElement<K, V>(source))
        {
        }
    }
}
