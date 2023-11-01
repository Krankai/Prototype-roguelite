using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShoot3DLogic : MonoBehaviour
{
    enum ComputationMode
    {
        None = 0,
        ComputeArcRadius = 10,
        ComputeInitialForce = 20,
    }


    [Header("Static Properties")]
    protected static float ShootingArcRadius = 0f;
    protected static float ShootingInitialForce = 0f;

    [SerializeField, MMReadOnly]
    private float _shootingArcRadius;
    [SerializeField, MMReadOnly]
    private float _shootingInitialForce;
}
