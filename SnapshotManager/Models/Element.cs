using SnapshotManager.Abstruactions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
#if NET45
using Newtonsoft.Json;
#else
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

        /// <summary>
        /// 隐式转换为内部值类型。
        /// </summary>
        /// <param name="element">包装元素。</param>
        public static implicit operator T(ValueElement<T> element) => element.Value;

        /// <summary>
        /// 从值类型隐式转换为包装元素。
        /// </summary>
        /// <param name="value">原始值。</param>
        public static implicit operator ValueElement<T>(T value) => new ValueElement<T>(value);
    }

    /// <summary>
    /// 基于 JSON 序列化的通用对象包装器。
    /// 适用于不想手动实现 DeepClone 的复杂对象（POCO）。
    /// 注意：性能低于手写 DeepClone，且依赖 System.Text.Json (或 Newtonsoft.Json)。
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
            if (Data == null) return new JsonElement<T>(default!);

            // 偷懒的深拷贝：序列化再反序列化
#if NET45
            var json = JsonConvert.SerializeObject(Data);
            var clone = JsonConvert.DeserializeObject<T>(json);
#else
            var json = JsonSerializer.Serialize(Data);
            var clone = JsonSerializer.Deserialize<T>(json);
#endif
            return new JsonElement<T>(clone!);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is JsonElement<T> other)
            {
                // 简单的比较逻辑：比较序列化后的 JSON 字符串
                // (这能解决大部分 POCO 的比较问题，但效率一般)
#if NET45
                var jsonA = JsonConvert.SerializeObject(Data);
                var jsonB = JsonConvert.SerializeObject(other.Data);
#else
                var jsonA = JsonSerializer.Serialize(Data);
                var jsonB = JsonSerializer.Serialize(other.Data);
#endif
                return jsonA == jsonB;
            }
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode() => Data?.GetHashCode() ?? 0;

        /// <inheritdoc />
        public override string ToString() => Data?.ToString() ?? "null";
    }

    /// <summary>
    /// 基础类型列表的 Element 包装器。
    /// </summary>
    /// <typeparam name="T">基础类型。</typeparam>
    public class PrimitiveListElement<T> : ElementBase
    {
        /// <summary>
        /// 内部存储的列表。
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// 初始化基础类型列表包装器。
        /// </summary>
        /// <param name="items">原始列表。</param>
        public PrimitiveListElement(List<T> items)
        {
            Items = items ?? new List<T>();
        }

        /// <inheritdoc />
        public override ElementBase DeepClone()
        {
            return new PrimitiveListElement<T>(new List<T>(Items));
        }
    }

    /// <summary>
    /// 字典类型的 Element 包装器。
    /// </summary>
    public class DictionaryElement<K, V> : ElementBase
        where K : notnull
    {
        /// <summary>
        /// 内部存储的字典。
        /// </summary>
        public Dictionary<K, V> Map { get; set; }

        /// <summary>
        /// 初始化字典包装器。
        /// </summary>
        /// <param name="map">原始字典。</param>
        public DictionaryElement(Dictionary<K, V> map)
        {
            Map = map ?? new Dictionary<K, V>();
        }

        /// <inheritdoc />
        public override ElementBase DeepClone()
        {
            return new DictionaryElement<K, V>(new Dictionary<K, V>(Map));
        }
    }

    /// <summary>
    /// HashSet 类型的 Element 包装器。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    public class HashSetElement<T> : ElementBase
    {
        /// <summary>
        /// 内部存储的 HashSet。
        /// </summary>
        public HashSet<T> Set { get; set; }

        /// <summary>
        /// 初始化 HashSet 包装器。
        /// </summary>
        /// <param name="set">原始 HashSet。</param>
        public HashSetElement(HashSet<T> set)
        {
            Set = set ?? new HashSet<T>();
        }

        /// <inheritdoc />
        public override ElementBase DeepClone()
        {
            return new HashSetElement<T>(new HashSet<T>(Set));
        }
    }

    /// <summary>
    /// ElementBase 二维矩阵的包装器。
    /// </summary>
    public class MatrixElement : ElementBase
    {
        /// <summary>
        /// 内部存储的二维列表（行）。
        /// </summary>
        public List<List<ElementBase>> Rows { get; set; }

        /// <summary>
        /// 初始化矩阵包装器。
        /// </summary>
        /// <param name="rows">原始二维列表。</param>
        public MatrixElement(List<List<ElementBase>> rows)
        {
            Rows = rows ?? new List<List<ElementBase>>();
        }

        /// <inheritdoc />
        public override ElementBase DeepClone()
        {
            var newRows = new List<List<ElementBase>>(Rows.Count);
            foreach (var row in Rows)
            {
                var newRow = new List<ElementBase>(row.Count);
                foreach (var item in row)
                {
                    newRow.Add(item.DeepClone());
                }
                newRows.Add(newRow);
            }
            return new MatrixElement(newRows);
        }
    }
}
