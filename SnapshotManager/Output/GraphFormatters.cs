using SnapshotManager.Abstruactions;
using SnapshotManager.Core;
using System.Text;

namespace SnapshotManager.Output
{
    /// <summary>
    /// 将差异树格式化为 Graphviz DOT 语法的格式化器。
    /// </summary>
    public class GraphvizDiffFormatter : IDiffFormatter
    {
        /// <inheritdoc />
        public string Format(DiffNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph DiffTree {");
            sb.AppendLine("  node [shape=box, style=filled, fontname=\"Arial\"];");
            
            int nodeIdCounter = 0;
            Traverse(node, sb, ref nodeIdCounter, -1);

            sb.AppendLine("}");
            return sb.ToString();
        }

        private int Traverse(DiffNode node, StringBuilder sb, ref int counter, int parentId)
        {
            int currentId = counter++;
            string color = GetColor(node.Type);
            string label = GetLabel(node);
            
            // 定义节点
            sb.AppendLine($"  node{currentId} [label=\"{Escape(label)}\", fillcolor=\"{color}\"];");

            // 定义边
            if (parentId >= 0)
            {
                sb.AppendLine($"  node{parentId} -> node{currentId};");
            }

            // 递归子节点
            foreach (var child in node.Children)
            {
                Traverse(child, sb, ref counter, currentId);
            }

            return currentId;
        }

        private string GetColor(DiffType type)
        {
            return type switch
            {
                DiffType.Added => "#ccffcc",    // Light Green
                DiffType.Removed => "#ffcccc",  // Light Red
                DiffType.Modified => "#ffffcc", // Light Yellow
                _ => "#ffffff"                  // White
            };
        }

        private string GetLabel(DiffNode node)
        {
            var sb = new StringBuilder();
            sb.Append(node.Name);
            if (node.Type == DiffType.Modified)
            {
                sb.Append($"\\n{SafeString(node.OldValue)} -> {SafeString(node.NewValue)}");
            }
            else if (node.Type == DiffType.Added)
            {
                sb.Append($"\\n(New: {SafeString(node.NewValue)})");
            }
            else if (node.Type == DiffType.Removed)
            {
                sb.Append($"\\n(Old: {SafeString(node.OldValue)})");
            }
            return sb.ToString();
        }

        private string SafeString(object? obj) => obj?.ToString() ?? "null";

        private string Escape(string text)
        {
            return text.Replace("\"", "\\\"");
        }
    }

    /// <summary>
    /// 将差异树格式化为 Mermaid 流程图语法的格式化器。
    /// </summary>
    public class MermaidDiffFormatter : IDiffFormatter
    {
        /// <inheritdoc />
        public string Format(DiffNode node)
        {
            var sb = new StringBuilder();
            sb.AppendLine("graph TD");
            
            // 定义样式类
            sb.AppendLine("  classDef added fill:#ccffcc,stroke:#33cc33,stroke-width:2px;");
            sb.AppendLine("  classDef removed fill:#ffcccc,stroke:#cc3333,stroke-width:2px;");
            sb.AppendLine("  classDef modified fill:#ffffcc,stroke:#cccc33,stroke-width:2px;");
            sb.AppendLine("  classDef none fill:#ffffff,stroke:#333333,stroke-width:1px;");

            int nodeIdCounter = 0;
            Traverse(node, sb, ref nodeIdCounter, -1);

            return sb.ToString();
        }

        private int Traverse(DiffNode node, StringBuilder sb, ref int counter, int parentId)
        {
            int currentId = counter++;
            string label = GetLabel(node);
            string styleClass = GetStyleClass(node.Type);

            // Mermaid 节点定义: id["label"]:::styleClass
            // 注意：Mermaid ID 不能包含特殊字符，这里使用数字 ID
            sb.AppendLine($"  N{currentId}[\"{Escape(label)}\"]:::{styleClass}");

            if (parentId >= 0)
            {
                sb.AppendLine($"  N{parentId} --> N{currentId}");
            }

            foreach (var child in node.Children)
            {
                Traverse(child, sb, ref counter, currentId);
            }

            return currentId;
        }

        private string GetStyleClass(DiffType type)
        {
            return type switch
            {
                DiffType.Added => "added",
                DiffType.Removed => "removed",
                DiffType.Modified => "modified",
                _ => "none"
            };
        }

        private string GetLabel(DiffNode node)
        {
            var sb = new StringBuilder();
            sb.Append(node.Name);
            if (node.Type == DiffType.Modified)
            {
                sb.Append($"<br/>{SafeString(node.OldValue)} -> {SafeString(node.NewValue)}");
            }
            else if (node.Type == DiffType.Added)
            {
                sb.Append($"<br/>(New: {SafeString(node.NewValue)})");
            }
            else if (node.Type == DiffType.Removed)
            {
                sb.Append($"<br/>(Old: {SafeString(node.OldValue)})");
            }
            return sb.ToString();
        }

        private string SafeString(object? obj) => obj?.ToString() ?? "null";

        private string Escape(string text)
        {
            return text.Replace("\"", "&quot;");
        }
    }
}
