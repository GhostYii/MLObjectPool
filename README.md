# MLObjectPool

MLObjectPool is a Unity Package Manager (UPM) object pool plugin for the Unity game engine.

## Requirements

- Unity 2022.3.20 or newer (`GameObject.InstantiateAsync` is used by prefab pool async APIs)

## Installation

The repository root is a UPM package (`com.ghostyii.mlobjectpool`).

### Local package

Copy or clone the repository into your `Packages` folder, or add a file reference
in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.ghostyii.mlobjectpool": "file:../MLObjectPool"
  }
}
```

### Git dependency

Add the repository as a Git dependency in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.ghostyii.mlobjectpool": "https://github.com/GhostYii/MLObjectPool.git#<branch-or-tag>"
  }
}
```

The package contains `Runtime` and `Editor` assemblies (`MLObjectPool.Runtime` /
`MLObjectPool.Editor`). Runtime code is available in any assembly; the editor
assembly is only compiled in the Unity Editor.

## Quick Start

Use the singleton `ObjectPoolManager` to create pools and allocate/recycle objects.

```csharp
var goPool = ObjectPoolManager.Instance.CreatePrefabPool("prefab", prefab, 100, true);
var dataPool = ObjectPoolManager.Instance.CreatePool<PoolableData>("data", 50, true);

GameObject go = goPool.Allocation();
PoolableData data = dataPool.Allocation();
```

`autoExpand` controls whether a pool automatically creates more objects after it runs out.
When expansion occurs, the pool adds `Math.Max(currentSize, 1)` objects at a time.

## Manager API

```csharp
PrefabPool CreatePrefabPool(string name, GameObject prefab, int size = 0, bool autoExpand = true);
Pool<T> CreatePool<T>(string name, int size = 0, bool autoExpand = true) where T : new();
Pool<T> CreatePool<T>(string name, int size, bool autoExpand, Func<T> factory) where T : new();

void AddPool(string name, PoolBase pool);
PoolBase FindPoolByName(string name);
bool TryGetPool<T>(string name, out Pool<T> pool) where T : new();

object AllocationFromPool(string name);
bool RecycleFromPool<T>(string name, T obj);

void RemovePool(string name);
void RemovePool(PoolBase pool);
```

`AllocationFromPool` requests an allocation and allows a one-time expansion for that call.
It does not permanently change the pool's `AutoExpand` setting.

Removing a `PrefabPool` destroys all GameObjects owned by that pool. Removing a
`Pool<T>` only clears the pool and releases its references.

## Pool<T>

`Pool<T>` is the general-purpose C# object pool. It is intended for plain C# objects,
not Unity `GameObject` instances. For prefabs, use `PrefabPool`.

### Create

```csharp
var pool = ObjectPoolManager.Instance.CreatePool<PoolableData>("data", 100, true);

// Custom factory
var pool2 = ObjectPoolManager.Instance.CreatePool<PoolableData>(
    "data2", 100, true, () => new PoolableData(42));
```

### Allocate

```csharp
PoolableData obj = pool.Allocation();
PoolableData[] objs = pool.Allocation(10);
bool success = pool.TryAllocation(out PoolableData item);
```

When `AutoExpand` is false and no object is available:

- `Allocation()` returns `default(T)`;
- `TryAllocation` returns false;
- `Allocation(int size)` returns null before allocating any objects.

### Recycle

```csharp
pool.Recycle(obj);
pool.Recycle(objs);
pool.RecycleAll();
```

## PrefabPool

`PrefabPool` manages Unity `GameObject` instances created from a prefab.

### Create

```csharp
var pool = ObjectPoolManager.Instance.CreatePrefabPool("prefab", prefab, 100, true);
```

### Allocate

```csharp
GameObject go = pool.Allocation();
GameObject[] gos = pool.Allocation(10);
bool success = pool.TryAllocation(out GameObject item);

pool.AllocationAsync(go => { });
pool.AllocationAsync(10, gos => { });
```

When `AutoExpand` is false and no object is available:

- sync APIs return null/false;
- async callbacks receive null.

### Recycle

```csharp
pool.Recycle(go);
pool.Recycle(gos);
pool.RecycleAll();
```

## Custom Pools

Inherit `PoolBase` and implement:

```csharp
public override int AvailableObjectCount { get; }
public override int ActiveObjectCount { get; }
public override object Allocation(bool isExpand);
public override bool Recycle(object obj, Type type);
public override bool RecycleAll();
```

Override `Clear()` when your custom pool owns disposable or scene resources.

## Interfaces

The following pool event interfaces are supported:

- `IBeforeAllocationHandler`
- `IAllocationHandler`
- `IAfterAllocationHandler`
- `IBeforeRecycleHandler`
- `IRecycleHandler`
- `IAfterRecycleHandler`

The old `IAllocationHanlder` name is kept for compatibility and is marked obsolete.

Call sequence:

```
[Allocation]
OnBeforeAllocation -> OnAllocation/DefaultAllocation -> OnAfterAllocation

[Recycle]
OnBeforeRecycle -> OnRecycle/DefaultRecycle -> OnAfterRecycle
```

If a prefab object implements the allocation/recycle handler, the default
`SetActive`/parent handling is not called automatically.

Default prefab handling:

```csharp
void OnGameObjectSpawn(GameObject obj)
{
    obj.transform.SetParent(null);
    obj.SetActive(true);
}

void OnGameObjectDespawn(GameObject obj)
{
    obj.transform.SetParent(poolRoot, false);
    obj.transform.localPosition = Vector3.zero;
    obj.transform.localRotation = Quaternion.identity;
    obj.transform.localScale = Vector3.one;
    obj.SetActive(false);
}
```

## Pool Event Trigger

`PoolEventTrigger` is a MonoBehaviour component that implements the pool event
interfaces and exposes UnityEvent entries in the Inspector, similar to uGUI's
`EventTrigger`.

Runtime events can also be added through `PrefabPoolObject`:

```csharp
var marker = go.GetComponent<PrefabPoolObject>();
marker.AddEvent(EventTriggerType.Allocation, pool => { });
marker.RemoveEvent(EventTriggerType.Allocation, callback);
```

Runtime events added through `PrefabPoolObject` are cleared when the object is
recycled.
