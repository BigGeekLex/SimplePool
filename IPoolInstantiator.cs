using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSFD
{
    /// <summary>
    /// Class отвечает за СОДАНИЕ и УДАЛЕНИЕ объекта (ВОЗВРАЩЕНИЕМ ОБЪЕКТА В ПУЛ ОН НЕ ЗАНИМАЕТСЯ!)
    /// </summary>
    public interface IPoolInstantiator
    {
        GameObject Create(GameObject prefab,
            Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), bool isActive = true, Transform parent = null);

        /// <summary>
        /// If time less then 0, then object will be destroyed immediate if it is possible
        /// </summary>
        /// <param name="go"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        void Destruct(GameObject go, float time = 0);

    }
}