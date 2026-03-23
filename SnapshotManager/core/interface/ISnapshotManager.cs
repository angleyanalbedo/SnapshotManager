using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core.@interface
{
    public interface ISnapshotManager<T>
    {
        void AddSnapshot(Snapshot<T> snapshot);
        
        // 添加缺失的重载
        void AddSnapshot(string key, Snapshot<T> snapshot);

        // [新增] 直接传入数据，自动生成 Key
        string TakeSnapshot(T data);

        // [新增] 直接传入数据，指定 Key
        void TakeSnapshot(string key, T data);

        Snapshot<T> GetSnapshot(string name);

        IEnumerable<Snapshot<T>> ListSnapshots();

        DiffNode Diff(string snapA, string snapB);

        // [新增] 对比：历史快照 vs 当前数据
        DiffNode DiffWith(string baseSnapKey, T currentData);
    }

}
