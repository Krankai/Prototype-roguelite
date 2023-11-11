using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
    public class CharacterHandleSuckShoot : CharacterAbility
    {
        [Header("State")]
        // the current state: Idle (nothing), Charge (ready for action), or Trigger (execute action)
        [Tooltip("the current state: Idle (nothing), Charge (ready for action), or Trigger (execute action)")]
        [SerializeField, MMReadOnly]
        CharacterActionState CurrentState = CharacterActionState.Idle;
        // the next action to be executed on triggered
        [Tooltip("the next action to be executed on triggered")]
        [MMReadOnly]
        public CharacterActionType NextAction = CharacterActionType.Suck;


        [Header("Inputs")]
        // the minimum inputs to determine if the character should 'charge' for next action
        [Tooltip("the threhold inputs to determine if the character should 'charge' for next action")]
        public Vector2 ThresholdInputCharge = new(0.5f, 0.5f);

        // the maximum inputs to determine if the character should 'trigger' next action
        [Tooltip("the threhold inputs to determine if the character should 'trigger' next action")]
        public Vector2 ThresholdInputTrigger = Vector2.zero;


        [Header("Actions")]
        // the shoot action in case of enough sucked objects
        [Tooltip("the shoot action in case of enough sucked objects")]
        public CharacterShootAction ShootAction;

        // whether to delay shooting action until character 'stops' rotating
        [Tooltip("whether to delay shooting action until character 'stops' rotating")]
        public bool IsTriggerShootAtNoRotation;

        [MMCondition(nameof(IsTriggerShootAtNoRotation), true)]
        // the minimum threshold angle below which character's rotation is considered stopped
        [Tooltip("the minimum threshold angle below which character's rotation is considered stopped")]
        public float MinShootThresholdRotationAngle = 0.05f;


        // the suck action in case of no sucked objects
        [Tooltip("the suck action in case of no sucked objects")]
        public CharacterSuckAction SuckAction;

        // whether to delay sucking action until character 'stops' rotating
        [Tooltip("whether to delay sucking action until character 'stops' rotating")]
        public bool IsTriggerSuckAtNoRotation;

        [MMCondition(nameof(IsTriggerSuckAtNoRotation), true)]
        // the minimum threshold angle below which character's rotation is considered stopped
        [Tooltip("the minimum threshold angle below which character's rotation is considered stopped")]
        public float MinSuckThresholdRotationAngle = 0.05f;


        [Header("Feedbacks")]
        public MMF_Player SuckActionFeedback;
        public MMF_Player ShootActionFeedback;



        [Header("Debug")]
        [MMReadOnly, SerializeField]
        protected bool IsSuckNext = false;
        [MMReadOnly, SerializeField]
        protected bool IsShootNext = false;

        protected Vector2 _currentInput = Vector2.zero;
        protected bool _isSwitchAction = false;

        protected CharacterOrientation3D _characterOrientation;
        protected Vector3 _prevCharacterAngles;
        protected Vector3 _currentCharacterAngles;

        protected bool _isPassThresholdCharge = false;
        protected bool _isPassThresholdTrigger = false;

        protected bool _isBelowSuckThresholdAngle = false;
        protected bool _isBelowShootThresholdAngle = false;


        protected override void Initialization()
        {
            base.Initialization();

            NextAction = CharacterActionType.Suck;

            if (ShootAction == default)
            {
                ShootAction = gameObject.GetComponentInChildren<CharacterShootAction>();
            }

            ShootAction.OnShootCompleteEvent.RemoveAllListeners();
            ShootAction.OnShootCompleteEvent.AddListener(OnCompleteAction);

            if (SuckAction == default)
            {
                SuckAction = gameObject.GetComponentInChildren<CharacterSuckAction>();
            }

            SuckAction.OnSuckCompleteEvent.RemoveAllListeners();
            SuckAction.OnSuckCompleteEvent.AddListener(OnCompleteAction);

            _characterOrientation = _character.FindAbility<CharacterOrientation3D>();
            if (_characterOrientation != default)
            {
                _currentCharacterAngles = _characterOrientation.ModelAngles;
            }
        }

        public override void LateProcessAbility()
        {
            base.LateProcessAbility();

            if (_characterOrientation != default)
            {
                _prevCharacterAngles = _currentCharacterAngles;
                _currentCharacterAngles = _characterOrientation.ModelAngles;

                var angleOffset = Mathf.Abs(_currentCharacterAngles.y - _prevCharacterAngles.y);
                _isBelowShootThresholdAngle = angleOffset <= MinShootThresholdRotationAngle;
                _isBelowSuckThresholdAngle = angleOffset <= MinSuckThresholdRotationAngle;
            }

            HandleTriggerAction();
        }

        protected override void HandleInput()
        {
            base.HandleInput();

            _currentInput.x = _horizontalInput;
            _currentInput.y = _verticalInput;

            if (CurrentState == CharacterActionState.Idle)
            {
                if (_isPassThresholdCharge = _currentInput.magnitude >= ThresholdInputCharge.magnitude)
                {
                    ChargeAction();
                }
            }
            else if (CurrentState == CharacterActionState.Charge)
            {
                if (_isPassThresholdTrigger = _currentInput.magnitude <= ThresholdInputTrigger.magnitude)
                {
                    TriggerAction();
                }
            }
        }

        protected virtual void HandleTriggerAction()
        {
            if (CurrentState != CharacterActionState.Trigger)
            {
                return;
            }

            if (NextAction == CharacterActionType.Shoot)
            {
                if (!IsTriggerShootAtNoRotation || _isBelowShootThresholdAngle)
                {
                    CurrentState = CharacterActionState.Executing;

                    Debug.Log("Trigger shooting");
                    ShootAction.ShootAllSuckedTargets();
                }
            }
            else if (NextAction == CharacterActionType.Suck)
            {
                if (!IsTriggerSuckAtNoRotation || _isBelowSuckThresholdAngle)
                {
                    CurrentState = CharacterActionState.Executing;

                    bool suckResult = SuckAction.SuckTargets();
                    Debug.Log($"Trigger sucking: {suckResult}");
                }
            }
        }

        protected virtual void ChargeAction()
        {
            CurrentState = CharacterActionState.Charge;
            Debug.Log("Charge");
        }

        protected virtual void TriggerAction()
        {
            CurrentState = CharacterActionState.Trigger;

            //if (NextAction == CharacterActionType.Suck)
            //{
            //    //bool suckResult = SuckAction.SuckTargets();
            //    //Debug.LogError($"Trigger sucking: {suckResult}");

            //    // TODO: monitor ModelAngles.y offset to detect if the rotation is coming to a close soon
            //    Invoke(nameof(Test), 0.1f);
            //}
            //else
            //{
            //    Debug.Log("Trigger shooting");
            //    ShootAction.ShootAllSuckedTargets();
            //}
        }

        public virtual void OnCompleteAction(bool isSwitchAction)
        {
            CurrentState = CharacterActionState.Idle;

            if (isSwitchAction)
            {
                var countEnums = System.Enum.GetValues(typeof(CharacterActionType)).Length;
                NextAction = (CharacterActionType)(((int)NextAction + 1) % countEnums);
            }

            Debug.Log($"OnCompleteAction: {NextAction}");
        }
    }

    enum CharacterActionState
    {
        Idle = 0,
        Charge = 1,
        Trigger = 2,
        Executing = 3,
    }

    public enum CharacterActionType
    {
        Suck = 0,
        Shoot = 1,
    }
}
