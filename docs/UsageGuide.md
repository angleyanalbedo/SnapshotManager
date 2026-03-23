# SnapshotManager 使用指南

本文档详细介绍了 SnapshotManager 库的使用方法，涵盖了从基础容器监控到自定义对象的高级用法。

## 1. 基础容器监控

SnapshotManager 提供了工厂类 `ContainerSnapshotManagerFactory` 和 `ElementSnapshotManagerFactory` 来快速创建针对常见数据结构的管理器。

### 1.1 List (列表)

用于监控 `List<T>` 的变化（新增、删除、修改）。

```csharp
// 创建管理器
var listManager = ContainerSnapshotManagerFactory.CreateListManager<int>();

// 原始数据
var list = new List<int> { 1, 2, 3 };

// 拍摄快照 (需包装为 PrimitiveListElement)
var key = listManager.TakeSnapshot(new PrimitiveListElement<int>(list));

// 修改数据
list.Add(4);

// 对比差异
listManager.DiffWithAndPrint(key, new PrimitiveListElement<int>(list));
```

### 1.2 Dictionary (字典)

用于监控 `Dictionary<K, V>` 的变化（键的新增、删除，值的修改）。

```csharp
// 创建管理器
var dictManager = ContainerSnapshotManagerFactory.CreateDictionaryManager<string, string>();

// 原始数据
var dict = new Dictionary<string, string> { { "key1", "value1" } };

// 拍摄快照
var key = dictManager.TakeSnapshot(new DictionaryElement<string, string>(dict));

// 修改数据
dict["key1"] = "value2";

// 对比差异
dictManager.DiffWithAndPrint(key, new DictionaryElement<string, string>(dict));
```

### 1.3 HashSet (哈希集合)

用于监控 `HashSet<T>` 的变化（元素的新增、删除）。

```csharp
// 创建管理器
var setManager = ContainerSnapshotManagerFactory.CreateHashSetManager<int>();

// 原始数据
var set = new HashSet<int> { 1, 2, 3 };

// 拍摄快照
var key = setManager.TakeSnapshot(new HashSetElement<int>(set));

// 修改数据
set.Remove(2);
set.Add(4);

// 对比差异
setManager.DiffWithAndPrint(key, new HashSetElement<int>(set));
```

### 1.4 Matrix (二维矩阵)

用于监控二维数据结构（如 `List<List<ElementBase>>`）。

```csharp
// 创建管理器
var matrixManager = ElementSnapshotManagerFactory.Create();

// 准备数据 (需构建 ElementBase 的二维列表)
var rows = new List<List<ElementBase>>
{
    new() { new ValueElement<int>(1), new ValueElement<int>(2) }
};

// 拍摄快照
var key = matrixManager.TakeSnapshot(new MatrixElement(rows));

// 修改数据
((ValueElement<int>)rows[0][0]).Value = 99;

// 对比差异
matrixManager.DiffWithAndPrint(key, new MatrixElement(rows));
```

## 2. 高级用法：自定义对象

对于复杂的业务对象，建议继承 `ElementBase` 并实现 `IDeepCloneable`。

### 步骤

1.  **定义模型**: 继承 `ElementBase`，实现 `DeepClone`。
2.  **定义快照**: 继承 `Snapshot<T>`。
3.  **初始化管理器**: 使用 `SnapshotManager<TSnapshot, TModel>` 构造函数，传入自定义 Diff 逻辑（可选）和快照工厂。

```csharp
public class MyData : ElementBase
{
    public string Name { get; set; }
    public override ElementBase DeepClone() => new MyData { Name = Name };
}

public class MySnapshot : Snapshot<MyData>
{
    public MySnapshot(string name, MyData data) : base(name, "", data) { }
}

// 使用反射 Diff
var manager = new SnapshotManager<MySnapshot, MyData>(
    (a, b) => new ElementDiff().Diff(a, b),
    (k, d) => new MySnapshot(k, d)
);
```

## 3. 输出结果

可以使用 `DiffAndPrint` 直接输出到控制台，或使用 `DiffAndFormat` 获取字符串。

```csharp
// 输出到控制台
manager.DiffWithAndPrint(key, currentData);

// 获取字符串
string diffText = manager.DiffWithAndFormat(key, currentData);
Console.WriteLine(diffText);
```
