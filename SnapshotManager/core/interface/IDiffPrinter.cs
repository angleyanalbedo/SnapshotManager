using SnapshotManager.core;

namespace SnapshotManager.core.@interface
{
    /// <summary>
    /// 差异打印机接口。
    /// </summary>
    public interface IDiffPrinter
    {
        /// <summary>
        /// 打印差异节点树。
        /// </summary>
        /// <param name="node">根差异节点。</param>
        void Print(DiffNode node);
    }

    /// <summary>
    /// 差异格式化器接口。
    /// </summary>
    public interface IDiffFormatter
    {
        /// <summary>
        /// 将差异节点树格式化为字符串。
        /// </summary>
        /// <param name="node">根差异节点。</param>
        /// <returns>格式化后的字符串。</returns>
        string Format(DiffNode node);
    }
}
