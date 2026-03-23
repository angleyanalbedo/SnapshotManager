using SnapshotManager.Abstruactions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SnapshotManager.Models
{
    /// <summary>
    /// 数据元素基类。
    /// 所有受 SnapshotManager 管理的数据对象建议继承此类。
    /// </summary>
    public abstract class ElementBase : IDeepCloneable<ElementBase>
    {
        /// <inheritdoc />
        public abstract ElementBase DeepClone();
    }
}
