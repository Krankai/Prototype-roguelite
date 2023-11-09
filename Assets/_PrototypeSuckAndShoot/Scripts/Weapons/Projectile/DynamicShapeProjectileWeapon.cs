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
        //[MMInspectorGroup("Projectiles", true, 22)]
        //[Header("Suckable Projectile")]
        //// the holder to (temporarily) keep cached projectile's shape
        //[Tooltip("the holder to (temporarily) keep cached projectile's shape")]
        //public Transform CachedProjectileHolder;

        [SerializeField]
        List<SuckableProjectile> _listCachedProjectiles = new();


        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            var baseProjectile = base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);

            var dynamicShapeProjectile = baseProjectile.MMGetComponentAroundOrAdd<DynamicShapeProjectile>();
            if (dynamicShapeProjectile.ModelAttachement != default)
            {
                if (_listCachedProjectiles.Count > 0)
                {
                    baseProjectile.SetActive(false);

                    var cachedProjectile = _listCachedProjectiles[0];
                    _listCachedProjectiles.RemoveAt(0);

                    var matchedObject = dynamicShapeProjectile.ModelAttachement.MMFindDeepChildBreadthFirst(cachedProjectile.ID);
                    if (matchedObject != default)
                    {
                        // Re-use
                        matchedObject.transform.SetLocalPositionAndRotation(cachedProjectile.Offset, Quaternion.Euler(cachedProjectile.Rotation));
                        matchedObject.transform.localScale = cachedProjectile.Scale;

                        matchedObject.gameObject.SetActive(true);

                        Destroy(cachedProjectile.Model);
                    }
                    else
                    {
                        // Instantiate new
                        var projectileModel = Instantiate(cachedProjectile.Model, dynamicShapeProjectile.ModelAttachement);
                        projectileModel.SetActive(false);

                        projectileModel.transform.SetLocalPositionAndRotation(cachedProjectile.Offset, Quaternion.Euler(cachedProjectile.Rotation));
                        projectileModel.transform.localScale = cachedProjectile.Scale;
                        projectileModel.name = cachedProjectile.ID;

                        projectileModel.SetActive(true);
                    }
                }
                else
                {
                    dynamicShapeProjectile.EnableDefaultShape();
                }
            }

            if (!baseProjectile.activeSelf)
            {
                baseProjectile.SetActive(true);
            }

            return baseProjectile;
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
}
