using SnapshotManager.core.@interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core
{
    public abstract class DiffPrinterBase : IDiffPrinter
    {
        /// <summary>
        /// 差异行前缀（可被子类替换）
        /// </summary>
        protected virtual string Prefix => "Δ ";

        /// <summary>
        /// 行格式化（子类可以 override）
        /// </summary>
        protected virtual string FormatLine(string line)
        {
            return $"{Prefix}{line}";
        }

        public virtual void Print(DiffResult result)
        {
            foreach (var item in result.Items)
            {
                PrintLine(FormatLine(item.ToString()));
            }
        }

        /// <summary>
        /// 抽象输出操作 —— 子类决定真正怎么输出
        /// </summary>
        protected abstract void PrintLine(string line);
    }

    public class ConsoleDiffPrinter : DiffPrinterBase
    {
        protected override void PrintLine(string line)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Δ ");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(line.Substring(2)); // 去掉父类前缀“Δ ”

            Console.ResetColor();
        }
        public override void Print(DiffResult result)
        {
            foreach (var item in result.Items)
            {
                Console.ForegroundColor = item.Kind switch
                {
                    DiffKind.Added => ConsoleColor.Green,
                    DiffKind.Removed => ConsoleColor.Red,
                    DiffKind.Modified => ConsoleColor.Yellow,
                    _ => ConsoleColor.White
                };

                Console.Write($"{item.Kind,-8}");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{item.Path,-20}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{item.OldValue} → {item.NewValue}");

                Console.ResetColor();
            }
        }
    }

    public class JsonDiffPrinter : DiffPrinterBase
    {
        private readonly string _path;

        public JsonDiffPrinter(string path)
        {
            _path = path;
        }

        protected override void PrintLine(string line)
        {
            // 不能逐行写 JSON，所以我们重写 Print 方法
            throw new NotSupportedException("Use Print() directly.");
        }

        public override void Print(DiffResult result)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(result.Items, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }
    }
    public class MarkdownDiffPrinter : DiffPrinterBase
    {
        private readonly string _path;

        public MarkdownDiffPrinter(string path) => _path = path;

        public override void Print(DiffResult result)
        {
            using var writer = new StreamWriter(_path);

            writer.WriteLine("# Diff Report\n");

            foreach (var line in result.Items)
                writer.WriteLine($"- `{line}`");
        }

        protected override void PrintLine(string line)
        {
            // 不使用
        }
    }

    public class ConsoleDiffNodePrinter : IDiffNodePrinter
    {
        public void Print(DiffNode diff)
        {
            PrintNode(diff, 0);
        }

        private void PrintNode(DiffNode node, int indent)
        {
            if (!node.HasDifference)
                return;

            Console.ForegroundColor = GetColor(node.Type);

            Console.WriteLine($"{new string(' ', indent * 2)}{node.Name}  [{node.Type}]");

            if (node.Type == DiffType.Modified)
            {
                Console.WriteLine($"{new string(' ', indent * 2 + 2)}Old: {node.OldValue}");
                Console.WriteLine($"{new string(' ', indent * 2 + 2)}New: {node.NewValue}");
            }

            Console.ResetColor();

            foreach (var child in node.Children)
                PrintNode(child, indent + 1);
        }

        private ConsoleColor GetColor(DiffType type) =>
            type switch
            {
                DiffType.Added => ConsoleColor.Green,
                DiffType.Removed => ConsoleColor.DarkRed,
                DiffType.Modified => ConsoleColor.Yellow,
                _ => ConsoleColor.Gray
            };
    }

    public class StringDiffNodePrinter : IDiffNodeFormatter
    {
        private readonly bool _useAnsiColor;

        /// <summary>
        /// </summary>
        /// <param name="useAnsiColor">
        /// true  – 返回 ANSI 彩色字符串，适合直接 Console.Write ；
        /// false – 返回纯文本，适合断言或写日志。
        /// </param>
        public StringDiffNodePrinter(bool useAnsiColor = false)
        {
            _useAnsiColor = useAnsiColor;
        }

        public string Format(DiffNode diff)
        {
            var sb = new StringBuilder();
            PrintNode(diff, 0, sb);
            return sb.ToString();
        }

        private void PrintNode(DiffNode node, int indent, StringBuilder sb)
        {
            if (!node.HasDifference) return;

            var ind = new string(' ', indent * 2);

            // 节点行：Name [Type]
            var header = $"{ind}{node.Name}  [{node.Type}]";
            sb.AppendLine(Colorize(header, MapColor(node.Type)));

            // 如果是 Modified，再输出 Old / New
            if (node.Type == DiffType.Modified)
            {
                sb.AppendLine(Colorize($"{ind}  Old: {node.OldValue}", MapColor(DiffType.Removed)));
                sb.AppendLine(Colorize($"{ind}  New: {node.NewValue}", MapColor(DiffType.Added)));
            }

            // 递归子节点
            foreach (var child in node.Children)
                PrintNode(child, indent + 1, sb);
        }

        // -------------- 颜色相关 --------------
        private ConsoleColor MapColor(DiffType type) =>
            type switch
            {
                DiffType.Added => ConsoleColor.Green,
                DiffType.Removed => ConsoleColor.DarkRed,
                DiffType.Modified => ConsoleColor.Yellow,
                _ => ConsoleColor.Gray
            };

        private string Colorize(string text, ConsoleColor color)
        {
            if (!_useAnsiColor) return text;

            string code = color switch
            {
                ConsoleColor.Green => "\x1b[32m",
                ConsoleColor.DarkRed => "\x1b[31m",
                ConsoleColor.Yellow => "\x1b[33m",
                _ => "\x1b[37m"
            };
            return $"{code}{text}\x1b[0m";
        }
    }

}
