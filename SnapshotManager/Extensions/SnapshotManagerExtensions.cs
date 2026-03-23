using SnapshotManager.Abstruactions;
using SnapshotManager.Core;
using SnapshotManager.Models;

namespace SnapshotManager.Extensions
{
    /// <summary>
    /// 提供 ISnapshotManager 的扩展方法，用于简化 Diff 操作、打印和格式化输出。
    /// </summary>
    public static class SnapshotManagerExtensions
    {
        /// <summary>
        /// 比较两个快照并直接打印差异到控制台。
        /// </summary>
        /// <typeparam name="TSnapshot">具体的快照类型。</typeparam>
        /// <typeparam name="TModel">被管理的数据模型类型。</typeparam>
        /// <param name="manager">快照管理器实例。</param>
        /// <param name="snapA">第一个快照的键（旧值）。</param>
        /// <param name="snapB">第二个快照的键（新值）。</param>
        /// <param name="printer">可选的差异打印器。如果为 null，默认使用 ConsoleDiffPrinter。</param>
        public static void DiffAndPrint<TSnapshot, TModel>(this ISnapshotManager<TSnapshot, TModel> manager, string snapA, string snapB, IDiffPrinter? printer = null)
            where TSnapshot : Snapshot<TModel>
            where TModel : ElementBase
        {
            var diffNode = manager.Diff(snapA, snapB);
            printer ??= new ConsoleDiffPrinter();
            printer.Print(diffNode);
        }

        /// <summary>
        /// 比较基准快照与当前数据，并直接打印差异到控制台。
        /// </summary>
        /// <typeparam name="TSnapshot">具体的快照类型。</typeparam>
        /// <typeparam name="TModel">被管理的数据模型类型。</typeparam>
        /// <param name="manager">快照管理器实例。</param>
        /// <param name="baseSnapKey">基准快照的键（旧值）。</param>
        /// <param name="currentData">当前数据对象（新值）。</param>
        /// <param name="printer">可选的差异打印器。如果为 null，默认使用 ConsoleDiffPrinter。</param>
        public static void DiffWithAndPrint<TSnapshot, TModel>(this ISnapshotManager<TSnapshot, TModel> manager, string baseSnapKey, TModel currentData, IDiffPrinter? printer = null)
            where TSnapshot : Snapshot<TModel>
            where TModel : ElementBase
        {
            var diffNode = manager.DiffWith(baseSnapKey, currentData);
            printer ??= new ConsoleDiffPrinter();
            printer.Print(diffNode);
        }

        /// <summary>
        /// 比较两个快照并返回格式化后的差异字符串。
        /// </summary>
        /// <typeparam name="TSnapshot">具体的快照类型。</typeparam>
        /// <typeparam name="TModel">被管理的数据模型类型。</typeparam>
        /// <param name="manager">快照管理器实例。</param>
        /// <param name="snapA">第一个快照的键（旧值）。</param>
        /// <param name="snapB">第二个快照的键（新值）。</param>
        /// <param name="formatter">可选的差异格式化器。如果为 null，默认使用 StringDiffFormatter。</param>
        /// <returns>格式化后的差异字符串。</returns>
        public static string DiffAndFormat<TSnapshot, TModel>(this ISnapshotManager<TSnapshot, TModel> manager, string snapA, string snapB, IDiffFormatter? formatter = null)
            where TSnapshot : Snapshot<TModel>
            where TModel : ElementBase
        {
            var diffNode = manager.Diff(snapA, snapB);
            formatter ??= new StringDiffFormatter();
            return formatter.Format(diffNode);
        }

        /// <summary>
        /// 比较基准快照与当前数据，并返回格式化后的差异字符串。
        /// </summary>
        /// <typeparam name="TSnapshot">具体的快照类型。</typeparam>
        /// <typeparam name="TModel">被管理的数据模型类型。</typeparam>
        /// <param name="manager">快照管理器实例。</param>
        /// <param name="baseSnapKey">基准快照的键（旧值）。</param>
        /// <param name="currentData">当前数据对象（新值）。</param>
        /// <param name="formatter">可选的差异格式化器。如果为 null，默认使用 StringDiffFormatter。</param>
        /// <returns>格式化后的差异字符串。</returns>
        public static string DiffWithAndFormat<TSnapshot, TModel>(this ISnapshotManager<TSnapshot, TModel> manager, string baseSnapKey, TModel currentData, IDiffFormatter? formatter = null)
            where TSnapshot : Snapshot<TModel>
            where TModel : ElementBase
        {
            var diffNode = manager.DiffWith(baseSnapKey, currentData);
            formatter ??= new StringDiffFormatter();
            return formatter.Format(diffNode);
        }
    }
}
