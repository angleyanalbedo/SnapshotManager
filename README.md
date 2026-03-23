

# SnapshotManager

一个轻量级、泛型的 C# 快照管理与差异对比库。
专为处理复杂数据结构（如列表、矩阵、自定义对象）的历史版本管理和差异分析而设计。

## ✨ 特性

- **泛型支持**：支持任意实现了 `IDeepCloneable` 的数据类型。
- **自动深拷贝**：快照存储时自动进行深拷贝，确保历史数据不被后续修改污染。
- **差异对比 (Diff)**：内置 List、Matrix (二维数组) 和自定义对象的差异对比算法。
- **灵活的 API**：支持直接存取数据对象，也支持精细控制快照对象。
- **可视化输出**：支持将差异结果打印到控制台或格式化为字符串。

## 🚀 快速开始

### 1. 初始化管理器

推荐使用工厂方法创建针对特定类型的管理器（例如针对 `ElementBase` 二维矩阵的管理器）：

```csharp
using SnapshotManager.core;

// 使用工厂创建预配置好的管理器
// 该管理器已内置了 MatrixDiff 和 ElementDiff 算法
var manager = ElementSnapshotManagerFactory.Create();
```

### 2. 创建快照 (Take Snapshot)

无需手动创建 `Snapshot` 对象，直接将数据交给管理器即可：

```csharp
var data = new List<List<ElementBase>> { /* ... 初始化数据 ... */ };

// 方式 A: 自动生成 Key (使用时间戳，如 "20231027_103000_123")
string key1 = manager.TakeSnapshot(data); 
Console.WriteLine($"Snapshot taken: {key1}");

// 方式 B: 指定 Key
manager.TakeSnapshot("v1.0", data);
```

### 3. 差异对比 (Diff)

#### ⚡ 实时对比 (DiffWith)
这是最常用的方式。你可以在修改数据后，直接与之前的快照进行对比，而**不需要**先把当前数据存为快照。

```csharp
// 1. 修改内存中的数据
data[0][0] = new MyElement(999);

// 2. 直接对比 "v1.0" 快照与当前 data
// 返回一个 DiffNode 树状结构
var diffNode = manager.DiffWith("v1.0", data);

// 3. 打印差异
var printer = new ConsoleDiffPrinter();
printer.Print(diffNode);
```

#### 📜 历史对比 (Diff)
对比两个已经存储的历史版本：

```csharp
manager.TakeSnapshot("v2.0", data);

// 对比两个已存储的版本
var diffNode = manager.Diff("v1.0", "v2.0");
```

## 🏗️ 核心架构

### SnapshotManager<T>
核心控制器。负责存储快照历史、查找快照以及执行 Diff 操作。
- `TakeSnapshot(T data)`: 快捷保存数据（自动深拷贝）。
- `DiffWith(string key, T data)`: 快捷对比当前数据与历史快照。

### Snapshot<T>
快照容器。
- 确保存储的数据是原始数据的**深拷贝**。
- 支持 `ListSnapshot`, `MatrixSnapshot` 等变体以优化特定结构的克隆性能。

### Diff 算法
- `IDiff<T>`: 核心对比接口。
- `ListDiff<T>`: 对比两个列表（检测增加、删除、修改）。
- `MatrixDiff<T>`: 对比二维矩阵（检测行增删、单元格修改）。
- `ElementDiff`: 对比自定义元素属性。

## 🛠️ 扩展指南

要支持自定义类型的快照管理：

1. **实现接口**：数据类需实现 `IDeepCloneable<T>`。
2. **定义 Diff**：实现 `IDiff<T>` 接口定义对比逻辑。
3. **配置 Manager**：

```csharp
var myManager = new SnapshotManager<MyType>(
    new MyTypeDiff(),            // Diff 算法
    data => new MySnapshot(data) // 工厂委托：告诉 Manager 如何把数据包装成快照
);
```
