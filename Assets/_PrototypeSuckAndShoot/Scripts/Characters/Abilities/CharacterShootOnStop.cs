using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
    public class CharacterShootOnStop : CharacterAbility
    {
        [MMReadOnly]
        public bool IsCharging = false;
        [MMReadOnly]
        public bool IsCanShoot = false;


        [Header("Bindings")]
        public CharacterHandleWeapon HandleWeapon;


        [Header("Input")]
        // the threshold input to determine if weapon is 'charged' on player movement
        [Tooltip("the threshold input to determine if weapon is 'charged' on player movement")]
        public Vector2 ThresholdInputCharge = Vector2.zero;

        // the threshold input to determine if trigger 'shoot' on weapon when player stops
        [Tooltip("the threshold input to determine if trigger 'shoot' on weapon when player stops")]
        public Vector2 ThresholdInputShoot = Vector2.zero;


        [Header("Feedback")]
        public MMF_Player ShootFeedback;

        protected Vector2 _currentInput = Vector2.zero;


        protected override void Initialization()
        {
            base.Initialization();

            if (HandleWeapon == default)
            {
                HandleWeapon = _character.FindAbility<CharacterHandleWeapon>();
            }
        }

        //private void Update()
        //{
        //    Debug.Log($"Input: {Lean.Touch.LeanTouch.Fingers.Count}");
        //}

        protected override void HandleInput()
        {
            base.HandleInput();

            _currentInput.x = _horizontalInput;
            _currentInput.y = _verticalInput;

            bool isPassThresholdCharge = _currentInput.magnitude >= ThresholdInputCharge.magnitude;
            if (!IsCharging && isPassThresholdCharge)
            {
                Charge();
            }

            bool isPassThresholdShoot = _currentInput.magnitude <= ThresholdInputCharge.magnitude;
            bool isReleaseInput = HandleWeapon.CurrentWeapon.InputAuthorized && Lean.Touch.LeanTouch.Fingers.Count <= 0;
            if (IsCharging && isReleaseInput && isPassThresholdShoot)
            {
                AttemptShoot();
            }
        }

        protected virtual void Charge()
        {
            Debug.LogError("Charge");
            IsCharging = true;
        }

        protected virtual void AttemptShoot()
        {
            if (IsCanShoot)
            {
                Debug.LogWarning("Shoot");

                HandleWeapon.CurrentWeapon.WeaponUsedMMFeedback = ShootFeedback;
                HandleWeapon.ShootStart();
                //ShootFeedback.PlayFeedbacks();
            }

            IsCharging = false;
            IsCanShoot = false;
        }
    }
}
