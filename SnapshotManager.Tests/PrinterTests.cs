using SnapshotManager.Core;
using SnapshotManager.Models;
using SnapshotManager.Output;
using System.Collections.Generic;
using Xunit;

namespace SnapshotManager.Tests
{
    public class PrinterTests
    {
        [Fact]
        public void StringSnapshotPrinter_ShouldPrintSnapshotDetails()
        {
            // Arrange
            var data = new ValueElement<int>(42);
            var snapshot = new Snapshot<ValueElement<int>>("TestSnap", "Test Desc", data);
            var printer = new StringSnapshotPrinter();

            // Act
            printer.Print(snapshot);
            var result = printer.Result;

            // Assert
            Assert.Contains("Name: TestSnap", result);
            Assert.Contains("Desc: Test Desc", result);
            Assert.Contains("42", result);
        }

        [Fact]
        public void SnapshotFormatter_ShouldFormatMatrix()
        {
            // Arrange
            var rows = new List<List<ElementBase>>
            {
                new() { new ValueElement<int>(1), new ValueElement<int>(2) }
            };
            var matrix = new MatrixElement(rows);

            // Act
            var result = SnapshotFormatter.Format(matrix);

            // Assert
            Assert.Contains("Matrix (1 rows)", result);
            Assert.Contains("Row 0", result);
            Assert.Contains("1", result);
            Assert.Contains("2", result);
        }

        [Fact]
        public void SnapshotFormatter_ShouldFormatList()
        {
            // Arrange
            var list = new List<string> { "A", "B" };
            var element = new PrimitiveListElement<string>(list);

            // Act
            var result = SnapshotFormatter.Format(element);

            // Assert
            Assert.Contains("List (2 items)", result);
            Assert.Contains("[0]: A", result);
            Assert.Contains("[1]: B", result);
        }

        [Fact]
        public void SnapshotFormatter_ShouldFormatDictionary()
        {
            // Arrange
            var dict = new Dictionary<string, int> { { "Key1", 100 } };
            var element = new DictionaryElement<string, int>(dict);

            // Act
            var result = SnapshotFormatter.Format(element);

            // Assert
            Assert.Contains("Dictionary (1 items)", result);
            Assert.Contains("[Key1]: 100", result);
        }

        [Fact]
        public void SnapshotFormatter_ShouldFormatHashSet()
        {
            // Arrange
            var set = new HashSet<int> { 99 };
            var element = new HashSetElement<int>(set);

            // Act
            var result = SnapshotFormatter.Format(element);

            // Assert
            Assert.Contains("HashSet (1 items)", result);
            Assert.Contains("- 99", result);
        }

        [Fact]
        public void GraphvizDiffFormatter_ShouldGenerateValidDot()
        {
            var node = new DiffNode { Name = "Root", Type = DiffType.None };
            node.Children.Add(new DiffNode { Name = "Child", Type = DiffType.Added, NewValue = 123 });

            var formatter = new GraphvizDiffFormatter();
            var output = formatter.Format(node);

            Assert.Contains("digraph DiffTree", output);
            Assert.Contains("label=\"Root\"", output);
            Assert.Contains("label=\"Child\\n(New: 123)\"", output);
            Assert.Contains("fillcolor=\"#ccffcc\"", output); // Added color
        }

        [Fact]
        public void MermaidDiffFormatter_ShouldGenerateValidMermaid()
        {
            var node = new DiffNode { Name = "Root", Type = DiffType.None };
            node.Children.Add(new DiffNode { Name = "Child", Type = DiffType.Modified, OldValue = 1, NewValue = 2 });

            var formatter = new MermaidDiffFormatter();
            var output = formatter.Format(node);

            Assert.Contains("graph TD", output);
            Assert.Contains("[\"Root\"]:::none", output);
            Assert.Contains("[\"Child<br/>1 -> 2\"]:::modified", output);
        }
    }
}
