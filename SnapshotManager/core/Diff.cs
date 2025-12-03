using SnapshotManager.core.@interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core
{
    public class DiffResult
    {
        public bool HasDifference => Items.Count > 0;

        public List<string> Items { get; } = new();

        public void Add(string diff)
        {
            Items.Add(diff);
        }
    }
    public abstract class DiffBase<T> : IDiff<T>
    {
        public abstract DiffResult Compare(T oldValue, T newValue);

        protected void AddIfDifferent(DiffResult result, string name, object a, object b)
        {
            if (!Equals(a, b))
                result.Add($"{name}: {a} → {b}");
        }
    }

    public abstract class Diff<T> : IDiff<T>
    {
        public abstract DiffResult Compare(T oldValue, T newValue);

        protected void AddIfDifferent(DiffResult result, string name, object a, object b)
        {
            if (!Equals(a, b))
                result.Add($"{name}: {a} → {b}");
        }
    }

    public class ListDiff<T> : DiffBase<List<T>>
    {
        private readonly IDiff<T> _elementDiff;

        public ListDiff(IDiff<T> elementDiff)
        {
            _elementDiff = elementDiff;
        }

        public override DiffResult Compare(List<T> oldList, List<T> newList)
        {
            var result = new DiffResult();

            int max = Math.Max(oldList.Count, newList.Count);

            for (int i = 0; i < max; i++)
            {
                T oldItem = i < oldList.Count ? oldList[i] : default;
                T newItem = i < newList.Count ? newList[i] : default;

                var r = _elementDiff.Compare(oldItem, newItem);
                foreach (var item in r.Items)
                    result.Add($"[Index {i}] {item}");
            }

            return result;
        }
    }
    public class MatrixDiff<T> : DiffBase<List<List<T>>>
    {
        private readonly IDiff<T> _elementDiff;

        public MatrixDiff(IDiff<T> elementDiff)
        {
            _elementDiff = elementDiff;
        }

        public override DiffResult Compare(List<List<T>> oldMatrix, List<List<T>> newMatrix)
        {
            var result = new DiffResult();

            int maxRows = Math.Max(oldMatrix.Count, newMatrix.Count);

            for (int i = 0; i < maxRows; i++)
            {
                var rowA = i < oldMatrix.Count ? oldMatrix[i] : null;
                var rowB = i < newMatrix.Count ? newMatrix[i] : null;

                if (rowA == null || rowB == null)
                {
                    result.Add($"Row {i}: Added/Removed");
                    continue;
                }

                int maxCols = Math.Max(rowA.Count, rowB.Count);

                for (int j = 0; j < maxCols; j++)
                {
                    var a = j < rowA.Count ? rowA[j] : default;
                    var b = j < rowB.Count ? rowB[j] : default;

                    var r = _elementDiff.Compare(a, b);
                    foreach (var item in r.Items)
                        result.Add($"({i},{j}) {item}");
                }
            }

            return result;
        }
    }


}
