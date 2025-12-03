using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core.@interface
{
    public interface ISnapshotManager<T>
    {
        void AddSnapshot(Snapshot<T> snapshot);

        Snapshot<T> GetSnapshot(string name);

        IEnumerable<Snapshot<T>> ListSnapshots();

        DiffNode Diff(string snapA, string snapB);
    }

}
