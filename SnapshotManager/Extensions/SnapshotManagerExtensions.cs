using SnapshotManager.Abstruactions;
using SnapshotManager.Core;

namespace SnapshotManager.Extensions
{
    public static class SnapshotManagerExtensions
    {
        /// <summary>
        /// 比较两个快照并直接打印差异到控制台
        /// </summary>
        public static void DiffAndPrint<T>(this ISnapshotManager<T> manager, string snapA, string snapB, IDiffPrinter? printer = null)
        {
            var diffNode = manager.Diff(snapA, snapB);
            printer ??= new ConsoleDiffPrinter();
            printer.Print(diffNode);
        }

        /// <summary>
        /// 比较基准快照与当前数据，并直接打印差异到控制台
        /// </summary>
        public static void DiffWithAndPrint<T>(this ISnapshotManager<T> manager, string baseSnapKey, T currentData, IDiffPrinter? printer = null)
        {
            var diffNode = manager.DiffWith(baseSnapKey, currentData);
            printer ??= new ConsoleDiffPrinter();
            printer.Print(diffNode);
        }

        /// <summary>
        /// 比较两个快照并返回格式化后的差异字符串
        /// </summary>
        public static string DiffAndFormat<T>(this ISnapshotManager<T> manager, string snapA, string snapB, IDiffFormatter? formatter = null)
        {
            var diffNode = manager.Diff(snapA, snapB);
            formatter ??= new StringDiffFormatter();
            return formatter.Format(diffNode);
        }

        /// <summary>
        /// 比较基准快照与当前数据，并返回格式化后的差异字符串
        /// </summary>
        public static string DiffWithAndFormat<T>(this ISnapshotManager<T> manager, string baseSnapKey, T currentData, IDiffFormatter? formatter = null)
        {
            var diffNode = manager.DiffWith(baseSnapKey, currentData);
            formatter ??= new StringDiffFormatter();
            return formatter.Format(diffNode);
        }
    }
}
