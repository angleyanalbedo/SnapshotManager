# API 参考 (API Reference)

## 工厂类 (Factories)

### `ContainerSnapshotManagerFactory`
用于创建常见容器的管理器。
*   `CreateListManager<T>()`: 创建 `List<T>` 管理器。
*   `CreateDictionaryManager<K, V>()`: 创建 `Dictionary<K, V>` 管理器。
*   `CreateHashSetManager<T>()`: 创建 `HashSet<T>` 管理器。

### `ElementSnapshotManagerFactory`
*   `Create()`: 创建用于 `MatrixElement` (二维矩阵) 的管理器。

## 管理器 (Manager)

### `SnapshotManager<TSnapshot, TModel>`
核心管理类。
*   `TakeSnapshot(TModel data)`: 创建快照。
*   `DiffWith(string baseKey, TModel currentData)`: 与当前数据对比。
*   `Diff(string keyA, string keyB)`: 对比两个历史快照。

## 数据包装器 (Wrappers)

位于 `SnapshotManager.Models` 命名空间。
*   `PrimitiveListElement<T>`
*   `DictionaryElement<K, V>`
*   `HashSetElement<T>`
*   `MatrixElement`
*   `ValueElement<T>`
*   `JsonElement<T>`

## 扩展方法 (Extensions)

位于 `SnapshotManager.Extensions` 命名空间。
*   `DiffAndPrint(...)`: 对比并打印到控制台。
*   `DiffWithAndPrint(...)`: 对比当前数据并打印。
*   `DiffAndFormat(...)`: 对比并返回字符串。
*   `DiffWithAndFormat(...)`: 对比当前数据并返回字符串。

## 输出与格式化 (Output & Formatting)

位于 `SnapshotManager.Output` 命名空间。

### 接口
*   `ISnapshotPrinter`: 快照打印接口。
*   `IDiffPrinter`: 差异打印接口。
*   `IDiffFormatter`: 差异格式化接口。

### 实现类
*   `ConsoleSnapshotPrinter`: 打印快照到控制台。
*   `StringSnapshotPrinter`: 打印快照到字符串。
*   `ConsoleDiffPrinter`: 打印差异到控制台（带颜色）。
*   `StringDiffFormatter`: 格式化差异为文本。
*   `GraphvizDiffFormatter`: 格式化差异为 Graphviz DOT 语法。
*   `MermaidDiffFormatter`: 格式化差异为 Mermaid 流程图语法。
