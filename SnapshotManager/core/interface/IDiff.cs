using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core.@interface
{
    /// <summary>
    /// 统一的比较接口，返回树状 DiffNode
    /// </summary>
    public interface IDiff<T>
    {
        DiffNode Diff(T? oldValue, T? newValue);
    }
}
