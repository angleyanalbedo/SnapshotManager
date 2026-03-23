using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.Abstruactions
{
    /// <summary>
    /// 深拷贝接口。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    public interface IDeepCloneable<T>
    {
        /// <summary>
        /// 创建当前对象的深拷贝副本。
        /// </summary>
        /// <returns>对象的深拷贝。</returns>
        T DeepClone();
    }
}
