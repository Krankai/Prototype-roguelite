using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeaponAngle : ProjectileWeapon
{
    [MMInspectorGroup("Projectiles", true, 22)]
    [Header("Shoot Angle")]
    // the range of shooting angle (in degrees) to apply for each projectile spawn
    [Tooltip("the range of shooting angle (in degrees) to apply for each projectile spawn")]
    public Vector3 ShootAngle = Vector3.zero;

    [Header("Sub Projectile")]
    public List<RangeShootAngle> ListShootAngles;
    [Tooltip("local positions")]
    public List<Vector3> ListStartOffsets;


    public virtual void AdjustProjectilesAngle(float originAngle, float arcHeightAngle, float shootingRangeAngle)
    {
        // Main projectil
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

        //Debug.Log($"Index: {projectileIndex}, Rotation: {angleX}, {angleY}, {angleZ}");
    }

    public override void WeaponUse()
    {
        ApplyRecoil();
        TriggerWeaponUsedFeedback();

        DetermineSpawnPosition();
        AdjustProjectilesOffset(ProjectilesPerShot);

        for (int i = 0; i < ProjectilesPerShot; i++)
        {
            // Setup offset
            if (i > 0)
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
        if (ListStartOffsets != default && ListStartOffsets.Count >= totalProjectiles - 1)
        {
            return;
        }

        if (ListStartOffsets == default)
        {
            ListStartOffsets = new();
        }

        int indexAddedNew = ListStartOffsets.Count;
        while (ListStartOffsets.Count < totalProjectiles - 1)
        {
            ListStartOffsets.Add(new());
        }

        Vector3 lastValue, lastLastValue;
        for (int i = 0, count = ListStartOffsets.Count; i < count; ++i)
        {
            if (i < indexAddedNew)
            {
                continue;
            }

            // Check in group of 2 (i.e. even and odd index)
            lastLastValue = (i >= 4) ? ListStartOffsets[i - 4] : Vector3Zero;
            lastValue = (i >= 2) ? ListStartOffsets[i - 2] : Vector3Zero;

            var offset = ListStartOffsets[i];
            offset.x = lastValue.x + (lastValue.x - lastLastValue.x);
            offset.y = lastValue.y + (lastValue.y - lastLastValue.y);
            offset.z = lastValue.z + (lastValue.z - lastLastValue.z);

            ListStartOffsets[i] = offset;
        }
    }

    private Vector3 OffsetSubProjectileSpawnPosition(int projectileIndex)
    {
        Vector3 spawnPosition;
        var indexListOffsets = (projectileIndex - 1) % ListStartOffsets.Count;
        var startPositionOffset = ListStartOffsets[indexListOffsets];

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
}

[System.Serializable]
public struct RangeShootAngle
{
    public Vector3 ShootAngle;
}
