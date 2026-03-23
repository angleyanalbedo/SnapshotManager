

# SnapshotManager

SnapshotManager 是一个轻量级的 .NET 库，用于管理对象状态的快照（Snapshot），并对不同时间点的数据进行差异比较（Diff）。

它特别适用于需要追踪复杂数据结构（如列表、二维矩阵、自定义对象）变化历史的场景，并提供了直观的差异输出功能。

## ✨ 核心特性

*   **快照管理**：轻松捕获和存储数据的某一时刻状态。
*   **深度克隆**：基于 `IDeepCloneable` 接口，确保快照数据的独立性，不受后续修改影响。
*   **智能 Diff**：支持列表（List）和矩阵（Matrix/Grid）级别的差异检测，能够识别：
    *   新增（Added）
    *   删除（Removed）
    *   修改（Modified）
*   **可视化输出**：内置 `ConsoleDiffPrinter` 和 `StringDiffFormatter`，可直接打印或格式化差异结果。
*   **扩展性**：支持自定义数据类型（继承 `ElementBase`）和自定义 Diff 逻辑。

## 🚀 快速开始 (Quick Start)

### 1. 定义数据模型

首先，定义你的数据类。为了支持快照和比较，你的类需要继承 `ElementBase` 并实现 `DeepClone` 方法。

```csharp
using SnapshotManager.Models;

public class MyItem : ElementBase
{
    public int Id { get; set; }
    public string Name { get; set; }

    // 必须实现 DeepClone 以保证快照数据的独立性
    public override ElementBase DeepClone()
    {
        return new MyItem 
        { 
            Id = this.Id, 
            Name = this.Name 
        };
    }

    // 重写 Equals 和 GetHashCode 以便 Diff 算法正确判断内容变化
    public override bool Equals(object? obj)
    {
        if (obj is MyItem other)
        {
            return Id == other.Id && Name == other.Name;
        }
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Id, Name);
    
    public override string ToString() => $"[{Id}: {Name}]";
}
```

### 2. 初始化管理器

你可以使用工厂方法创建一个专门管理二维元素矩阵的管理器，或者直接实例化泛型管理器。

```csharp
using SnapshotManager.Core;
using SnapshotManager.Models;

// 创建一个管理 List<List<ElementBase>> 的管理器
var manager = ElementSnapshotManagerFactory.Create();
```

### 3. 捕获快照 (Take Snapshot)

准备初始数据并保存为快照。

```csharp
// 初始化数据 (2行2列的矩阵)
var data = new List<List<ElementBase>>
{
    new List<ElementBase> { new MyItem { Id = 1, Name = "A" }, new MyItem { Id = 2, Name = "B" } },
    new List<ElementBase> { new MyItem { Id = 3, Name = "C" }, new MyItem { Id = 4, Name = "D" } }
};

// 保存快照，命名为 "v1"
manager.TakeSnapshot("v1", data);
Console.WriteLine("快照 v1 已保存。");
```

### 4. 修改数据并比较 (Diff)

修改当前数据，然后与之前的快照进行对比。

```csharp
using SnapshotManager.Extensions; // 引入扩展方法以使用 DiffWithAndPrint

// 修改数据：
// 1. 修改 (1,1) 的值
((MyItem)data[1][1]).Name = "D_Modified";
// 2. 删除第一行
data.RemoveAt(0);

// 与 "v1" 快照进行对比，并直接打印结果
// 需要传入一个 IDiffPrinter 实现，例如 ConsoleDiffPrinter
manager.DiffWithAndPrint("v1", data, new ConsoleDiffPrinter());
```

### 5. 仅获取差异对象

如果你不需要打印，只需要差异数据结构（`DiffNode`）进行后续处理：

```csharp
// 获取差异节点树
var diffNode = manager.DiffWith("v1", data);

// 或者比较两个已存储的快照
manager.TakeSnapshot("v2", data);
var diffBetweenSnapshots = manager.Diff("v1", "v2");
```

## 📂 项目结构

*   **SnapshotManager.Abstractions**: 定义核心接口 (`ISnapshotManager`, `IDiff`, `IDeepCloneable`)。
*   **SnapshotManager.Models**: 基础数据模型 (`Snapshot`, `ElementBase`)。
*   **SnapshotManager.Core**: 核心逻辑实现 (`SnapshotManager`, `Diff` 算法)。
*   **SnapshotManager.Extensions**: 便捷操作的扩展方法。

## 📄 License

[LICENSE.txt](LICENSE.txt)
