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

        // 构造函数 1: 尝试从 T 自身获取 IDiff 实现
        public SnapshotManager()
        {
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
        public SnapshotManager(Func<T?, T?, DiffNode> diffFunc)
        {
            _diffFunc = diffFunc;
        }

        // 构造函数 3: 注入比较器对象
        public SnapshotManager(IDiff<T> diffObj)
        {
            _diffFunc = diffObj.Diff;
        }



        public void AddSnapshot(Snapshot<T> snapshot)
        {
            _history[snapshot.Name] = snapshot;
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
    }

    // 工厂类更新：使用组合模式构建 Diff 逻辑
    public static class ElementSnapshotManagerFactory
    {
        public static SnapshotManager<List<List<ElementBase>>> Create()
        {
            // 组装：MatrixDiff -> ElementDiff
            var elementDiff = new ElementDiff();
            var matrixDiff = new MatrixDiff<ElementBase>(elementDiff);

            return new SnapshotManager<List<List<ElementBase>>>(matrixDiff);
        }
    }

}
