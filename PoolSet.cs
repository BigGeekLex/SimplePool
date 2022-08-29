using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
namespace MSFD.PoolSystem
{
    [System.Serializable]
    public class PoolSet
    {
        public GameObject prefab;
        public int count;
        public float spawnInterval = 0.05f;
        public PreSpawnAmountMode preSpawnAmountMode;
        public SpawnType spawnType = SpawnType.local;
    }

}