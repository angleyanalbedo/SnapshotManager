using System;
using System.Collections.Generic;
using System.Text;

namespace SnapshotManager.core.@interface
{
    public interface IDiffPrinter
    {
        void Print(DiffResult result);
    }

    public interface IDiffNodePrinter
    {
        void Print(DiffNode result);
    }

    public interface IDiffResultFormatter
    {
        string Format(DiffResult result);
    }

    public interface IDiffNodeFormatter
    {
        string Format(DiffNode result);
    }

}
