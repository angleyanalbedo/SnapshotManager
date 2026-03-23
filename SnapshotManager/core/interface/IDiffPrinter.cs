using SnapshotManager.core;

namespace SnapshotManager.core.@interface
{
    public interface IDiffPrinter
    {
        void Print(DiffNode node);
    }

    public interface IDiffFormatter
    {
        string Format(DiffNode node);
    }
}
