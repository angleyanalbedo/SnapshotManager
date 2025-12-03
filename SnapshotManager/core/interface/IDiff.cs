using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core.@interface
{
    public interface IDiff<T>
    {
        /// <summary>
        /// 对比两个对象，返回差异结果
        /// </summary>
        DiffResult Compare(T oldValue, T newValue);
    }

}
