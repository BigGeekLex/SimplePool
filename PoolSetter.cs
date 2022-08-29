using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
namespace MSFD.PoolSystem
{
    public class PoolSetter : MonoBehaviour
    {
        [InlineEditor]
        [SerializeField]
        PoolSetterData poolData;

        public void Activate()
        {
            foreach(PoolSet x in poolData.GetPoolSets())
            {
                PC.RequestPreSpawn(x.prefab, x.count, x.spawnType, x.preSpawnAmountMode, x.spawnInterval);
            }
        }
        public void CleanPool()
        {
            PC.CleanPool();
        }
    }
}
