

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

SnapshotManager 提供了工厂类，可以快速创建针对常见容器（List, Dictionary, Matrix）的管理器。

**注意**：为了统一管理，所有数据在传入管理器前都需要包裹在对应的 `Element` 包装器中（如 `PrimitiveListElement`, `DictionaryElement` 等）。

### 1. 监控列表 (List)

使用 `ContainerSnapshotManagerFactory.CreateListManager<T>` 来监控基础类型列表。

```csharp
using SnapshotManager.Core;
using SnapshotManager.Models;
using SnapshotManager.Extensions; // 引入扩展方法以使用 DiffWithAndPrint

// 1. 创建管理器
var listManager = ContainerSnapshotManagerFactory.CreateListManager<int>();

// 2. 准备数据
var myList = new List<int> { 1, 2, 3 };

// 3. 创建快照 (注意：需要使用 PrimitiveListElement 包装)
// TakeSnapshot 返回生成的快照 Key
string snapKey = listManager.TakeSnapshot(new PrimitiveListElement<int>(myList));

// 4. 修改数据
myList.Add(4);
myList[0] = 99;

// 5. 实时对比并打印差异
// 输出：Index[0] Modified (1 -> 99), Index[3] Added (4)
listManager.DiffWithAndPrint(snapKey, new PrimitiveListElement<int>(myList));
```

### 2. 监控字典 (Dictionary)

使用 `ContainerSnapshotManagerFactory.CreateDictionaryManager<K, V>` 来监控字典。

```csharp
// 1. 创建管理器
var dictManager = ContainerSnapshotManagerFactory.CreateDictionaryManager<string, string>();

// 2. 准备数据
var myDict = new Dictionary<string, string> { { "A", "Hello" }, { "B", "World" } };

// 3. 创建快照 (注意：使用 DictionaryElement 包装)
string snapKey = dictManager.TakeSnapshot(new DictionaryElement<string, string>(myDict));

// 4. 修改数据
myDict["A"] = "Hi";
myDict.Remove("B");

// 5. 实时对比
dictManager.DiffWithAndPrint(snapKey, new DictionaryElement<string, string>(myDict));
```

### 3. 监控二维矩阵 (Matrix / List of Lists)

使用 `ElementSnapshotManagerFactory.Create()` 来监控复杂的二维数据结构。

```csharp
// 1. 创建管理器
var matrixManager = ElementSnapshotManagerFactory.Create();

// 2. 准备数据 (List<List<ElementBase>>)
var rows = new List<List<ElementBase>>
{
    new() { new ValueElement<int>(1), new ValueElement<int>(2) },
    new() { new ValueElement<int>(3), new ValueElement<int>(4) }
};

// 3. 创建快照 (使用 MatrixElement 包装)
string snapKey = matrixManager.TakeSnapshot(new MatrixElement(rows));

// 4. 修改数据
((ValueElement<int>)rows[0][0]).Value = 999;

// 5. 对比
matrixManager.DiffWithAndPrint(snapKey, new MatrixElement(rows));
```

---

## 高级用法 (Advanced Usage)

对于自定义的复杂业务对象，你可以通过继承 `ElementBase` 并实现 `IDiff` 接口来完全控制快照和对比逻辑。

### 1. 定义数据模型

继承 `ElementBase` 并实现 `DeepClone`。

```csharp
public class UserProfile : ElementBase
{
    public string UserName { get; set; }
    public int Level { get; set; }

    // 必须实现深拷贝，确保快照数据的独立性
    public override ElementBase DeepClone()
    {
        return new UserProfile { UserName = UserName, Level = Level };
    }
}
```

### 2. 自定义快照类

定义一个继承自 `Snapshot<T>` 的类。

```csharp
public class UserSnapshot : Snapshot<UserProfile>
{
    public UserSnapshot(string name, UserProfile data) : base(name, "User Snapshot", data) { }
}
```

### 3. 配置 SnapshotManager

使用通用的 `SnapshotManager<TSnapshot, TModel>` 类，并注入自定义的 Diff 逻辑（可选）和快照工厂。

```csharp
// 初始化管理器
// 显式传入比较逻辑（这里使用内置的 ElementDiff 进行反射比较）
var userManager = new SnapshotManager<UserSnapshot, UserProfile>(
    (a, b) => new ElementDiff().Diff(a, b),
    (key, data) => new UserSnapshot(key, data) // 注入快照创建工厂
);

// 使用
var user = new UserProfile { UserName = "Admin", Level = 1 };
var key = userManager.TakeSnapshot(user);

user.Level = 2;
userManager.DiffWithAndPrint(key, user);
```

## 输出与格式化

库提供了 `ISnapshotPrinter` 和 `IDiffPrinter` 接口，以及默认实现：

*   **ConsoleDiffPrinter**: 将差异树以彩色文本输出到控制台。
*   **StringDiffFormatter**: 将差异树格式化为字符串。
*   **ConsoleSnapshotPrinter**: 打印快照内容。
*   **StringSnapshotPrinter**: 将快照内容导出为字符串。

```csharp
// 导出差异为字符串
string diffText = listManager.DiffWithAndFormat(snapKey, new PrimitiveListElement<int>(myList));
// File.WriteAllText("diff.txt", diffText);
```

## 可视化输出 (Graphviz & Mermaid)

除了文本输出，SnapshotManager 还支持生成 Graphviz DOT 和 Mermaid 流程图代码，方便集成到文档或可视化工具中。

```csharp
using SnapshotManager.Output;

// 生成 Graphviz DOT 代码
var dot = listManager.DiffWithAndFormat(snapKey, currentData, new GraphvizDiffFormatter());
// File.WriteAllText("diff.dot", dot);

// 生成 Mermaid 代码
var mermaid = listManager.DiffWithAndFormat(snapKey, currentData, new MermaidDiffFormatter());
Console.WriteLine(mermaid);
```

## 📂 项目结构

*   **SnapshotManager.Abstractions**: 定义核心接口 (`ISnapshotManager`, `IDiff`, `IDeepCloneable`)。
*   **SnapshotManager.Models**: 基础数据模型 (`Snapshot`, `ElementBase`)。
*   **SnapshotManager.Core**: 核心逻辑实现 (`SnapshotManager`, `Diff` 算法)。
*   **SnapshotManager.Extensions**: 便捷操作的扩展方法。

## 📚 文档 (Documentation)

更详细的使用指南和最佳实践，请参阅 [docs/UsageGuide.md](docs/UsageGuide.md)。

## 📄 License

[LICENSE.txt](LICENSE.txt)
