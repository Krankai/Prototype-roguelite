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
        public CharacterOrientation3D OrientationThreeD;


        [Header("Input")]
        // the threshold input to determine if weapon is 'charged' on player movement
        [Tooltip("the threshold input to determine if weapon is 'charged' on player movement")]
        public Vector2 ThresholdInputCharge = Vector2.zero;

        // the threshold input to determine if trigger 'shoot' on weapon when player stops
        [Tooltip("the threshold input to determine if trigger 'shoot' on weapon when player stops")]
        public Vector2 ThresholdInputShoot = Vector2.zero;


        [Header("Rotation")]
        // whether to rotate character facing shooting direction
        [Tooltip("whether to rotate character facing shooting direction")]
        public bool IsRotateFacingShootDirection;


        [Header("Shoot")]
        // the number of frames to be delayed before triggering the shoot action
        [Tooltip("the number of frames to be delayed before triggering the shoot action")]
        [Min(0)]
        public int ShootDelayedFrameCount = 5;


        [Header("Feedback")]
        public MMF_Player ShootFeedback;

        protected Vector2 _currentInput = Vector2.zero;
        protected bool _cachedForcedRotation = false;
        protected bool _isTriggerShoot = false;



        protected override void Initialization()
        {
            base.Initialization();

            if (HandleWeapon == default)
            {
                HandleWeapon = _character.FindAbility<CharacterHandleWeapon>();
            }

            if (OrientationThreeD == default)
            {
                OrientationThreeD = _character.FindAbility<CharacterOrientation3D>();
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

                if (IsRotateFacingShootDirection)
                {
                    _cachedForcedRotation = OrientationThreeD.ForcedRotation;

                    OrientationThreeD.ForcedRotation = true;
                    OrientationThreeD.ShouldRotateToFaceWeaponDirection = true;
                }

                if (HandleWeapon.CurrentWeapon.WeaponUsedMMFeedback != ShootFeedback)
                {
                    HandleWeapon.CurrentWeapon.WeaponUsedMMFeedback = ShootFeedback;
                }

                ShootAfterFrames();
            }

            IsCharging = false;
            IsCanShoot = false;
        }

        protected virtual void ShootAfterFrames()
        {
            StartCoroutine(CoroutineActionAfterDelayedFrames(ShootDelayedFrameCount, HandleWeapon.ShootStart));
        }

        public virtual void OnResetForceRotation()
        {
            OrientationThreeD.ForcedRotation = _cachedForcedRotation;
            OrientationThreeD.ShouldRotateToFaceWeaponDirection = false;
        }

        private IEnumerator CoroutineActionAfterDelayedFrames(int delayFrames, System.Action callbackAction)
        {
            while (delayFrames-- > 0)
            {
                yield return null;
            }

            callbackAction?.Invoke();
        }
    }
}
