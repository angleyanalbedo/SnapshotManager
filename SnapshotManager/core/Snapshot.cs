using SnapshotManager.core.@interface;
using System.Xml.Linq;

namespace SnapshotManager.core
{
    /// <summary>
    /// Represents a snapshot of data of a specified type, including metadata such as timestamp, name, and description.
    /// </summary>
    /// <remarks>Use this class to capture and store the state of an object or value at a specific point in
    /// time, along with descriptive metadata. The snapshot is immutable after creation, except for the metadata
    /// properties, which can be modified if needed.</remarks>
    /// <typeparam name="T">The type of data contained in the snapshot.</typeparam>
    public class Snapshot<T>
    {
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        // 受保护 —— 子类可使用，但外部不能修改
        protected T _snap;

        public Snapshot(string name, string description, T data)
        {
            Name = name;
            Description = description;
            _snap = data;
        }

        public T GetData() => _snap;
        public T Data { get => _snap; }
    }
    public class ListSnapshot<T> : Snapshot<List<T>>
    where T : IDeepCloneable<T>
    {
        public ListSnapshot(string name, string description, List<T> source)
            : base(name, description, Clone(source))
        {
        }

        private static List<T> Clone(List<T> src)
        {
            var result = new List<T>(src.Count);
            foreach (var item in src)
                result.Add(item.DeepClone());
            return result;
        }
    }

    public class MatrixSnapshot<T> : Snapshot<List<List<T>>>
    where T : IDeepCloneable<T>
    {
        public MatrixSnapshot(string name, string description, List<List<T>> src)
            : base(name, description, Clone(src))
        {
        }

        private static List<List<T>> Clone(List<List<T>> src)
        {
            var result = new List<List<T>>(src.Count);

            foreach (var row in src)
            {
                var newRow = new List<T>(row.Count);
                foreach (var cell in row)
                    newRow.Add(cell.DeepClone());
                result.Add(newRow);
            }

            return result;
        }
    }


    /// <summary>
    /// Represents a snapshot of a two-dimensional array of elements, preserving the state of a collection of element
    /// lists at a specific point in time.
    /// </summary>
    /// <remarks>Use this class to capture and work with the state of a two-dimensional collection of
    /// elements, such as a grid or matrix, for undo/redo operations or historical analysis. The snapshot is a deep
    /// clone of the original data, ensuring that changes to the source collection do not affect the stored
    /// snapshot.</remarks>
    public class ElementArraySnapshot : Snapshot<List<List<ElementBase>>>
    {
        public ElementArraySnapshot(
            string name,
            string description,
            List<List<ElementBase>> src)
            : base(name, description, DeepClone2D(src))
        { }

        private static List<List<ElementBase>> DeepClone2D(List<List<ElementBase>> src)
        {
            return src.Select(
                row => row.Select(e => e?.DeepClone()).ToList()
            ).ToList();
        }
    }

}
