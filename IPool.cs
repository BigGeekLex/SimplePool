using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MSFD
{
    public interface IPool
    {
        /// <summary>
        /// If this object potentially should spawn on several devices in multiplayer mode use networkmode
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="spawnType"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="isActive"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        GameObject Spawn(GameObject prefab, SpawnType spawnType = SpawnType.local,
            Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), bool isActive = true, Transform parent = null);
        /// <summary>
        ///  If time less then 0, then object will be destroyed immediate if it is possible
        /// </summary>
        /// <param name="go"></param>
        /// <param name="time"></param>
        /// <param name="despawnType"></param>
        void Despawn(GameObject go, float time = 0, SpawnType spawnType = SpawnType.local, DespawnType despawnType = DespawnType.pool);
        bool TryCleanQueue(string poolQueueName);
        void CleanPool();
        void RequestPreSpawn(GameObject prefab, int count, SpawnType spawnType = SpawnType.local, PreSpawnAmountMode amountMode = PreSpawnAmountMode.totalAmount, float delayedUnscaledSpawnInterval = 0.1f);
        bool TryClampQueueSize(string poolQueueName, int maxCount);
        int GetQueueFullness(string poolQueueName);
        Transform GetQueueParent();

    }
}