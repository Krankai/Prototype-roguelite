using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class DynamicShapeProjectileWeapon : ProjectileWeapon
    {
        [MMInspectorGroup("Projectiles", true, 22)]
        [Header("Sucked Holder")]
        // the holder transform that sucked objects will be attached to
        [Tooltip("the holder transform that sucked objects will be attached to")]
        public List<Transform> HolderTransforms = new();

        // the position type the sucked object is attached into the transform holder
        [Tooltip("the position type the sucked object is attached into the transform holder")]
        public SuckedObjectAttachPosition AttachPositionType = SuckedObjectAttachPosition.Center;

        [Space, SerializeField, ReadOnly]
        internal List<SuckableProjectile> _listCachedProjectiles = new();

        protected int _indexHolderForSuckedObject = 0;
        protected Vector3 _localAttachPosition = Vector3.zero;


        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            var baseProjectile = base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);

            var dynamicShapeProjectile = baseProjectile.MMGetComponentAroundOrAdd<DynamicShapeProjectile>();
            if (dynamicShapeProjectile.ModelAttachement != default)
            {
                if (_listCachedProjectiles.Count > 0)
                {
                    //baseProjectile.SetActive(false);

                    var cachedProjectile = _listCachedProjectiles[0];
                    _listCachedProjectiles.RemoveAt(0);

                    var currentWorldPosition = cachedProjectile.Model.transform.position;

                    cachedProjectile.Model.transform.SetParent(dynamicShapeProjectile.ModelAttachement);
                    cachedProjectile.Model.transform.localPosition = cachedProjectile.Offset;
                    cachedProjectile.Model.transform.position = currentWorldPosition;
                    cachedProjectile.Model.gameObject.SetActive(true);

                    //var matchedObject = dynamicShapeProjectile.ModelAttachement.MMFindDeepChildBreadthFirst(cachedProjectile.ID);
                    //if (matchedObject != default)
                    //{
                    //    // Re-use
                    //    matchedObject.transform.SetLocalPositionAndRotation(cachedProjectile.Offset, Quaternion.Euler(cachedProjectile.Rotation));
                    //    matchedObject.transform.localScale = cachedProjectile.Scale;

                    //    SetBaseHealth(matchedObject.gameObject, dynamicShapeProjectile.BaseHealth);
                    //    matchedObject.gameObject.SetActive(true);
                    //}
                    //else
                    //{
                    //    // Instantiate new
                    //    var projectileModel = Instantiate(cachedProjectile.Model, dynamicShapeProjectile.ModelAttachement);
                    //    projectileModel.SetActive(false);

                    //    projectileModel.transform.SetLocalPositionAndRotation(cachedProjectile.Offset, Quaternion.Euler(cachedProjectile.Rotation));
                    //    projectileModel.transform.localScale = cachedProjectile.Scale;
                    //    projectileModel.name = cachedProjectile.ID;

                    //    SetBaseHealth(projectileModel, dynamicShapeProjectile.BaseHealth);
                    //    projectileModel.SetActive(true);
                    //}
                }
                else
                {
                    dynamicShapeProjectile.EnableDefaultShape();
                }
            }

            return baseProjectile;
        }

        protected virtual void SetBaseHealth(GameObject projectileObject, Health baseHealth)
        {
            var modelHeatlh = projectileObject.MMGetComponentNoAlloc<Health>();
            if (modelHeatlh == default)
            {
                modelHeatlh = projectileObject.GetComponentInChildren<Health>();
            }

            modelHeatlh.MasterHealth = baseHealth;
        }

        public virtual void CacheProjectile(CharacterSuckable suckable)
        {
            if (suckable.SuckableAsProjectilePrefab == default)
            {
                return;
            }

            _listCachedProjectiles.Add(new SuckableProjectile
            {
                Model = suckable.SuckableAsProjectilePrefab,
                ID = suckable.SuckableAsProjectileID,
                Offset = suckable.OffsetSuckableAsProjectile,
                Rotation = suckable.RotationSuckableAsProjectile,
                Scale = suckable.ScaleSuckableAsProjectile,
            });
        }

        public virtual void SaveSuckedProjectile(CharacterSuckable suckable)
        {
            if (suckable.SuckableAsProjectilePrefab == default)
            {
                return;
            }

            var holder = (HolderTransforms.Count > 0) ? HolderTransforms[_indexHolderForSuckedObject++ % HolderTransforms.Count] : default;
            if (holder == default)
            {
                holder = transform;
            }

            var projectileObject = Instantiate(suckable.SuckableAsProjectilePrefab);
            projectileObject.transform.SetParent(holder);
            projectileObject.transform.localScale = suckable.ScaleSuckableAsProjectile;
            projectileObject.name = suckable.SuckableAsProjectileID;

            if (AttachPositionType == SuckedObjectAttachPosition.BackZ)
            {
                var modelRenderer = projectileObject.GetComponentInChildren<Renderer>();
                if (modelRenderer != default)
                {
                    _localAttachPosition = suckable.OffsetSuckableAsProjectile;
                    _localAttachPosition.z += modelRenderer.bounds.extents.z;
                }
            }
            else // if (AttachPositionType == SuckedObjectAttachPosition.Center)
            {
                _localAttachPosition = suckable.OffsetSuckableAsProjectile;
            }

            var rotation = Quaternion.Euler(suckable.RotationSuckableAsProjectile);
            projectileObject.transform.SetLocalPositionAndRotation(_localAttachPosition, rotation);

            if (!suckable.IsShownOnSucked)
            {
                projectileObject.SetActive(false);
            }

            _listCachedProjectiles.Add(new SuckableProjectile
            {
                Model = projectileObject,
                ID = suckable.SuckableAsProjectileID,
                Offset = suckable.OffsetSuckableAsProjectile,
                Rotation = suckable.RotationSuckableAsProjectile,
                Scale = suckable.ScaleSuckableAsProjectile,
            });
        }
    }

    [System.Serializable]
    struct SuckableProjectile
    {
        public GameObject Model;
        public string ID;
        public Vector3 Offset;
        public Vector3 Rotation;
        public Vector3 Scale;
    }

    [System.Serializable]
    public enum SuckedObjectAttachPosition
    {
        Center = 0,
        BackZ = 1,
    }
}
