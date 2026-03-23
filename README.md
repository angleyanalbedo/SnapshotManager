

# SnapshotManager

一个轻量级、泛型的 C# 快照管理与差异对比库。
专为处理复杂数据结构（如列表、矩阵、自定义对象）的历史版本管理和差异分析而设计。

---

## 📖 快速上手指南

### 1. 第一步：定义你的数据类 (Element)

你的数据类必须继承 `ElementBase` 并实现 `DeepClone` 方法。这是为了确保快照存储的是数据的副本，而不是引用。

```csharp
using SnapshotManager.core;

// 定义一个简单的业务对象
public class MyData : ElementBase
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Score { get; set; }

    // 必须实现：深拷贝逻辑
    public override ElementBase DeepClone()
    {
        return new MyData 
        { 
            Id = this.Id, 
            Name = this.Name, 
            Score = this.Score 
        };
    }
}
```

### 2. 第二步：初始化管理器

我们提供了一个工厂方法，专门用于管理 `List<List<ElementBase>>` (二维矩阵) 类型的数据。

```csharp
using SnapshotManager.core;

// 创建管理器实例
var manager = ElementSnapshotManagerFactory.Create();
```

### 3. 第三步：创建快照 (Take Snapshot)

你不需要手动创建复杂的 Snapshot 对象，直接把你的数据扔给管理器即可。

```csharp
// 1. 准备初始数据
var data = new List<List<ElementBase>>
{
    new() { new MyData { Id = 1, Name = "Alice", Score = 80 } },
    new() { new MyData { Id = 2, Name = "Bob", Score = 90 } }
};

// 2. 存快照 (方式 A：自动生成时间戳 Key)
string v1Key = manager.TakeSnapshot(data); 
Console.WriteLine($"已保存快照: {v1Key}");

// 3. 存快照 (方式 B：指定自定义 Key)
manager.TakeSnapshot("Version_1.0", data);
```

### 4. 第四步：修改数据并对比 (Diff)

这是最强大的功能。你可以修改内存中的数据，然后直接和之前的快照进行对比，查看发生了什么变化。

```csharp
// 1. 修改数据：修改 Alice 的分数，删除 Bob
((MyData)data[0][0]).Score = 95; 
data.RemoveAt(1); 

// 2. 【实时对比】当前数据 vs "Version_1.0" 快照
var diffNode = manager.DiffWith("Version_1.0", data);

// 3. 【历史对比】对比两个已存储的快照 (假设你存了 v1 和 v2)
// var historyDiff = manager.Diff("Version_1.0", "Version_2.0");
```

### 5. 第五步：打印差异结果

我们提供了打印机类，可以将复杂的 Diff 树状结构可视化输出。

```csharp
using SnapshotManager.core;

// 1. 创建控制台打印机
var printer = new ConsoleDiffPrinter();

// 2. 打印结果
// 绿色 = 新增, 红色 = 删除, 黄色 = 修改
printer.Print(diffNode);
```

**输出示例：**
```text
ListDiff (Modified)
  Row[0] (Modified)
    Col[0] (Modified)
      Score: 80 -> 95 (Modified)
  Row[1] (Removed)
```

---

## 🛠️ 进阶：如何实现自定义 Diff 算法？

如果你不想用默认的反射对比，或者你的数据结构不是二维数组，你可以自定义 Diff 算法。

### 1. 实现 IDiff 接口

```csharp
public class MyCustomDiff : IDiff<MyComplexData>
{
    public DiffNode Diff(MyComplexData? oldVal, MyComplexData? newVal)
    {
        var node = new DiffNode { Name = "MyData" };
        
        // 在这里写你的对比逻辑...
        if (oldVal.Value != newVal.Value)
        {
            node.Type = DiffType.Modified;
            node.OldValue = oldVal.Value;
            node.NewValue = newVal.Value;
        }
        
        return node;
    }
}
```

### 2. 组装 Manager

```csharp
var myManager = new SnapshotManager<MyComplexData>(
    new MyCustomDiff(),            // 1. 你的 Diff 算法
    (key, data) => new Snapshot<MyComplexData>(key, data) // 2. 告诉 Manager 如何包装快照
);
```

---

## 🏗️ 核心概念

*   **SnapshotManager**: 总管家。负责存取快照、执行 Diff。
*   **Snapshot**: 数据的容器。它会在内部深拷贝一份数据，防止外部修改影响历史记录。
*   **DiffNode**: 差异树节点。包含差异类型（Added/Removed/Modified）、旧值、新值以及子节点。
*   **ElementBase**: 所有数据项的基类，强制要求实现深拷贝。

---

## 📦 打包与发布

在 .NET 项目中，我们使用 `dotnet pack` 命令来生成 NuGet 包。请在终端（Terminal）中运行以下命令：

```bash
dotnet pack -c Release
```

### 命令说明：
*   `dotnet pack`: 执行打包操作。
*   `-c Release`: 使用 `Release` 配置进行构建（优化代码，移除调试符号），这是发布正式包的标准做法。

### 预期结果：
命令执行成功后，你会在 `SnapshotManager\bin\Release\` 目录下看到生成的 `.nupkg` 文件（例如 `SnapshotManager.1.1.3.nupkg`）。你可以将该文件上传到 NuGet.org 或私有源。
