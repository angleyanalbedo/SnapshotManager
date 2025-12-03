using SnapshotManager.core.@interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core
{
    public class SnapshotManager<T> : ISnapshotManager<T>
    {
        private readonly Dictionary<string, Snapshot<T>> _history =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Func<T, T, DiffNode> _diffFunc;

        public SnapshotManager(Func<T, T, DiffNode> diffFunc)
        {
            _diffFunc = diffFunc;
        }

        public void AddSnapshot(Snapshot<T> snapshot)
        {
            _history[snapshot.Name] = snapshot;
        }

        public Snapshot<T> GetSnapshot(string name)
        {
            return _history[name];
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
    }

    public static class ElementSnapshotManagerFactory
    {
        public static SnapshotManager<List<List<ElementBase>>> Create()
        {
            return new SnapshotManager<List<List<ElementBase>>>(
                (oldArr, newArr) => ElementArrayDiff.Diff(oldArr, newArr)
            );
        }
    }

}
