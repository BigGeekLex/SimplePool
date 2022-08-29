using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSFD.PoolSystem
{
    [CreateAssetMenu(fileName = "PoolSetterData_", menuName = "MSFD/PoolSetterData")]
    public class PoolSetterData : ScriptableObject
    {
        [SerializeField]
        List<PoolSet> poolSets = new List<PoolSet>();

        public List<PoolSet> GetPoolSets()
        {
            return poolSets;
        }
    }
}