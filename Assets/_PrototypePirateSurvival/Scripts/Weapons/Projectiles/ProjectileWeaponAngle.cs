using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileWeaponAngle : ProjectileWeapon
{
    #region Data Structures
    [System.Serializable]
    public struct RangeShootAngle
    {
        public Vector3 ShootAngle;
    }

    [System.Serializable]
    public struct ProjectileShootAngleRange
    {
        [Tooltip("in degrees")]
        public Vector3 FromAngle;
        [Tooltip("in degrees")]
        public Vector3 ToAngle;
    }
    #endregion Data Structures

    [MMInspectorGroup("Projectiles", true, 22)]
    [Header("Main")]
    // the range of shooting angle (in degrees) to apply for each projectile spawn
    [Tooltip("the range of shooting angle (in degrees) to apply for each projectile spawn")]
    public Vector3 ShootAngle = Vector3.zero;

    [Header("Sub-Projectiles")]
    [Tooltip("offsets relative to main projectile spawn position"), FormerlySerializedAs("ListStartOffsets")]
    public List<Vector3> ListSubSpawnOffsets;
    public List<RangeShootAngle> ListShootAngles;


    [Header("Multi Projectiles")]
    [MMInformation("\nThis setting will override Projectile Spawn Offset\n", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
    public bool IsAppliedMultiProjectile = false;

    public List<Vector3> ListProjectileSpawnOffsets;
    public List<ProjectileShootAngleRange> ListProjectileShootAngleRanges;



    public virtual void AdjustProjectilesAngle(float originAngle, float arcHeightAngle, float shootingRangeAngle)
    {
        // Main projectile
        var angle = ShootAngle;
        angle.x = arcHeightAngle;
        angle.y = originAngle;
        ShootAngle = angle;

        // Sub-projectiles
        if (ListShootAngles == default)
        {
            ListShootAngles = new();
            for (int i = 0, count = ProjectilesPerShot - 1; i < count; ++i)
            {
                ListShootAngles.Add(new());
            }
        }
        else
        {
            while (ListShootAngles.Count < ProjectilesPerShot - 1)
            {
                ListShootAngles.Add(new());
            }
        }

        for (int i = 0, count = ListShootAngles.Count; i < count; ++i)
        {
            var angleRangeData = ListShootAngles[i];

            if (i % 2 == 0)
            {
                angle = angleRangeData.ShootAngle;
                angle.x = arcHeightAngle;
                angle.y = originAngle - shootingRangeAngle / 2f * (i / 2 + 1);
                angleRangeData.ShootAngle = angle;
            }
            else // if (i % 2 == 1)
            {
                angle = angleRangeData.ShootAngle;
                angle.x = arcHeightAngle;
                angle.y = originAngle + shootingRangeAngle / 2f * (i / 2 + 1);
                angleRangeData.ShootAngle = angle;
            }

            ListShootAngles[i] = angleRangeData;
        }
    }

    public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
    {
        var projectileGameObject = base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);

        var projectile = projectileGameObject.MMGetComponentNoAlloc<Projectile>();
        if (projectile == default)
        {
            return projectileGameObject;
        }

        if (IsAppliedMultiProjectile)
        {
            for (int i = 0, count = ListProjectileShootAngleRanges.Count; i < count; ++i)
            {
                var angleRange = ListProjectileShootAngleRanges[i];

                var randomizedAngle = UnityEngine.Random.Range(angleRange.FromAngle.x, angleRange.ToAngle.x);
                var rotateX = Quaternion.AngleAxis(randomizedAngle, transform.right);

                randomizedAngle = UnityEngine.Random.Range(angleRange.FromAngle.y, angleRange.ToAngle.y);
                var rotateY = Quaternion.AngleAxis(randomizedAngle, transform.up);

                randomizedAngle = UnityEngine.Random.Range(angleRange.FromAngle.z, angleRange.ToAngle.z);
                var rotateZ = Quaternion.AngleAxis(randomizedAngle, transform.forward);

                var direction = rotateX * rotateY * rotateZ * transform.forward;
                projectile.SetDirection(direction, transform.rotation, true);
            }
        }
        else
        {
            if (projectileIndex == 0)
            {
                var rotateX = Quaternion.AngleAxis(ShootAngle.x, transform.right);
                var rotateY = Quaternion.AngleAxis(ShootAngle.y, transform.up);
                var rotateZ = Quaternion.AngleAxis(ShootAngle.z, transform.forward);

                var direction = rotateX * rotateY * rotateZ * transform.forward;
                projectile.SetDirection(direction, transform.rotation, true);
            }
            else if (ListShootAngles.Count > 0)
            {
                SetupSubProjectilesDirection(projectile, projectileIndex);
            }
        }

        return projectileGameObject;
    }

    public void SetupSubProjectilesDirection(Projectile projectile, int projectileIndex)
    {
        var indexListAngles = (projectileIndex - 1) % ListShootAngles.Count;
        var shootAngleRange = ListShootAngles[indexListAngles];

        var angleX = shootAngleRange.ShootAngle.x;
        var rotateX = Quaternion.AngleAxis(angleX, transform.right);

        var angleY = shootAngleRange.ShootAngle.y;
        var rotateY = Quaternion.AngleAxis(angleY, transform.up);

        var angleZ = shootAngleRange.ShootAngle.z;
        var rotateZ = Quaternion.AngleAxis(angleZ, transform.forward);

        var direction = rotateX * rotateY * rotateZ * transform.forward;
        projectile.SetDirection(direction, transform.rotation, true);
    }

    public override void WeaponUse()
    {
        ApplyRecoil();
        TriggerWeaponUsedFeedback();

        DetermineSpawnPosition();

        if (IsAppliedMultiProjectile)
        {
            // TODO ?
        }
        else
        {
            AdjustProjectilesOffset(ProjectilesPerShot);
        }

        for (int i = 0; i < ProjectilesPerShot; i++)
        {
            // Setup offset
            if (IsAppliedMultiProjectile)
            {
                SpawnPosition = OffsetProjectSpawnedPosition(i);
            }
            else if (i > 0)
            {
                SpawnPosition = OffsetSubProjectileSpawnPosition(i);
            }

            SpawnProjectile(SpawnPosition, i, ProjectilesPerShot, true);
            PlaySpawnFeedbacks();
        }
    }

    private readonly Vector3 Vector3Zero = Vector3.zero;

    private void AdjustProjectilesOffset(int totalProjectiles)
    {
        if (ListSubSpawnOffsets != default && ListSubSpawnOffsets.Count >= totalProjectiles - 1)
        {
            return;
        }

        if (ListSubSpawnOffsets == default)
        {
            ListSubSpawnOffsets = new();
        }

        int indexAddedNew = ListSubSpawnOffsets.Count;
        while (ListSubSpawnOffsets.Count < totalProjectiles - 1)
        {
            ListSubSpawnOffsets.Add(new());
        }

        Vector3 lastValue, lastLastValue;
        for (int i = 0, count = ListSubSpawnOffsets.Count; i < count; ++i)
        {
            if (i < indexAddedNew)
            {
                continue;
            }

            // Check in group of 2 (i.e. even and odd index)
            lastLastValue = (i >= 4) ? ListSubSpawnOffsets[i - 4] : Vector3Zero;
            lastValue = (i >= 2) ? ListSubSpawnOffsets[i - 2] : Vector3Zero;

            var offset = ListSubSpawnOffsets[i];
            offset.x = lastValue.x + (lastValue.x - lastLastValue.x);
            offset.y = lastValue.y + (lastValue.y - lastLastValue.y);
            offset.z = lastValue.z + (lastValue.z - lastLastValue.z);

            ListSubSpawnOffsets[i] = offset;
        }
    }

    private Vector3 OffsetSubProjectileSpawnPosition(int projectileIndex)
    {
        Vector3 spawnPosition;
        var indexListOffsets = (projectileIndex - 1) % ListSubSpawnOffsets.Count;
        var startPositionOffset = ListSubSpawnOffsets[indexListOffsets];

        if (Flipped)
        {
            if (FlipWeaponOnCharacterFlip)
            {
                var flippedStartPositionOffset = startPositionOffset;
                flippedStartPositionOffset.y = -flippedStartPositionOffset.y;

                spawnPosition = this.transform.position - this.transform.rotation * flippedStartPositionOffset;
            }
            else
            {
                spawnPosition = this.transform.position - this.transform.rotation * startPositionOffset;
            }
        }
        else
        {
            spawnPosition = this.transform.position + this.transform.rotation * startPositionOffset;
        }

        return spawnPosition;
    }




    public void ShootStartOnDetectEnemy()
    {
        MMGameEvent.Trigger("DualShoot");
    }

    private Vector3 OffsetProjectSpawnedPosition(int projectileIndex)
    {
        Vector3 spawnPosition;
        var indexOffset = projectileIndex % ListProjectileSpawnOffsets.Count;
        var offset = ListProjectileSpawnOffsets[indexOffset];

        if (Flipped)
        {
            if (FlipWeaponOnCharacterFlip)
            {
                var flippedStartPositionOffset = offset;
                flippedStartPositionOffset.y = -flippedStartPositionOffset.y;

                spawnPosition = this.transform.position - this.transform.rotation * flippedStartPositionOffset;
            }
            else
            {
                spawnPosition = this.transform.position - this.transform.rotation * offset;
            }
        }
        else
        {
            spawnPosition = this.transform.position + this.transform.rotation * offset;
        }

        return spawnPosition;
    }


    #region Debug Visualization
    private void OnDrawGizmos()
    {
        if (IsAppliedMultiProjectile)
        {
            var direction = transform.forward;

            for (int i = 0, count = ListProjectileSpawnOffsets.Count; i < count; ++i)
            {
                var origin = transform.TransformPoint(transform.localPosition + ListProjectileSpawnOffsets[i]);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, origin + 2 * direction);

                if (i < ListProjectileShootAngleRanges.Count)
                {
                    var angleRange = ListProjectileShootAngleRanges[i];

                    Gizmos.color = Color.yellow;
                    var limitDirection = Quaternion.AngleAxis(angleRange.FromAngle.y, transform.up) * direction;
                    Gizmos.DrawLine(origin, origin + 2 * limitDirection);
                    
                    limitDirection = Quaternion.AngleAxis(angleRange.ToAngle.y, transform.up) * direction;
                    Gizmos.DrawLine(origin, origin + 2 * limitDirection);


                    Gizmos.color = Color.red;
                    limitDirection = Quaternion.AngleAxis(angleRange.FromAngle.x, transform.right) * direction;
                    Gizmos.DrawLine(origin, origin + 2 * limitDirection);

                    limitDirection = Quaternion.AngleAxis(angleRange.ToAngle.x, transform.right) * direction;
                    Gizmos.DrawLine(origin, origin + 2 * limitDirection);
                }
            }
        }
    }
    #endregion Debug Visualization
}
