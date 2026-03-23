

# SnapshotManager

`SnapshotManager` 是一个通用快照与差异管理工具，用于保存对象状态并计算对象差异（Diff），支持复杂对象结构（如二维数组）的树状对比。

---

## 核心接口与类

* **IDiff<T>**
  核心比较接口，返回树状结构的 `DiffNode`：

  ```csharp
  public interface IDiff<T>
  {
      DiffNode Diff(T oldValue, T newValue);
  }
  ```

* **DiffNode**
  表示差异的树节点，包含 `Children` 列表，能够完整描述层级变化（如：矩阵 -> 行 -> 列 -> 属性）。

* **SnapshotManager<T>**
  管理快照历史并执行 Diff：

  ```csharp
  // 使用工厂创建针对二维 Element 的管理器
  var manager = ElementSnapshotManagerFactory.Create();
  
  manager.AddSnapshot("s1", snapshot1);
  manager.AddSnapshot("s2", snapshot2);
  
  // 获取树状差异
  DiffNode diffTree = manager.Diff("s1", "s2");
  ```

---

## 使用示例

```csharp
// 1. 准备数据
var elements = new List<List<ElementBase>> { new() { new MyElement { Value = 1 } } };
var snapshot1 = new ElementArraySnapshot("s1", "初始", elements);

// 2. 修改数据并创建新快照
// 注意：实际使用中通常需要深拷贝或重新生成数据
var elements2 = new List<List<ElementBase>> { new() { new MyElement { Value = 10 } } };
var snapshot2 = new ElementArraySnapshot("s2", "修改后", elements2);

// 3. 初始化管理器 (推荐使用 Factory 处理复杂类型)
var manager = ElementSnapshotManagerFactory.Create();
manager.AddSnapshot(snapshot1);
manager.AddSnapshot(snapshot2);

// 4. 计算差异
DiffNode rootNode = manager.Diff("s1", "s2");

// 5. 检查结果 (伪代码)
// rootNode.Children[0] (Row[0])
//   -> Children[0] (Col[0])
//     -> Children[0] (Value: 1 -> 10)
if (rootNode.HasDifference)
{
    Console.WriteLine("Found changes!");
    // 可以使用递归打印函数展示整棵树
}
```

---
