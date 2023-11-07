using MoreMountains.Tools;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class DynamicScaleObjectPooler : MMSimpleObjectPooler
    {
        [MMReadOnly]
        public Vector3 DynamicScale = Vector3.one;

        protected Vector3 _cachedScale;


        protected override void OnEnable()
        {
            base.OnEnable();

            _cachedScale = GameObjectToPool.transform.localScale;
        }

        public override GameObject GetPooledGameObject()
        {
            var basePooledObject = base.GetPooledGameObject();

            //basePooledObject.transform.localScale = DynamicScale;

            var poolableObject = basePooledObject.MMGetComponentNoAlloc<DynamicScalePoolableObject>();
            if (poolableObject != default)
            {
                poolableObject.CachedScale = _cachedScale;

                poolableObject.ExecuteOnEnable.RemoveAllListeners();
                poolableObject.ExecuteOnEnable.AddListener(() =>
                {
                    poolableObject.transform.localScale = DynamicScale;
                });
            }

            return basePooledObject;
        }
    }
}
