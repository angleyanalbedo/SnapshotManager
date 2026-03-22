using SnapshotManager.core.@interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SnapshotManager.core
{
    public abstract class ElementBase : IDeepCloneable<ElementBase>, IDiff<ElementBase>
    {

        public abstract ElementBase DeepClone();

        /// <summary>
        /// 自动反射比较（通用的字段级 Diff）
        /// </summary>
        public virtual DiffNode Diff(ElementBase oldValue, ElementBase newValue)
        {
            var node = new DiffNode
            {
                Name = oldValue?.GetType().Name ?? "Element"
            };

            if (oldValue == null || newValue == null)
            {
                node.Type = (oldValue == null && newValue != null) ? DiffType.Added :
                            (oldValue != null && newValue == null) ? DiffType.Removed :
                            DiffType.None;
                return node;
            }

            foreach (var prop in oldValue.GetType().GetProperties()
                                         .Where(p => p.CanRead))
            {
                var oldVal = prop.GetValue(oldValue);
                var newVal = prop.GetValue(newValue);

                if (!Equals(oldVal, newVal))
                {
                    node.Children.Add(new DiffNode
                    {
                        Name = prop.Name,
                        Type = DiffType.Modified,
                        OldValue = oldVal,
                        NewValue = newVal
                    });
                }
            }

            return node;
        }
    }
}
