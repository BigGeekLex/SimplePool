using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSFD;
using Sirenix.OdinInspector;
/// <summary>
/// Pool Core
/// </summary>
public class PC : SingletoneBase<PC>
{
    IPool pool;
    protected override void AwakeInitialization()
    {
        pool = GetComponent<IPool>();
        if(pool == null)
        {
            Debug.LogError("There is no IPool in PC");
            Debug.LogError("Autocreation of PoolStandart...");
            pool = gameObject.AddComponent<PoolStandart>();
        }
    }
    public static GameObject Spawn(GameObject prefab, Vector3 position = default, Quaternion rotation = default, bool isActive = true, Transform parent = null)
    {
        return Spawn(prefab, SpawnType.local, position, rotation, isActive, parent);
    }
    [Button]
    public static GameObject Spawn(GameObject prefab, SpawnType spawnType,
            Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), bool isActive = true, Transform parent = null)
    {
        return Instance.GetPool().Spawn(prefab, spawnType, position, rotation, isActive, parent);
    }
    [Button]
    public static void Despawn(GameObject go, float time = 0, SpawnType spawnType = SpawnType.local, DespawnType despawnType = DespawnType.pool)
    {
        Instance.GetPool().Despawn(go, time, spawnType, despawnType);
    }
    [Button]
    public static void CleanQueue(string poolQueueName)
    {
        Instance.GetPool().TryCleanQueue(poolQueueName);
    }
    [Button]
    public static void RequestPreSpawn(GameObject prefab, int count, SpawnType spawnType = SpawnType.local, PreSpawnAmountMode amountMode = PreSpawnAmountMode.totalAmount, float delayedUnscaledSpawnInterval = 0.05f)
    {
        Instance.GetPool().RequestPreSpawn(prefab, count, spawnType, amountMode, delayedUnscaledSpawnInterval);
    }

    [Button]
    public static int GetQueueFullness(string poolQueueName)
    {
        return Instance.GetPool().GetQueueFullness(poolQueueName);
    }
    [Button]
    public static void ClampQueueSize(string poolQueueName, int maxCount)
    {
        Instance.GetPool().TryClampQueueSize(poolQueueName, maxCount);
    }
    [Button]
    public static void CleanPool()
    {
        Instance.GetPool().CleanPool();
    }

    IPool GetPool()
    {
        return pool;
    }
}
/// <summary>
/// If this object potentially should spawn on several devices in multyplayer use networkmode
/// </summary>
public enum SpawnType { local, network}
public enum DespawnType { pool, destroy}
/// <summary>
/// Clamp - control max count of objects in pool in moment. It can destroy extra objects
/// Additive - simple add objects in pool
/// Total - add count of objects to target amount
/// </summary>
public enum PreSpawnAmountMode { totalAmount, addAmount, clampMaxCount}
public class Tags
{
    List<string> tags;

    public Tags(params string[] tags)
    {
        this.tags = new List<string>(tags);
    }
}