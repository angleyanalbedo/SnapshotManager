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
    /// <typeparam name="T">被管理的数据类型。</typeparam>
    public class SnapshotManager<T> : ISnapshotManager<T>
    {
        private readonly Dictionary<string, Snapshot<T>> _history =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Func<T?, T?, DiffNode>? _diffFunc;
        
        // [修改] 工厂现在接收 (key, data) 两个参数
        private readonly Func<string, T, Snapshot<T>>? _snapshotFactory;

        /// <summary>
        /// 初始化管理器。尝试从类型 T 自身获取 IDiff 实现。
        /// </summary>
        /// <param name="snapshotFactory">可选。用于将数据包装为 Snapshot 的工厂委托。</param>
        public SnapshotManager(Func<string, T, Snapshot<T>>? snapshotFactory = null)
        {
            _snapshotFactory = snapshotFactory;

            if (typeof(IDiff<T>).IsAssignableFrom(typeof(T)))
            {
                _diffFunc = (oldVal, newVal) =>
                {
                    var diffObj = oldVal ?? newVal;
                    if (diffObj == null)
                        return new DiffNode { Name = typeof(T).Name, Type = DiffType.None };

                    return ((IDiff<T>)diffObj).Diff(oldVal, newVal);
                };
            }
        }

        /// <summary>
        /// 初始化管理器，注入自定义的比较委托。
        /// </summary>
        /// <param name="diffFunc">用于对比两个 T 对象的委托。</param>
        /// <param name="snapshotFactory">可选。用于将数据包装为 Snapshot 的工厂委托。</param>
        public SnapshotManager(Func<T?, T?, DiffNode> diffFunc, Func<string, T, Snapshot<T>>? snapshotFactory = null)
        {
            _diffFunc = diffFunc;
            _snapshotFactory = snapshotFactory;
        }

        /// <summary>
        /// 初始化管理器，注入自定义的比较器对象。
        /// </summary>
        /// <param name="diffObj">实现了 IDiff 接口的比较器。</param>
        /// <param name="snapshotFactory">可选。用于将数据包装为 Snapshot 的工厂委托。</param>
        public SnapshotManager(IDiff<T> diffObj, Func<string, T, Snapshot<T>>? snapshotFactory = null)
        {
            _diffFunc = diffObj.Diff;
            _snapshotFactory = snapshotFactory;
        }



        /// <inheritdoc />
        public void AddSnapshot(Snapshot<T> snapshot)
        {
            // 如果 snapshot 没有名字，自动生成一个
            var key = string.IsNullOrEmpty(snapshot.Name) 
                ? DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") 
                : snapshot.Name;
            
            _history[key] = snapshot;
        }

        /// <inheritdoc />
        public Snapshot<T> GetSnapshot(string name)
        {
            if (!_history.TryGetValue(name, out var snap))
                throw new KeyNotFoundException($"Snapshot '{name}' not found.");
            return snap;
        }

        /// <inheritdoc />
        public void AddSnapshot(string key, Snapshot<T> snapshot)
        {
            _history[key] = snapshot;
        }

        /// <inheritdoc />
        public string TakeSnapshot(T data)
        {
            var key = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            TakeSnapshot(key, data);
            return key;
        }

        /// <inheritdoc />
        public void TakeSnapshot(string key, T data)
        {
            if (_snapshotFactory == null)
                throw new InvalidOperationException("Snapshot factory is not configured. Cannot create snapshot from data directly.");

            // [修改] 将 key 传递给工厂，以便 Snapshot 对象拥有正确的名称
            var snapshot = _snapshotFactory(key, data);
            
            _history[key] = snapshot;
        }

        /// <inheritdoc />
        public IEnumerable<Snapshot<T>> ListSnapshots()
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
        public DiffNode DiffWith(string baseSnapKey, T currentData)
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
        /// 创建一个预配置的管理器，用于处理 List&lt;List&lt;ElementBase&gt;&gt; 类型的数据。
        /// <para>已内置 MatrixDiff 和 ElementDiff 算法。</para>
        /// </summary>
        /// <returns>配置好的 SnapshotManager 实例。</returns>
        public static SnapshotManager<List<List<ElementBase>>> Create()
        {
            // 组装：MatrixDiff -> ElementDiff
            var elementDiff = new ElementDiff();
            var matrixDiff = new MatrixDiff<ElementBase>(elementDiff);

            return new SnapshotManager<List<List<ElementBase>>>(
                matrixDiff, 
                (key, data) => new ElementArraySnapshot(key, "Auto Generated Snapshot", data)
            );
        }
    }

}
