using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace MSFD
{
    public class PoolStandart : MonoBehaviour, IPool
    {
        [ValidateInput("HasIPoolCreateDestruct", "GO must has IPoolCreateDestruct")]
        [SerializeField]
        GameObject standartInstantiatorGO;
        [ValidateInput("HasIPoolCreateDestruct", "GO must has IPoolCreateDestruct")]
        [SerializeField]
        GameObject networkInstantiatorGO;

        [SerializeField]
        SetNullParentMode setNullParentMode = SetNullParentMode.setToQueueParent;

        [Header("When true => destroy all objects, GOes cash is disabled")]
        [SerializeField]
        bool isPoolDisabled = false;

        IPoolInstantiator standartInstantiator;
        IPoolInstantiator networkInstantiator;
        Dictionary<string, PoolQueue> goTable = new Dictionary<string, PoolQueue>();

        Transform poolQueueParent;
        Transform spawnedObjectsParent;

        void Awake()
        {
            poolQueueParent = new GameObject("Pool_Queue").transform;
            poolQueueParent.SetParent(transform);
            spawnedObjectsParent = new GameObject("Pool_Spawned").transform;
            spawnedObjectsParent.SetParent(transform);

            standartInstantiator = standartInstantiatorGO?.GetComponent<IPoolInstantiator>();
            if (standartInstantiator == null)
            {
                Debug.LogError("There is no standartInstantiator" + " in current scene. Add " + typeof(IPoolInstantiator) + " to current scene to get correct access");
                Debug.LogError("Autocreation of " + typeof(PoolInstantiatorStandart) + "...");
                standartInstantiator = gameObject.AddComponent<PoolInstantiatorStandart>();
            }
            networkInstantiator = networkInstantiatorGO?.GetComponent<IPoolInstantiator>();
            if (networkInstantiator == null)
            {
                Debug.LogError("There is no networkInstantiator" + " in current scene. Add " + typeof(IPoolInstantiator) + " to current scene to get correct access");
                Debug.LogError("Autocreation of " + typeof(PoolInstantiatorStandart) + "...");
                networkInstantiator = gameObject.AddComponent<PoolInstantiatorStandart>();
            }
        }
        bool HasIPoolCreateDestruct(GameObject go)
        {
            return go?.GetComponent<IPoolInstantiator>() != null;
        }
        public GameObject Spawn(GameObject prefab, SpawnType spawnType, Vector3 position = default, Quaternion rotation = default, bool isActive = true, Transform parent = null)
        {
            GameObject go;
            if (TryFindExistingGO(prefab, out go))
            {
#if UNITY_EDITOR
                PoolQueue poolQueue = goTable[prefab.name];
                if (spawnType != poolQueue.spawnType)
                {
                    Debug.LogError("Pool Queue spawn type is not match with requested spawn type: " + spawnType + " != " + poolQueue.spawnType);
                }
#endif
                go.transform.SetParent(parent);
#if UNITY_EDITOR
                SetParent(go, parent);
#endif
                go.transform.position = position;
                go.transform.rotation = rotation;
                go.SetActive(isActive);
                return go;
            }
            else
            {
                go = GetPoolInstantiator(spawnType).Create(prefab, position, rotation, isActive, parent);
#if UNITY_EDITOR
                SetParent(go, parent);
#endif
                return go;
            }
        }

        private bool TryFindExistingGO(GameObject prefab, out GameObject go)
        {
            go = null;
            PoolQueue poolQueue;
            if (goTable.TryGetValue(GetCorrectGoName(prefab), out poolQueue))
            {
                if (poolQueue.Count > 0)
                {
                    go = poolQueue.Dequeue();
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public void Despawn(GameObject go, float time = 0, SpawnType spawnType = SpawnType.local, DespawnType despawnType = DespawnType.pool)
        {
            if (despawnType == DespawnType.destroy || isPoolDisabled)
            {
                GetPoolInstantiator(spawnType).Destruct(go, time);
                return;
            }
            else
            {
                DespawnProcess(go, time, spawnType);
            }
        }
        void DespawnProcess(GameObject go, float time, SpawnType spawnType)
        {
            StartCoroutine(DespawnProcessCoroutine(go, time, spawnType));
        }
        IEnumerator DespawnProcessCoroutine(GameObject go, float time, SpawnType spawnType)
        {
            if (time <= 0)
            {
                AddGOInQueue(go, spawnType);
            }
            else
            {
                yield return new WaitForSeconds(time);
                AddGOInQueue(go, spawnType);
            }
        }
        private void AddGOInQueue(GameObject go, SpawnType spawnType)
        {
            PoolQueue poolQueue;
            if (!goTable.TryGetValue(GetCorrectGoName(go), out poolQueue))
            {
                poolQueue = new PoolQueue(this, GetCorrectGoName(go), spawnType);
                goTable.Add(poolQueue.name, poolQueue);
            }
#if UNITY_EDITOR
            //Check for destoying already destroyed object
            if (poolQueue.Contains(go))
            {
                Debug.LogError("Attempt to despawn already despawned object!");
                return;
            }
#endif
            go.SetActive(false);
            poolQueue.Enqueue(go);
            go.transform.SetParent(poolQueue.parent);
        }
        IPoolInstantiator GetPoolInstantiator(SpawnType spawnType)
        {
            if (spawnType == SpawnType.local)
                return standartInstantiator;
            else
                return networkInstantiator;
        }
        string GetCorrectGoName(GameObject go)
        {
            return go.name.Replace("(Clone)", "");
        }

        public Transform GetQueueParent()
        {
            return poolQueueParent;
        }

        public bool TryCleanQueue(string poolQueueName)
        {
            PoolQueue poolQueue;
            if (goTable.TryGetValue(poolQueueName, out poolQueue))
            {
                CleanQueue(poolQueue);
                return true;
            }
            else
                return false;
        }
        void CleanQueue(PoolQueue poolQueue)
        {
            while (poolQueue.Count > 0)
            {
                Despawn(poolQueue.Dequeue(), 0, poolQueue.spawnType, DespawnType.destroy);
            }
            //Destroy(poolQueue.parent.gameObject);
        }
        public void CleanPool()
        {
            foreach (var x in goTable)
            {
                CleanQueue(x.Value);
            }
            foreach (Transform x in spawnedObjectsParent)
            {
                Destroy(x.gameObject);
            }
            StopAllCoroutines();
        }

        public void RequestPreSpawn(GameObject prefab, int count, SpawnType spawnType = SpawnType.local,
            PreSpawnAmountMode amountMode = PreSpawnAmountMode.totalAmount, float delayedUnscaledSpawnInterval = 0.1f)
        {
            int targetCount = count;
            if (amountMode == PreSpawnAmountMode.clampMaxCount)
            {
                targetCount = count - GetGoAmountInQueue(prefab.name);
                if (targetCount <= 0)
                {
                    TryClampQueueSize(prefab.name, count);
                    return;
                }
            }
            else if (amountMode == PreSpawnAmountMode.totalAmount)
            {
                targetCount = count - GetGoAmountInQueue(prefab.name);
                if (targetCount <= 0)
                {
                    return;
                }
            }

            if (delayedUnscaledSpawnInterval <= 0)
            {
                for (int i = 0; i < targetCount; i++)
                {
                    Despawn(GetPoolInstantiator(spawnType).Create(prefab));
                }
            }
            else
            {
                StartCoroutine(PreSpawnCoroutine(prefab, targetCount, spawnType, amountMode, delayedUnscaledSpawnInterval));
            }
        }
        IEnumerator PreSpawnCoroutine(GameObject prefab, int count, SpawnType spawnType, PreSpawnAmountMode amountMode, float delayedUnscaledSpawnInterval)
        {
            for (int i = 0; i < count; i++)
            {
                Despawn(GetPoolInstantiator(spawnType).Create(prefab));
                yield return new WaitForSecondsRealtime(delayedUnscaledSpawnInterval);
            }
        }
        int GetGoAmountInQueue(string poolQueueName)
        {
            PoolQueue poolQueue;
            if (goTable.TryGetValue(poolQueueName, out poolQueue))
            {
                return poolQueue.Count;
            }
            return 0;
        }
        public int GetQueueFullness(string poolQueueName)
        {
            return GetGoAmountInQueue(poolQueueName);
        }

        public bool TryClampQueueSize(string poolQueueName, int maxCount)
        {
            PoolQueue poolQueue;
            if (goTable.TryGetValue(poolQueueName, out poolQueue))
            {
                int despawnCount = poolQueue.Count - maxCount;
                if (despawnCount < 0)
                    return false;
                for (int i = 0; i < despawnCount; i++)
                {
                    GetPoolInstantiator(poolQueue.spawnType).Destruct(poolQueue.Dequeue(), 0);
                }
                return true;
            }
            else
                return false;
        }
#if UNITY_EDITOR
        [Obsolete("Only for Editor")]
        /// <summary>
        /// Method only for Editor!!!
        /// </summary>
        /// <param name="go"></param>
        /// <param name="poolQueue"></param>
        /// <param name="targetParent"></param>
        void SetParent(GameObject go, Transform targetParent)
        {
            if (targetParent != null || setNullParentMode == SetNullParentMode.setToRoot)
            {
                go.transform.SetParent(targetParent);
            }
            else
            {
                go.transform.SetParent(spawnedObjectsParent);
            }
        }
#endif
        public enum SetNullParentMode { setToQueueParent, setToRoot }
    }
    public class PoolQueue
    {
        public string name;
        Queue<GameObject> queue = new Queue<GameObject>();
        public Transform parent;
        public SpawnType spawnType;
        public PoolQueue(IPool pool, string _name, SpawnType _spawnType)
        {
            name = _name;
            spawnType = _spawnType;
            parent = new GameObject(name).transform;
            parent.SetParent(pool.GetQueueParent());
        }
        public void Enqueue(GameObject go)
        {
            queue.Enqueue(go);
#if UNITY_EDITOR
            UpdateCountName();
#endif
        }
        public GameObject Dequeue()
        {
            GameObject go = queue.Dequeue();
#if UNITY_EDITOR
            UpdateCountName();
#endif 
            return go;
        }
        public int Count
        {
            get
            {
                return queue.Count;
            }
        }
        public bool Contains(GameObject go)
        {
            return queue.Contains(go);
        }
#if UNITY_EDITOR
        void UpdateCountName()
        {
            parent.name = name + " [" + Count + "]";
        }
#endif
    }
}