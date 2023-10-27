using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class DynamicForcePhysicObjectPooler : MMSimpleObjectPooler
{
    [Space, MMReadOnly]
    public float AppliedInitialForce = 0f;


    public override GameObject GetPooledGameObject()
    {
        var pooledGameObject = base.GetPooledGameObject();

        var physicsProjectile = pooledGameObject.MMGetComponentNoAlloc<PhysicsProjectile>();
        if (physicsProjectile == default)
        {
            return pooledGameObject;
        }

        var cachedInitialForce = physicsProjectile.InitialForce;
        physicsProjectile.InitialForce = AppliedInitialForce;

        var poolableObject = pooledGameObject.MMGetComponentNoAlloc<DynamicForcePhysicPoolableObject>();
        if (poolableObject != default)
        {
            poolableObject.OriginalInitialForce = cachedInitialForce;
        }

        return pooledGameObject;
    }
}
