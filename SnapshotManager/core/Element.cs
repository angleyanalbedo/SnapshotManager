using SnapshotManager.core.@interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SnapshotManager.core
{
    public abstract class ElementBase : IDeepCloneable<ElementBase>
    {
        public abstract ElementBase DeepClone();
    }
}
