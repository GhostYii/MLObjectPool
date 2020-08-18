# MLObjectPool 
MLObjectPool is a dll project for Unity game engine authored by Ghostyii.

## How to add Unity reference in my local repository ?
You can get answer in this link: https://docs.unity3d.com/Manual/UsingDLL.html

## How to use MLObjectPool ?
You can use the singeton script 'ObjectPool' to create default pool or prefab pool.
```csharp
ObjectPool.Instance.CreatePrefabPool(string name, GameObject go, int size, bool autoExpand);
ObjectPool.Instance.CreatePool<T>(string name, int size, bool autoExpand);
```
The params 'autoExpand' means while the pool is fill, system will auto create more object (raw pool's size) for next allocation. **This means that the object pool will automatically expand to twice the size of the current object pool every time it fills up.**

General allocation and recycle method:
```csharp
ObjectPool.Instance.AllocationFromPool(string name);
ObjectPool.Instance.RecycleFromPool<T>(string name, T obj);
```

## Can I define my pool ?
Yes.  
You can create your pool by inherit class '**PoolBase**'.

## Interfaces
MLObjectPool support the '**IPoolObjectBeforeHandler**','**IPoolObjectHandler**','**IPoolObjectAfterHandler**'.  
- IPoolObjectBeforeHandler  
This interface's method will be called before the pool allocation/recycle.
- IPoolObjectHandler  
This interface's method will be called on object pool allocation/recycle.  
- IPoolObjectAfterHandler  
This interface's method will be called after the pool allocation/recycle.

### Call sequence:  
[Allocation Step]  
OnBeforeAllocation -> OnAllocation/DefaultAllocation -> OnAfterAllocation

[Recycle Step]  
OnBeforeRecycle -> OnRecycle/DefaultRecycle -> OnAfterRecycle

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

## PoolObject
PoolObject is a Mono script. It just implement the IPoolObjectHandler interface.