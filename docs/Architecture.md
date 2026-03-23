# 架构设计 (Architecture)

SnapshotManager 采用分层架构，主要由以下几个部分组成：

## 1. Abstractions (抽象层)
定义了核心接口，解耦了实现。
*   `ISnapshotManager`: 管理器的标准行为。
*   `IDiff<T>`: 差异比较算法的标准接口。
*   `IDeepCloneable<T>`: 深拷贝接口。
*   `IDiffPrinter` / `IDiffFormatter`: 输出接口。

## 2. Models (模型层)
定义了数据结构。
*   `ElementBase`: 所有受管数据的基类。
*   `Snapshot<T>`: 承载数据副本和元数据。
*   `DiffNode`: 树状结构的差异结果。

## 3. Core (核心层)
*   `SnapshotManager<TSnapshot, TModel>`: 负责协调快照的创建、存储和对比。
*   `Diff` 实现类: `ListDiff`, `DictionaryDiff`, `MatrixDiff`, `ElementDiff` (反射) 等。

## 4. Extensions (扩展层)
提供便捷的扩展方法，如 `DiffAndPrint`，简化用户调用。

## 设计理念
*   **不可变性**: 快照一旦创建，其内部数据不应被修改（通过 DeepClone 保证）。
*   **策略模式**: Diff 算法可插拔，用户可以注入自定义的 `IDiff` 实现。
*   **组合优于继承**: 通过 `Element` 包装器组合原始数据，而不是强制原始数据继承特定基类（虽然 ElementBase 是基类，但通常作为 Wrapper 使用）。
