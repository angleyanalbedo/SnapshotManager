using SnapshotManager.Models;
using System;
using System.Collections;
using System.Text;

namespace SnapshotManager.Output
{
    /// <summary>
    /// 快照打印接口。
    /// </summary>
    public interface ISnapshotPrinter
    {
        /// <summary>
        /// 打印 ElementBase 数据。
        /// </summary>
        void Print(ElementBase element);

        /// <summary>
        /// 打印快照。
        /// </summary>
        void Print<T>(Snapshot<T> snapshot) where T : ElementBase;
    }

    /// <summary>
    /// 控制台快照打印器。
    /// </summary>
    public class ConsoleSnapshotPrinter : ISnapshotPrinter
    {
        public void Print(ElementBase element)
        {
            Console.WriteLine(SnapshotFormatter.Format(element));
        }

        public void Print<T>(Snapshot<T> snapshot) where T : ElementBase
        {
            Console.WriteLine(SnapshotFormatter.Format(snapshot));
        }
    }

    /// <summary>
    /// 字符串快照打印器（导出器）。
    /// 将输出累积到内部 StringBuilder 中。
    /// </summary>
    public class StringSnapshotPrinter : ISnapshotPrinter
    {
        private readonly StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// 获取当前的打印结果。
        /// </summary>
        public string Result => _sb.ToString();

        /// <summary>
        /// 清空缓冲区。
        /// </summary>
        public void Clear() => _sb.Clear();

        public void Print(ElementBase element)
        {
            _sb.AppendLine(SnapshotFormatter.Format(element));
        }

        public void Print<T>(Snapshot<T> snapshot) where T : ElementBase
        {
            _sb.AppendLine(SnapshotFormatter.Format(snapshot));
        }
    }

    /// <summary>
    /// 快照格式化工具类。
    /// </summary>
    public static class SnapshotFormatter
    {
        /// <summary>
        /// 格式化快照对象。
        /// </summary>
        public static string Format<T>(Snapshot<T> snapshot) where T : ElementBase
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== Snapshot Info ===");
            sb.AppendLine($"Name: {snapshot.Name}");
            sb.AppendLine($"Time: {snapshot.TimeStamp}");
            sb.AppendLine($"Desc: {snapshot.Description}");
            sb.AppendLine("--- Data ---");
            sb.Append(Format(snapshot.Data));
            return sb.ToString();
        }

        /// <summary>
        /// 格式化 ElementBase 数据。
        /// </summary>
        public static string Format(ElementBase element, int indentLevel = 0)
        {
            if (element == null) return "null";

            var indent = new string(' ', indentLevel * 2);
            var sb = new StringBuilder();

            // MatrixElement
            if (element is MatrixElement matrix)
            {
                sb.AppendLine($"{indent}Matrix ({matrix.Rows.Count} rows):");
                for (int i = 0; i < matrix.Rows.Count; i++)
                {
                    sb.AppendLine($"{indent}- Row {i}:");
                    foreach (var item in matrix.Rows[i])
                    {
                        // 递归打印，增加缩进
                        sb.AppendLine(Format(item, indentLevel + 2));
                    }
                }
                return sb.ToString().TrimEnd();
            }

            // 处理泛型类型
            var type = element.GetType();
            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();

                // PrimitiveListElement<T>
                if (genericDef == typeof(PrimitiveListElement<>))
                {
                    dynamic listElem = element;
                    sb.AppendLine($"{indent}List ({listElem.Items.Count} items):");
                    var items = (IEnumerable)listElem.Items;
                    int idx = 0;
                    foreach (var item in items)
                    {
                        sb.AppendLine($"{indent}  [{idx++}]: {item}");
                    }
                    return sb.ToString().TrimEnd();
                }

                // DictionaryElement<K, V>
                if (genericDef == typeof(DictionaryElement<,>))
                {
                    dynamic dictElem = element;
                    sb.AppendLine($"{indent}Dictionary ({dictElem.Map.Count} items):");
                    var map = (IDictionary)dictElem.Map;
                    foreach (DictionaryEntry entry in map)
                    {
                        sb.AppendLine($"{indent}  [{entry.Key}]: {entry.Value}");
                    }
                    return sb.ToString().TrimEnd();
                }

                // ValueElement<T>
                if (genericDef == typeof(ValueElement<>))
                {
                    dynamic valElem = element;
                    return $"{indent}{valElem.Value}";
                }
            }

            // 默认 fallback
            return $"{indent}{element}";
        }
    }
}
