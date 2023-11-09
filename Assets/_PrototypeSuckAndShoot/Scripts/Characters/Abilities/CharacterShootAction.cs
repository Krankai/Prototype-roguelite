using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    public class CharacterShootAction : MonoBehaviour, MMEventListener<SuckedTargetEvent>
    {
        // === Projectile
        [Header("Projectile")]
        // the prefabs used as projectile for shooting
        [Tooltip("the prefabs used as projectile for shooting")]
        [MMReadOnly]
        //public GameObject ShootProjectile;
        public List<GameObject> ShootProjectiles;

        //// the scale to apply for the shooting projectile
        //[Tooltip("the scale to apply for the shooting projectile")]
        //[MMReadOnly]
        //public Vector3 ScaleProjectile = Vector3.one;

        // === Bindings
        [Header("Bindings")]
        public CharacterHandleWeapon HandleWeapon;

        // === Feedbacks
        [Header("Feedbacks")]
        public MMF_Player ShootStartFeedback;
        public MMF_Player ShootCompleteFeedback;

        public UnityEvent OnShootStartEvent;
        public UnityEvent<bool> OnShootCompleteEvent;

        protected Character _character;
        protected MMF_Events _shootCompleteMMFEvent;


        protected virtual void Start()
        {
            Initialization();
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening();
        }

        protected virtual void Initialization()
        {
            _character = LevelManager.Instance.Players[0];

            if (HandleWeapon == default)
            {
                HandleWeapon = _character.FindAbility<CharacterHandleWeapon>();
            }
        }

        public virtual void ShootAllSuckedTargets()
        {
            // TODO: set projectile before shooting from Suckable

            if (ShootStartFeedback != default)
            {
                ShootStartFeedback.PlayFeedbacks();
            }
            OnShootStartEvent?.Invoke();

            HandleWeapon.CurrentWeapon.WeaponStopMMFeedback = ShootCompleteFeedback;
            HandleWeapon.ShootStart();
        }

        public virtual void OnShootComplete()
        {
            OnShootCompleteEvent?.Invoke(true);
        }

        public void OnMMEvent(SuckedTargetEvent eventType)
        {
            if (eventType.Suckable == default)
            {
                return;
            }

            if (HandleWeapon.CurrentWeapon == default)
            {
                HandleWeapon.OnWeaponChange += () => OnSaveSuckedAsProjectile(eventType.Suckable);
                return;
            }

            OnSaveSuckedAsProjectile(eventType.Suckable);
        }

        protected virtual void OnSaveSuckedAsProjectile(CharacterSuckable suckedTarget)
        {
            var dynamicShapeWeapon = HandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<DynamicShapeProjectileWeapon>();
            if (dynamicShapeWeapon == default)
            {
                return;
            }

            dynamicShapeWeapon.CacheProjectile(suckedTarget);
        }
    }
}
