using SnapshotManager.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.Abstruactions
{
    /// <summary>
    /// 统一的比较接口，返回树状 DiffNode
    /// </summary>
    public interface IDiff<T>
    {
        /// <summary>
        /// 比较两个对象并生成差异树。
        /// </summary>
        /// <param name="oldValue">旧值。</param>
        /// <param name="newValue">新值。</param>
        /// <returns>差异节点。</returns>
        DiffNode Diff(T? oldValue, T? newValue);
    }
}
