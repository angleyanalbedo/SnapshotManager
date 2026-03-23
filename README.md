

# SnapshotManager

`SnapshotManager` 是一个通用快照与差异管理工具，用于保存对象状态并计算对象差异（Diff），支持复杂对象结构，如二维数组。

---

## 核心接口与类

* **IDiff<T>**
  对象实现自定义差异计算：

  ```csharp
  public interface IDiff<T>
  {
      DiffNode Diff(T oldValue, T newValue);
  }
  ```

* **ElementBase**
  所有自定义元素的通用基类：

  ```csharp
  public abstract class ElementBase : IDeepCloneable<ElementBase>
  {
      public abstract ElementBase DeepClone();
  }
  ```

* **ElementArraySnapshot**
  保存二维列表快照：

  ```csharp
  public class ElementArraySnapshot : Snapshot<List<List<ElementBase>>>
  {
      public ElementArraySnapshot(string name, string desc, List<List<ElementBase>> src)
          : base(name, desc, DeepClone2D(src)) { }
      private static List<List<ElementBase>> DeepClone2D(List<List<ElementBase>> src) => 
          src.Select(row => row.Select(e => e?.DeepClone()).ToList()).ToList();
  }
  ```

* **SnapshotManager<T>**
  管理快照并计算差异：

  ```csharp
  var manager = new SnapshotManager<List<List<ElementBase>>>();
  manager.AddSnapshot("s1", snapshot1);
  manager.AddSnapshot("s2", snapshot2);
  var diff = manager.CompareSnapshots("s1", "s2");
  ```

---

## 使用示例

```csharp
var elements = new List<List<ElementBase>> { new() { new MyElement { Value = 1 } } };
var snapshot1 = new ElementArraySnapshot("s1", "初始", elements);

// 修改后快照
elements[0][0] = new MyElement { Value = 10 };
var snapshot2 = new ElementArraySnapshot("s2", "修改后", elements);

// 管理与对比
var manager = new SnapshotManager<List<List<ElementBase>>>();
manager.AddSnapshot("s1", snapshot1);
manager.AddSnapshot("s2", snapshot2);
var diff = manager.CompareSnapshots("s1", "s2");
Console.WriteLine(diff);
```

---
