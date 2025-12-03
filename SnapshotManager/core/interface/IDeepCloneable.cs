using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core.@interface
{
    public interface IDeepCloneable<T>
    {
        T DeepClone();
    }
}
