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
                PrintLine(FormatLine(item));
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

}
