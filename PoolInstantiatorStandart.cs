using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSFD
{
    public class PoolInstantiatorStandart : MonoBehaviour, IPoolInstantiator
    {
        public GameObject Create(GameObject prefab, Vector3 position = default, Quaternion rotation = default, bool isActive = true, Transform parent = null)
        {
            GameObject go;
            if (parent == null)
                go = Instantiate(prefab, position, rotation);
            else
                go = Instantiate(prefab, position, rotation, parent);

            go.SetActive(isActive);
            return go;
        }
        public void Destruct(GameObject go, float time = 0)
        {
            if (time < 0)
                DestroyImmediate(go);
            else
                Destroy(go, time);
        }
    }
}