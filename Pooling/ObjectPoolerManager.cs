using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolerManager : MonoBehaviour
{
    public static MonoBehaviourPool<PedestrianEntity> PedestrianPool;
    public static MonoBehaviourPool<VehicleEntity> VehiclePool;
    public static ObjectPoolerManager Instance;

    private void Start()
    {
        PedestrianPool = new MonoBehaviourPool<PedestrianEntity>("Pedestrian Pool", transform, true);
        VehiclePool = new MonoBehaviourPool<VehicleEntity>("Vehicle Pool", transform, true);
        Instance = this;
    }

    public static void ClearPools()
    {
        PedestrianPool.Clear(true);
        VehiclePool.Clear(true);
    }
}
