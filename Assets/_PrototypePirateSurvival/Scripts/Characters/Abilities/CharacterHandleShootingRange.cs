using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;
using UnityEngine;
using static CustomEvents;

[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
[RequireComponent(typeof(CharacterHandleWeapon))]
public class CharacterHandleShootingRange : CharacterAbility, MMEventListener<EnemyDetectEvent>, MMEventListener<MMGameEvent>
{
    [Header("Abilities")]
    //// handle weapon ability
    //[Tooltip("handle weapon ability")]
    //public CharacterHandleWeapon HandleWeaponAbility;
    //// handle secondary weapon ability
    //[Tooltip("handle secondary weapon ability")]
    //public CharacterHandleSecondaryWeapon HandleSecondaryWeaponAbility;
    //// list of handle sub-weapons ability
    //[Tooltip("list of handle sub-weapons ability")]
    //public List<CharacterHandleSubWeapon> ListHandleSubWeaponAbilities;

    [SerializeField, MMReadOnly]
    private List<CharacterHandleWeapon> ListActivatedWeaponAbilities;

    [Header("Feedback")]
    public MMF_Player ShootingRangeFeedbacks;

    [Header("Settings")]
    [Tooltip("The shooting style: None = no additional settings, FixedHorizontally = shoot outwards to both sides, always, LockedAimInRange = shoot at enemy in range only")]
    public ShootingStyle ShootingStyle;
    // shooting range radius
    [Tooltip("shooting range radius")]
    public float ShootingRangeRadius;
    // shooting range detection arc angle
    [Tooltip("shooting range detection arc angle")]
    public float DetectionRangeArcAngle = 60;
    // rotation angle for shooting range arc around Y axis
    [Tooltip("rotation angle for shooting range arc around Y axis")]
    public float ShootingRangeAngle = 60f;
    // rotation angle for shooting range arc around X axis
    [Tooltip("rotation angle for shooting range arc around X axis")]
    public float ShootingRangeArcHeightAngle = -50f;

    [Header("Output")]
    // required initial force
    [Tooltip("required initial force"), MMReadOnly]
    public float InitialForce;
    // required angle to lineup projectile
    [Tooltip("required angle to lineup projectile"), MMReadOnly]
    public float ProjectileAngleY;

    [Header("Debug")]
    [SerializeField, MMReadOnly]
    private float _initialVelocity;
    [SerializeField, MMReadOnly]
    private float _angle;
    [SerializeField, MMReadOnly]
    private float _gravityModifier;

    private ProjectileWeapon _weapon;
    private PhysicsProjectile _projectilePhysics;
    private Rigidbody _projectileRigidBody;


    protected override void Initialization()
    {
        base.Initialization();

        TriggerFeedbacks();

        var multiWeaponHandle = gameObject.GetComponentInChildren<MultiWeaponHandle>();
        if (multiWeaponHandle == default)
        {
            ListActivatedWeaponAbilities = _character.FindAbilities<CharacterHandleWeapon>();
        }
        else
        {
            ListActivatedWeaponAbilities = multiWeaponHandle.ListActivatedHandleWeaponAbilities;
        }

        if (ShootingStyle == ShootingStyle.Fixed4Direction)
        {
            // note: call for the initialization only, actual argument's value is not important
            RefreshTargetWeapon(handleWeaponWithEnemy: default);
            Invoke(nameof(SetupFixedHorizontalShooting), 1f);
        }
        else
        {
            var handleWeaponAbilities = _character.FindAbilities<CharacterHandleWeapon>();
            for (int i = 0, count = handleWeaponAbilities.Count; i < count; ++i)
            {
                var ability = handleWeaponAbilities[i];
                if (ability == default)
                {
                    continue;
                }

                ability.ForceAlwaysShoot = false;
            }

            //HandleWeaponAbility.ForceAlwaysShoot = false;
            //HandleSecondaryWeaponAbility.ForceAlwaysShoot = false;

            //for (int i = 0, count = ListHandleSubWeaponAbilities.Count; i < count; ++i)
            //{
            //    ListHandleSubWeaponAbilities[i].ForceAlwaysShoot = false;
            //}
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (ShootingStyle == ShootingStyle.LockedAimInRange)
        {
            EnableColliderTriggerFeedback();
            this.MMEventStartListening<EnemyDetectEvent>();
        }
        else
        {
            DisableColliderTriggerFeedback();
            this.MMEventStopListening<EnemyDetectEvent>();
        }

        this.MMEventStartListening<MMGameEvent>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (ShootingStyle == ShootingStyle.LockedAimInRange)
        {
            this.MMEventStopListening<EnemyDetectEvent>();
        }

        this.MMEventStopListening<MMGameEvent>();
    }

    private void ComputeRequiredInitialForce(float range)
    {
        // v = sqrt(x * g / sin2a) -> F = v * mass / Time.fixedDeltaTime

        var gravityModifier = Physics.gravity.magnitude;
        _gravityModifier = gravityModifier;

        var angle = (_weapon is ProjectileWeaponAngle) ? (_weapon as ProjectileWeaponAngle).ShootAngle.x : 0;
        _angle = Mathf.Abs(angle);

        _weapon.DetermineSpawnPosition();

        var distanceTillHitGround = _weapon.SpawnPosition.y / Mathf.Tan(_angle * Mathf.Deg2Rad);

        var relativeWeaponPositionX = _weapon.SpawnPosition.x - _character.transform.position.x;
        var relativeWeaponPositionZ = _weapon.SpawnPosition.z - _character.transform.position.z;
        var relativeWeaponOffset = Mathf.Max(relativeWeaponPositionX, relativeWeaponPositionZ);

        // TODO: temporary; find correct solution later!!!
        //var maxDistanceProjectile = (range >= distanceTillHitGround)
        //    ? range - distanceTillHitGround - relativeWeaponOffset
        //    : range - relativeWeaponOffset;
        var maxDistanceProjectile = range - relativeWeaponOffset;
        //maxDistanceProjectile += 0.5f;

        if (maxDistanceProjectile < 0)
        {
            maxDistanceProjectile = range;
        }

        _initialVelocity = Mathf.Sqrt(maxDistanceProjectile * _gravityModifier / Mathf.Sin(2 * _angle * Mathf.Deg2Rad));
        InitialForce = _initialVelocity * _projectileRigidBody.mass / Time.fixedDeltaTime;
    }

    private void SetForceToGetToShootingRange()
    {
        //var dynamicForceObjectPooler = HandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<DynamicForcePhysicObjectPooler>();
        //if (dynamicForceObjectPooler != default)
        //{
        //    dynamicForceObjectPooler.AppliedInitialForce = InitialForce;
        //}

        //dynamicForceObjectPooler = HandleSecondaryWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<DynamicForcePhysicObjectPooler>();
        //if (dynamicForceObjectPooler != default)
        //{
        //    dynamicForceObjectPooler.AppliedInitialForce = InitialForce;
        //}

        //for (int i = 0, count = ListHandleSubWeaponAbilities.Count; i < count; ++i)
        //{
        //    var handleSubWeaponAbility = ListHandleSubWeaponAbilities[i];

        //    dynamicForceObjectPooler = handleSubWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<DynamicForcePhysicObjectPooler>();
        //    if (dynamicForceObjectPooler != default)
        //    {
        //        dynamicForceObjectPooler.AppliedInitialForce = InitialForce;
        //    }
        //}

        for (int i = 0, count = ListActivatedWeaponAbilities.Count; i < count; ++i)
        {
            var ability = ListActivatedWeaponAbilities[i];
            if (ability == default)
            {
                continue;
            }

            var dynamicForceObjectPooler = ability.CurrentWeapon.gameObject.MMGetComponentNoAlloc<DynamicForcePhysicObjectPooler>();
            if (dynamicForceObjectPooler == default)
            {
                continue;
            }

            dynamicForceObjectPooler.AppliedInitialForce = InitialForce;
        }
    }

    private void ComputeRequiredAngleForProjectile(Vector3 enemyPosition)
    {
        var weaponPoint = _weapon.transform.position;
        var weaponAimDirection = _weapon.transform.forward;

        enemyPosition.y = weaponPoint.y;

        var distanceDirectionToEnemy = (enemyPosition - weaponPoint).normalized;
        ProjectileAngleY = Vector3.SignedAngle(weaponAimDirection, distanceDirectionToEnemy, _weapon.transform.up);
    }

    // TODO: re-check ProjectileAngleY
    private void SetAngleToLineUpProjectile()
    {
        //if (HandleWeaponAbility.CurrentWeapon is ProjectileWeaponAngle)
        //{
        //    var weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;
        //    weapon.AdjustProjectilesAngle(ProjectileAngleY, ShootingRangeArcHeightAngle, ShootingRangeAngle);
        //}

        //if (HandleSecondaryWeaponAbility.CurrentWeapon is ProjectileWeaponAngle)
        //{
        //    var weapon = HandleSecondaryWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;
        //    weapon.AdjustProjectilesAngle(ProjectileAngleY, ShootingRangeArcHeightAngle, ShootingRangeAngle);
        //}

        //for (int i = 0, count = ListHandleSubWeaponAbilities.Count; i < count; ++i)
        //{
        //    var handleSubWeaponAbility = ListHandleSubWeaponAbilities[i];

        //    if (handleSubWeaponAbility.CurrentWeapon is ProjectileWeaponAngle)
        //    {
        //        var weapon = handleSubWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;
        //        weapon.AdjustProjectilesAngle(ProjectileAngleY, ShootingRangeArcHeightAngle, ShootingRangeAngle);
        //    }
        //}

        for (int i = 0, count = ListActivatedWeaponAbilities.Count; i < count; ++i)
        {
            var ability = ListActivatedWeaponAbilities[i];
            if (ability == default || ability.CurrentWeapon is not ProjectileWeaponAngle)
            {
                continue;
            }

            var weapon = ability.CurrentWeapon as ProjectileWeaponAngle;
            weapon.AdjustProjectilesAngle(ProjectileAngleY, ShootingRangeArcHeightAngle, ShootingRangeAngle);
        }
    }

    private void TriggerFeedbacks()
    {
        var listScaleFeedbacks = ShootingRangeFeedbacks.GetFeedbacksOfType<MMF_Scale>();
        for (int i = 0, count = listScaleFeedbacks.Count; i < count; ++i)
        {
            var scaleFeedback = listScaleFeedbacks[i];

            var scale = scaleFeedback.DestinationScale;
            scale.x = ShootingRangeRadius * 2 + 1;
            scale.y = ShootingRangeRadius * 2 + 1;

            scaleFeedback.DestinationScale = scale;
        }

        var listFloatControllerFeedback = ShootingRangeFeedbacks.GetFeedbacksOfType<MMF_FloatController>();
        for (int i = 0, count = listFloatControllerFeedback.Count; i < count; ++i)
        {
            var floatControllerFeedback = listFloatControllerFeedback[i];

            if (floatControllerFeedback.Label.Equals("RadiusController", System.StringComparison.OrdinalIgnoreCase))
            {
                floatControllerFeedback.ToDestinationValue = ShootingRangeRadius;
            }
            else if (floatControllerFeedback.Label.Equals("AngleController", System.StringComparison.OrdinalIgnoreCase))
            {
                floatControllerFeedback.ToDestinationValue = DetectionRangeArcAngle;
            }
        }

        ShootingRangeFeedbacks.PlayFeedbacks();
    }

    public void OnMMEvent(EnemyDetectEvent eventType)
    {
        RefreshTargetWeapon(eventType.HandleWeaponAbility);

        var enemyPosition = eventType.EnemyPosition;
        var range = Vector3.Distance(enemyPosition, _weapon.transform.position);

        ComputeRequiredAngleForProjectile(eventType.EnemyPosition);
        SetAngleToLineUpProjectile();

        ComputeRequiredInitialForce(Mathf.Abs(range));
        SetForceToGetToShootingRange();

        MMGameEvent.Trigger("DualShoot");
    }

    private void RefreshTargetWeapon(CharacterHandleWeapon handleWeaponWithEnemy)
    {
        if (handleWeaponWithEnemy != default)
        {
            _weapon = handleWeaponWithEnemy.CurrentWeapon as ProjectileWeaponAngle;
        }
        else if (ListActivatedWeaponAbilities.Count > 0)
        {
            //_weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;
            _weapon = ListActivatedWeaponAbilities[0].CurrentWeapon as ProjectileWeaponAngle;
        }

        if (_weapon == default)
        {
            MMDebug.LogDebugToConsole("Error: no weapon of type ProjectileWeaponAngle spotted", "yellow", 3, true);
            return;
        }

        var projectileGameObject = (_weapon.ObjectPooler as MMSimpleObjectPooler).GameObjectToPool;
        if (projectileGameObject != default)
        {
            _projectilePhysics = projectileGameObject.MMGetComponentNoAlloc<PhysicsProjectile>();
            _projectileRigidBody = projectileGameObject.MMGetComponentNoAlloc<Rigidbody>();
        }
    }

    private void DisableColliderTriggerFeedback()
    {
        var colliderFeedback = ShootingRangeFeedbacks.GetFeedbackOfType<MMF_Collider>();
        if (colliderFeedback != default)
        {
            colliderFeedback.Mode = MMF_Collider.Modes.Disable;
        }
    }

    private void EnableColliderTriggerFeedback()
    {
        var colliderFeedback = ShootingRangeFeedbacks.GetFeedbackOfType<MMF_Collider>();
        if (colliderFeedback != default)
        {
            colliderFeedback.Mode = MMF_Collider.Modes.ToggleActive;
        }
    }

    private void SetupFixedHorizontalShooting()
    {
        ProjectileAngleY = 0;
        SetAngleToLineUpProjectile();

        ComputeRequiredInitialForce(Mathf.Abs(ShootingRangeRadius));
        SetForceToGetToShootingRange();

        //HandleWeaponAbility.ForceAlwaysShoot = true;
        //HandleSecondaryWeaponAbility.ForceAlwaysShoot = true;

        //for (int i = 0, count = ListHandleSubWeaponAbilities.Count; i < count; ++i)
        //{
        //    ListHandleSubWeaponAbilities[i].ForceAlwaysShoot = true;
        //}

        var handleWeaponAbilities = _character.FindAbilities<CharacterHandleWeapon>();
        for (int i = 0, count = handleWeaponAbilities.Count; i < count; ++i)
        {
            var ability = handleWeaponAbilities[i];
            if (ability == default)
            {
                continue;
            }

            ability.ForceAlwaysShoot = ListActivatedWeaponAbilities.Contains(ability);
        }
    }

    public void ToggleShootingStyle()
    {
        var newStyle = (ShootingStyle == ShootingStyle.Fixed4Direction) ? ShootingStyle.LockedAimInRange : ShootingStyle.Fixed4Direction;
        ChangeShootingStyle(newStyle);
    }

    private void ChangeShootingStyle(ShootingStyle newStyle)
    {
        ShootingStyle = newStyle;

        if (ShootingStyle == ShootingStyle.LockedAimInRange)
        {
            EnableColliderTriggerFeedback();
            this.MMEventStartListening<EnemyDetectEvent>();
        }
        else
        {
            DisableColliderTriggerFeedback();
            this.MMEventStopListening<EnemyDetectEvent>();
        }

        Initialization();
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        if (eventType.EventName.Equals("UpdateActivatedWeapons", System.StringComparison.OrdinalIgnoreCase))
        {
            Initialization();
        }
    }

    #region Obsolete
    // Obsolete
    //public void ComputeShootingRange()
    //{
    //    ProjectileWeapon _weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeaponAngle;
    //    if (_weapon == default)
    //    {
    //        _weapon = HandleWeaponAbility.CurrentWeapon as ProjectileWeapon;
    //    }

    //    var projectileGameObject = _weapon.ObjectPooler.GetPooledGameObject();
    //    if (projectileGameObject != default)
    //    {
    //        _projectilePhysics = projectileGameObject.MMGetComponentNoAlloc<PhysicsProjectile>();
    //        _projectileRigidBody = projectileGameObject.MMGetComponentNoAlloc<Rigidbody>();
    //    }

    //    // ===


    //    // Rough estimate: since start at initial height h0, the actual distance will be longer
    //    // x = (v0^2 * sin(2a)) / g

    //    if (HandleWeaponAbility == default)
    //    {
    //        return;
    //    }


    //    // === Computation - Rough estimation only ===
    //    var initialVelocity = (_projectilePhysics.InitialForce / _projectileRigidBody.mass) * Time.fixedDeltaTime;
    //    _initialVelocity = initialVelocity;

    //    float angle = 0;
    //    if (_weapon is ProjectileWeaponAngle)
    //    {
    //        var weaponAngle = _weapon as ProjectileWeaponAngle;
    //        //angle = Mathf.Max(Mathf.Abs(weaponAngle.FromShootAngle.x), Mathf.Abs(weaponAngle.ToShootAngle.x));
    //        angle = Mathf.Abs(weaponAngle.ShootAngle.x);
    //    }
    //    _angle = angle;

    //    var gravityModifier = Physics.gravity.magnitude;
    //    _gravityModifier = gravityModifier;

    //    _weapon.DetermineSpawnPosition();


    //    var maxDistanceProjectileMotion = initialVelocity * initialVelocity * Mathf.Sin(2 * angle * Mathf.Deg2Rad) / gravityModifier;
    //    var distanceTillHitGround = _weapon.SpawnPosition.y / Mathf.Tan(angle * Mathf.Deg2Rad);

    //    ShootingRangeRadius = _weapon.SpawnPosition.x + maxDistanceProjectileMotion + distanceTillHitGround;

    //    TriggerFeedbacks();
    //}
    #endregion Obsolete
}

[System.Serializable]
/// <summary>
/// The shooting style: None = no additional settings, Fixed4Direction = always shoot in 4 directions, LockedAimInRange = shoot at enemy in range only
/// </summary>
public enum ShootingStyle
{
    None = 0,
    Fixed4Direction = 10,
    LockedAimInRange = 20,
}
