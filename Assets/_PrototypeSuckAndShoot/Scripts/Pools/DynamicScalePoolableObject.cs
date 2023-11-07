using MoreMountains.Tools;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class DynamicScalePoolableObject : MMPoolableObject
    {
        [MMReadOnly]
        public Vector3 CachedScale = Vector3.one;

        public override void Destroy()
        {
            base.Destroy();
            transform.localScale = CachedScale;
        }
    }
}
