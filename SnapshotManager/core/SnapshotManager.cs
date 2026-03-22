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

        private readonly Func<T, T, DiffNode> _diffFunc;
        private readonly Func<T, T, DiffResult> _compareFunc;

        // 构造函数，自动适配 IDiff<T>
        public SnapshotManager()
        {
            if (typeof(IDiff<T>).IsAssignableFrom(typeof(T)))
            {
                _diffFunc = (oldVal, newVal) =>
                {
                    var diffObj = oldVal ?? newVal; // 任一非 null 实例用于调用 Diff
                    if (diffObj == null)
                        return new DiffNode { Name = typeof(T).Name, Type = DiffType.None };

                    return ((IDiff<T>)diffObj).Diff(oldVal, newVal);
                };
            }
            else
            {
                throw new InvalidOperationException("T 必须实现 IDiff<T>");
            }
        }
        public SnapshotManager(Func<T, T, DiffNode> diffFunc)
        {
            _diffFunc = diffFunc;
        }

        // 接受 IDiff<T>
        public SnapshotManager(IDiff<T> diffObj)
        {
            _diffFunc = diffObj.Diff;
        }

        // 接受 ICompare<T>
        public SnapshotManager(ICompare<T> compareObj)
        {
           _compareFunc = compareObj.Compare;
        }



        public void AddSnapshot(Snapshot<T> snapshot)
        {
            _history[snapshot.Name] = snapshot;
        }

        public Snapshot<T> GetSnapshot(string name)
        {
            return _history[name];
        }

        public void AddSnapshot(string key, Snapshot<T> snapshot)
        {
            _history[key] = snapshot;
        }

        public IEnumerable<Snapshot<T>> ListSnapshots()
        {
            return _history.Values.OrderBy(s => s.TimeStamp);
        }

        public DiffNode Diff(string snapA, string snapB)
        {
            var a = GetSnapshot(snapA).Data;
            var b = GetSnapshot(snapB).Data;

            return _diffFunc(a, b);
        }
        public DiffNode CompareSnapshots(string oldKey, string newKey)
        {
            if (!_history.TryGetValue(oldKey, out var oldSnap) ||
                !_history.TryGetValue(newKey, out var newSnap))
            {
                throw new ArgumentException("指定的快照不存在");
            }

            return _diffFunc(oldSnap.Data, newSnap.Data);
        }
    }

    public static class ElementSnapshotManagerFactory
    {
        public static SnapshotManager<List<List<ElementBase>>> Create()
        {
            return new SnapshotManager<List<List<ElementBase>>>(
                (oldArr, newArr) => new ElementArrayDiff().Diff(oldArr, newArr)
            );
        }
    }

}
