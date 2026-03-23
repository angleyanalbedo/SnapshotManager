using SnapshotManager.Abstruactions;
using System;
using System.Text;

namespace SnapshotManager.Core
{
    // 1. 控制台打印机
    /// <summary>
    /// 控制台差异打印机。
    /// 将差异树以彩色文本形式输出到控制台。
    /// </summary>
    public class ConsoleDiffPrinter : IDiffPrinter
    {
        /// <inheritdoc />
        public void Print(DiffNode node)
        {
            PrintNode(node, 0);
        }

        private void PrintNode(DiffNode node, int indent)
        {
            if (!node.HasDifference) return;

            var pad = new string(' ', indent * 2);
            Console.ForegroundColor = GetColor(node.Type);
            Console.Write($"{pad}{node.Name} [{node.Type}]");

            if (node.Type == DiffType.Modified)
            {
                Console.Write($" : {node.OldValue} -> {node.NewValue}");
            }
            else if (node.Type == DiffType.Added)
            {
                Console.Write($" : (Added) {node.NewValue}");
            }
            else if (node.Type == DiffType.Removed)
            {
                Console.Write($" : (Removed) {node.OldValue}");
            }

            Console.WriteLine();
            Console.ResetColor();

            foreach (var child in node.Children)
            {
                PrintNode(child, indent + 1);
            }
        }

        private ConsoleColor GetColor(DiffType type) => type switch
        {
            DiffType.Added => ConsoleColor.Green,
            DiffType.Removed => ConsoleColor.Red,
            DiffType.Modified => ConsoleColor.Yellow,
            _ => ConsoleColor.Gray
        };
    }

    // 2. 字符串格式化器 (用于日志或测试)
    /// <summary>
    /// 字符串差异格式化器。
    /// 将差异树格式化为缩进的字符串。
    /// </summary>
    public class StringDiffFormatter : IDiffFormatter
    {
        /// <inheritdoc />
        public string Format(DiffNode node)
        {
            var sb = new StringBuilder();
            FormatNode(node, 0, sb);
            return sb.ToString();
        }

        private void FormatNode(DiffNode node, int indent, StringBuilder sb)
        {
            if (!node.HasDifference) return;

            var pad = new string(' ', indent * 2);
            sb.Append($"{pad}{node.Name} [{node.Type}]");

            if (node.Type == DiffType.Modified)
            {
                sb.Append($" : {node.OldValue} -> {node.NewValue}");
            }
            else if (node.Type == DiffType.Added)
            {
                sb.Append($" : (Added) {node.NewValue}");
            }
            else if (node.Type == DiffType.Removed)
            {
                sb.Append($" : (Removed) {node.OldValue}");
            }

            sb.AppendLine();

            foreach (var child in node.Children)
            {
                FormatNode(child, indent + 1, sb);
            }
        }
    }
}
