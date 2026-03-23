using SnapshotManager.core.@interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnapshotManager.core
{
    public class SnapshotManager<T> : ISnapshotManager<T>
    {
        private readonly Dictionary<string, Snapshot<T>> _history =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Func<T?, T?, DiffNode>? _diffFunc;
        
        // [新增] 用于将数据包装为 Snapshot 的工厂委托
        private readonly Func<T, Snapshot<T>>? _snapshotFactory;

        // 构造函数 1: 尝试从 T 自身获取 IDiff 实现
        // [修改] 增加可选的 snapshotFactory 参数
        public SnapshotManager(Func<T, Snapshot<T>>? snapshotFactory = null)
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

        // 构造函数 2: 注入比较委托
        // [修改] 增加可选的 snapshotFactory 参数
        public SnapshotManager(Func<T?, T?, DiffNode> diffFunc, Func<T, Snapshot<T>>? snapshotFactory = null)
        {
            _diffFunc = diffFunc;
            _snapshotFactory = snapshotFactory;
        }

        // 构造函数 3: 注入比较器对象
        // [修改] 增加可选的 snapshotFactory 参数
        public SnapshotManager(IDiff<T> diffObj, Func<T, Snapshot<T>>? snapshotFactory = null)
        {
            _diffFunc = diffObj.Diff;
            _snapshotFactory = snapshotFactory;
        }



        public void AddSnapshot(Snapshot<T> snapshot)
        {
            // 如果 snapshot 没有名字，自动生成一个
            var key = string.IsNullOrEmpty(snapshot.Name) 
                ? DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") 
                : snapshot.Name;
            
            _history[key] = snapshot;
        }

        public Snapshot<T> GetSnapshot(string name)
        {
            if (!_history.TryGetValue(name, out var snap))
                throw new KeyNotFoundException($"Snapshot '{name}' not found.");
            return snap;
        }

        public void AddSnapshot(string key, Snapshot<T> snapshot)
        {
            _history[key] = snapshot;
        }

        // [新增] 实现 TakeSnapshot (自动 Key)
        public string TakeSnapshot(T data)
        {
            var key = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            TakeSnapshot(key, data);
            return key;
        }

        // [新增] 实现 TakeSnapshot (指定 Key)
        public void TakeSnapshot(string key, T data)
        {
            if (_snapshotFactory == null)
                throw new InvalidOperationException("Snapshot factory is not configured. Cannot create snapshot from data directly.");

            var snapshot = _snapshotFactory(data);
            // 确保 snapshot 内部也持有这个 key (如果 Snapshot 类有 Name 属性的话)
            // 这里我们主要依赖 _history 的 key
            _history[key] = snapshot;
        }

        public IEnumerable<Snapshot<T>> ListSnapshots()
        {
            return _history.Values.OrderBy(s => s.TimeStamp);
        }

        // 统一后的 Diff 方法
        public DiffNode Diff(string snapA, string snapB)
        {
            if (_diffFunc is null)
                throw new InvalidOperationException("Diff function is not configured for this manager.");

            var a = GetSnapshot(snapA).Data;
            var b = GetSnapshot(snapB).Data;

            return _diffFunc(a, b);
        }

        // [新增] 实现 DiffWith (快照 vs 数据)
        public DiffNode DiffWith(string baseSnapKey, T currentData)
        {
            if (_diffFunc is null)
                throw new InvalidOperationException("Diff function is not configured for this manager.");

            var baseData = GetSnapshot(baseSnapKey).Data;
            return _diffFunc(baseData, currentData);
        }
    }

    // 工厂类更新：使用组合模式构建 Diff 逻辑
    public static class ElementSnapshotManagerFactory
    {
        public static SnapshotManager<List<List<ElementBase>>> Create()
        {
            // 组装：MatrixDiff -> ElementDiff
            var elementDiff = new ElementDiff();
            var matrixDiff = new MatrixDiff<ElementBase>(elementDiff);

            // [修改] 注入 Snapshot 的创建逻辑 (ElementArraySnapshot)
            return new SnapshotManager<List<List<ElementBase>>>(
                matrixDiff, 
                data => new ElementArraySnapshot(data)
            );
        }
    }

}
