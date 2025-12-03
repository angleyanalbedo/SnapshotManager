using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core.@interface
{
    public interface ICompare<T>
    {
        /// <summary>
        /// 对比两个对象，返回差异结果
        /// </summary>
        DiffResult Compare(T oldValue, T newValue);

    }
    /// <summary>
    /// 任何对象都可以自己实现 Diff
    /// </summary>
    public interface IDiff<T>
    {
        DiffNode Diff(T oldValue, T newValue);
    }

}
