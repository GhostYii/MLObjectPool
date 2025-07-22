# MLObjectPool

MLObjectPool is a dll project for Unity game engine authored by Ghostyii.

## How to add Unity reference in my local repository ?

You can get answer in this link: https://docs.unity3d.com/Manual/UsingDLL.html

## How to use MLObjectPool ?

You can use the singeton script 'ObjectPoolManager' to create default pool or prefab pool.

```csharp
ObjectPoolManager.Instance.CreatePrefabPool(string name, GameObject go, int size, bool autoExpand);
ObjectPoolManager.Instance.CreatePool<T>(string name, int size, bool autoExpand);
```

The params 'autoExpand' means while the pool is fill, system will auto create more object (raw pool's size) for next allocation. **This means that the object pool will automatically expand to twice the size of the current object pool every time it fills up.**

General allocation and recycle method:

```csharp
ObjectPoolManager.Instance.AllocationFromPool(string name);
ObjectPoolManager.Instance.RecycleFromPool<T>(string name, T obj);
```

## Pool\<T>

Pool\<T> is the general pool in MLObjectPool.  

### Create

```csharp
//create by pool manager
var goPool = ObjectPoolManager.Instance.CreatePool<GameObject>("gameObject", 100, true);

//create by constructor (before v1.0.5)
var goPool = new Pool<GameObject>(100, true);
ObjectPoolManager.Instance.AddPool("gameObject", goPool);
```

### Allocation

```csharp
public override object Allocation(bool isExpand);
public T Allocation();
public T[] Allocation(int size);
```

### Recycle

```csharp
public override bool Recycle(object obj, Type type);
public bool Recycle(T obj);
public bool Recycle(T[] objs);
```

## PrefabPool

### Create

```csharp
//create by pool manager
var prefabPool = ObjectPoolManager.Instance.CreatePrefabPool("prefab", prefab, 100, true);

//create by constructor (before v1.0.5)
var prefabPool = new PrefabPool(prefab, 100, true);
ObjectPoolManager.Instance.AddPool("prefab", prefabPool);
```

### Allocation

```csharp
public GameObject Allocation();
public GameObject[] Allocation(int size);
public override object Allocation(bool isExpand);

// async api
public void AllocationAsync(Action<GameObject> callback);
public void AllocationAsync(int size, Action<GameObject[]> callback);
```

**only sync version checkout 'sync' branch.**

### Recycle

```csharp
public bool Recycle(GameObject obj);
public override bool Recycle(object obj, Type type);
public bool Recycle(GameObject[] objs);
```

## Can I define my pool ?

Yes.  
You can create your pool by inherit class '**PoolBase**'.

## Interfaces

MLObjectPool support the '**IAllocationHandler**','**IRecycleHandler**','**IBeforeAllocationHandler**','**IBeforeRecycleHandler**','**IAfterAllocationHandler**','**IAfterRecycleHandler**'.  

- IAllocationHandler  
  This interface's method will be called on object pool allocation.
- IRecycleHandler  
  This interface's method will be called on object pool recycle. 
- IBeforeAllocationHandler  
  This interface's method will be called before the pool allocation.
- IBeforeRecycleHandler  
  This interface's method will be called before the pool recycle.
- IAfterAllocationHandler  
  This interface's method will be called after the pool allocation.
- IAfterRecycleHandler  
  This interface's method will be called after the pool recycle.

### Call sequence:

[Allocation Step]  
OnBeforeAllocation -> OnAllocation/DefaultAllocation -> OnAfterAllocation

[Recycle Step]  
OnBeforeRecycle -> OnRecycle/DefaultRecycle -> OnAfterRecycle

[**NOTITION**]  
**If pool object implement the 'IAllocationHandler' or 'IRecycleHandler', the default allocation/recycle method will not called.**

### Default Allocation/Recycle

If pool object dont implement IPoolObjectHandler, it will invoke the method as follow(take PrefabPool as an example):

```csharp
void OnGameObjectSpawn(GameObject obj)
{
    obj.transform.SetParent(null);
    obj.SetActive(true);
}

void OnGameObjectDespawn(GameObject obj)
{
    obj.transform.parent = poolRoot;
    obj.transform.position = Vector3.zero;
    obj.transform.rotation = Quaternion.identity;
    obj.transform.localScale = Vector3.one;
    obj.SetActive(false);
}
```

You can implement interface in any script (just like IPointEnterHandler).  
**Interface only available in Pool\<T> or PrefabPool.**

If you have your custom pool, you may consider to support those interfaces.

## Pool Event Trigger

PoolEventTrigger is a Mono script. It just implement all interfaces in MLObjectPool.  
Useage just like the EventTrigger in uGUI.