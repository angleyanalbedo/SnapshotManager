using SnapshotManager.Abstruactions;
using SnapshotManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnapshotManager.Core
{
    /// <summary>
    /// 通用的快照管理器实现。
    /// </summary>
    /// <typeparam name="TSnapshot">具体的快照类型。</typeparam>
    /// <typeparam name="TModel">被管理的数据模型类型。</typeparam>
    public class SnapshotManager<TSnapshot, TModel> : ISnapshotManager<TSnapshot, TModel>
        where TSnapshot : Snapshot<TModel>
        where TModel : ElementBase
    {
        private readonly Dictionary<string, TSnapshot> _history =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Func<TModel?, TModel?, DiffNode>? _diffFunc;
        
        // [修改] 工厂现在接收 (key, data) 两个参数
        private readonly Func<string, TModel, TSnapshot>? _snapshotFactory;

        /// <summary>
        /// 初始化管理器。尝试从类型 TModel 自身获取 IDiff 实现。
        /// </summary>
        /// <param name="snapshotFactory">可选。用于将数据包装为 Snapshot 的工厂委托。</param>
        public SnapshotManager(Func<string, TModel, TSnapshot>? snapshotFactory = null)
        {
            _snapshotFactory = snapshotFactory;

            if (typeof(IDiff<TModel>).IsAssignableFrom(typeof(TModel)))
            {
                _diffFunc = (oldVal, newVal) =>
                {
                    var diffObj = oldVal ?? newVal;
                    if (diffObj == null)
                        return new DiffNode { Name = typeof(TModel).Name, Type = DiffType.None };

                    return ((IDiff<TModel>)diffObj).Diff(oldVal, newVal);
                };
            }
        }

        /// <summary>
        /// 初始化管理器，注入自定义的比较委托。
        /// </summary>
        /// <param name="diffFunc">用于对比两个 TModel 对象的委托。</param>
        /// <param name="snapshotFactory">可选。用于将数据包装为 Snapshot 的工厂委托。</param>
        public SnapshotManager(Func<TModel?, TModel?, DiffNode> diffFunc, Func<string, TModel, TSnapshot>? snapshotFactory = null)
        {
            _diffFunc = diffFunc;
            _snapshotFactory = snapshotFactory;
        }

        /// <summary>
        /// 初始化管理器，注入自定义的比较器对象。
        /// </summary>
        /// <param name="diffObj">实现了 IDiff 接口的比较器。</param>
        /// <param name="snapshotFactory">可选。用于将数据包装为 Snapshot 的工厂委托。</param>
        public SnapshotManager(IDiff<TModel> diffObj, Func<string, TModel, TSnapshot>? snapshotFactory = null)
        {
            _diffFunc = diffObj.Diff;
            _snapshotFactory = snapshotFactory;
        }

        /// <inheritdoc />
        public void AddSnapshot(TSnapshot snapshot)
        {
            // 如果 snapshot 没有名字，自动生成一个
            var key = string.IsNullOrEmpty(snapshot.Name) 
                ? DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") 
                : snapshot.Name;
            
            _history[key] = snapshot;
        }

        /// <inheritdoc />
        public TSnapshot GetSnapshot(string name)
        {
            if (!_history.TryGetValue(name, out var snap))
                throw new KeyNotFoundException($"Snapshot '{name}' not found.");
            return snap;
        }

        /// <inheritdoc />
        public void AddSnapshot(string key, TSnapshot snapshot)
        {
            _history[key] = snapshot;
        }

        /// <inheritdoc />
        public string TakeSnapshot(TModel data)
        {
            var key = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            TakeSnapshot(key, data);
            return key;
        }

        /// <inheritdoc />
        public void TakeSnapshot(string key, TModel data)
        {
            if (_snapshotFactory == null)
                throw new InvalidOperationException("Snapshot factory is not configured. Cannot create snapshot from data directly.");

            // [修改] 将 key 传递给工厂，以便 Snapshot 对象拥有正确的名称
            var snapshot = _snapshotFactory(key, data);
            
            _history[key] = snapshot;
        }

        /// <inheritdoc />
        public IEnumerable<TSnapshot> ListSnapshots()
        {
            return _history.Values.OrderBy(s => s.TimeStamp);
        }

        /// <inheritdoc />
        public DiffNode Diff(string snapA, string snapB)
        {
            if (_diffFunc is null)
                throw new InvalidOperationException("Diff function is not configured for this manager.");

            var a = GetSnapshot(snapA).Data;
            var b = GetSnapshot(snapB).Data;

            return _diffFunc(a, b);
        }

        /// <inheritdoc />
        public DiffNode DiffWith(string baseSnapKey, TModel currentData)
        {
            if (_diffFunc is null)
                throw new InvalidOperationException("Diff function is not configured for this manager.");

            var baseData = GetSnapshot(baseSnapKey).Data;
            return _diffFunc(baseData, currentData);
        }
    }

    /// <summary>
    /// 针对 ElementBase 二维列表的管理器工厂。
    /// </summary>
    public static class ElementSnapshotManagerFactory
    {
        /// <summary>
        /// 创建一个预配置的管理器，用于处理 MatrixElement 类型的数据。
        /// </summary>
        /// <returns>配置好的 SnapshotManager 实例。</returns>
        public static SnapshotManager<ElementArraySnapshot, MatrixElement> Create()
        {
            return new SnapshotManager<ElementArraySnapshot, MatrixElement>(
                new MatrixElementDiff(), 
                (key, data) => new ElementArraySnapshot(key, "Auto Generated Snapshot", data)
            );
        }
    }

    /// <summary>
    /// 针对基础类型列表（如 List&lt;int&gt;）的专用管理器。
    /// </summary>
    /// <typeparam name="T">基础数据类型。</typeparam>
    public class PrimitiveListSnapshotManager<T> : SnapshotManager<PrimitiveListSnapshot<T>, PrimitiveListElement<T>>
    {
        /// <summary>
        /// 初始化基础类型列表快照管理器。
        /// </summary>
        public PrimitiveListSnapshotManager()
            : base(
                  new PrimitiveListElementDiff<T>(),
                  (key, data) => new PrimitiveListSnapshot<T>(key, "Auto Generated", data))
        {
        }
    }

    /// <summary>
    /// 针对字典（如 Dictionary&lt;string, int&gt;）的专用管理器。
    /// </summary>
    /// <typeparam name="K">键类型。</typeparam>
    /// <typeparam name="V">值类型（假定为基础类型）。</typeparam>
    public class DictionarySnapshotManager<K, V> : SnapshotManager<DictionarySnapshot<K, V>, DictionaryElement<K, V>>
        where K : notnull
    {
        /// <summary>
        /// 初始化字典快照管理器。
        /// </summary>
        public DictionarySnapshotManager()
            : base(
                  new DictionaryElementDiff<K, V>(),
                  (key, data) => new DictionarySnapshot<K, V>(key, "Auto Generated", data))
        {
        }
    }

    /// <summary>
    /// 针对 HashSet（如 HashSet&lt;int&gt;）的专用管理器。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    public class HashSetSnapshotManager<T> : SnapshotManager<HashSetSnapshot<T>, HashSetElement<T>>
    {
        /// <summary>
        /// 初始化 HashSet 快照管理器。
        /// </summary>
        public HashSetSnapshotManager()
            : base(
                  new HashSetElementDiff<T>(),
                  (key, data) => new HashSetSnapshot<T>(key, "Auto Generated", data))
        {
        }
    }

    /// <summary>
    /// 容器快照管理器工厂。
    /// </summary>
    public static class ContainerSnapshotManagerFactory
    {
        /// <summary>
        /// 创建基础类型列表管理器。
        /// </summary>
        public static PrimitiveListSnapshotManager<T> CreateListManager<T>()
        {
            return new PrimitiveListSnapshotManager<T>();
        }

        /// <summary>
        /// 创建字典管理器。
        /// </summary>
        public static DictionarySnapshotManager<K, V> CreateDictionaryManager<K, V>()
            where K : notnull
        {
            return new DictionarySnapshotManager<K, V>();
        }

        /// <summary>
        /// 创建 HashSet 管理器。
        /// </summary>
        public static HashSetSnapshotManager<T> CreateHashSetManager<T>()
        {
            return new HashSetSnapshotManager<T>();
        }
    }

}
