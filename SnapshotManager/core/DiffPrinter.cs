using SnapshotManager.core.@interface;
using System;
using System.Text;

namespace SnapshotManager.core
{
    // 1. 控制台打印机
    public class ConsoleDiffPrinter : IDiffPrinter
    {
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
    public class StringDiffFormatter : IDiffFormatter
    {
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
