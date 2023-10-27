using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicForcePhysicPoolableObject : MMPoolableObject
{
    [MMReadOnly]
    public float OriginalInitialForce = 0f;

    public override void Destroy()
    {
        base.Destroy();

        var physicsProjectile = gameObject.MMGetComponentNoAlloc<PhysicsProjectile>();
        if (physicsProjectile == default)
        {
            return;
        }

        physicsProjectile.InitialForce = OriginalInitialForce;
    }
}
