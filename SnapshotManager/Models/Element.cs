using SnapshotManager.Abstruactions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
#if !NET45
using System.Text.Json;
#endif

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

    /// <summary>
    /// 通用值类型元素包装器。
    /// 适用于 int, double, string, bool 等不可变或值类型。
    /// </summary>
    /// <typeparam name="T">值类型。</typeparam>
    public class ValueElement<T> : ElementBase
    {
        /// <summary>
        /// 存储的值。
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 初始化值类型元素。
        /// </summary>
        /// <param name="value">初始值。</param>
        public ValueElement(T value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override ElementBase DeepClone()
        {
            // 对于值类型和 string，直接返回新对象即可（string 是不可变的）
            return new ValueElement<T>(Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is ValueElement<T> other)
            {
                return EqualityComparer<T>.Default.Equals(Value, other.Value);
            }
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        /// <inheritdoc />
        public override string ToString() => Value?.ToString() ?? "null";

        // 隐式转换，让使用更方便
        public static implicit operator T(ValueElement<T> element) => element.Value;
        public static implicit operator ValueElement<T>(T value) => new ValueElement<T>(value);
    }

#if !NET45
    /// <summary>
    /// 基于 JSON 序列化的通用对象包装器。
    /// 适用于不想手动实现 DeepClone 的复杂对象（POCO）。
    /// 注意：性能低于手写 DeepClone，且依赖 System.Text.Json。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    public class JsonElement<T> : ElementBase
    {
        /// <summary>
        /// 存储的数据对象。
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 初始化 JSON 元素包装器。
        /// </summary>
        /// <param name="data">初始数据。</param>
        public JsonElement(T data)
        {
            Data = data;
        }

        /// <inheritdoc />
        public override ElementBase DeepClone()
        {
            if (Data == null) return new JsonElement<T>(default);

            // 偷懒的深拷贝：序列化再反序列化
            var json = JsonSerializer.Serialize(Data);
            var clone = JsonSerializer.Deserialize<T>(json);
            return new JsonElement<T>(clone!);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is JsonElement<T> other)
            {
                // 简单的比较逻辑：比较序列化后的 JSON 字符串
                // (这能解决大部分 POCO 的比较问题，但效率一般)
                var jsonA = JsonSerializer.Serialize(Data);
                var jsonB = JsonSerializer.Serialize(other.Data);
                return jsonA == jsonB;
            }
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode() => Data?.GetHashCode() ?? 0;

        /// <inheritdoc />
        public override string ToString() => Data?.ToString() ?? "null";
    }
#endif
}
