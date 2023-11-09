using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class DynamicShapeProjectile : Projectile
    {
        [Header("Model")]
        // the transform to which projectile's model will be attached
        [Tooltip("the transform to which projectile's model will be attached")]
        public Transform ModelAttachement;

        // the default mesh renderer
        [Tooltip("the default mesh renderer")]
        [SerializeField, MMReadOnly]
        protected MeshRenderer _defaultMeshRenderer;

        // whether to destroy model after done using, or keep it there for pooling
        [Tooltip("whether to destroy model after done using, or keep it there for pooling")]
        public bool IsDestroyed;


        protected override void Initialization()
        {
            base.Initialization();

            if (_defaultMeshRenderer == default)
            {
                _defaultMeshRenderer = gameObject.MMGetComponentNoAlloc<MeshRenderer>();
            }

            _defaultMeshRenderer.enabled = false;
        }

        public override void Destroy()
        {
            base.Destroy();

            if (IsDestroyed)
            {
                ModelAttachement.MMDestroyAllChildren();
            }
            else
            {
                for (int i = 0, count = ModelAttachement.childCount; i < count; ++i)
                {
                    var child = ModelAttachement.GetChild(i);
                    if (child == default)
                    {
                        continue;
                    }

                    child.gameObject.SetActive(false);
                }
            }
        }

        public virtual void EnableDefaultShape()
        {
            _defaultMeshRenderer.enabled = true;
        }
    }
}
