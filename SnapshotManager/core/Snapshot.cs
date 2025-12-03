using SnapshotManager.core.@interface;
using System.Xml.Linq;

namespace SnapshotManager.core
{
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

}
