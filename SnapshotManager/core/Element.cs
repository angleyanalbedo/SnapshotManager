using SnapshotManager.core.@interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SnapshotManager.core
{
    public abstract class ElementBase : IDeepCloneable<ElementBase>, IDiff<ElementBase>
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        public abstract ElementBase DeepClone();

        /// <summary>
        /// 自动反射比较（通用的字段级 Diff）
        /// </summary>
        public virtual DiffNode Diff(ElementBase? oldValue, ElementBase? newValue)
        {
            var node = new DiffNode
            {
                Name = oldValue?.GetType().Name ?? newValue?.GetType().Name ?? "Element"
            };

            if (oldValue == null && newValue != null)
            {
                node.Type = DiffType.Added;
                return node;
            }
            if (oldValue != null && newValue == null)
            {
                node.Type = DiffType.Removed;
                return node;
            }
            if (oldValue == null && newValue == null)
            {
                return node;
            }

            var properties = _propertyCache.GetOrAdd(
                oldValue!.GetType(),
                t => t.GetProperties().Where(p => p.CanRead).ToArray());

            foreach (var prop in properties)
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
