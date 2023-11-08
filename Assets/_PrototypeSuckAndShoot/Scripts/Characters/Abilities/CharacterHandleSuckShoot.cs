using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpiritBomb.Prototype.SuckAndShoot
{
    //[MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
    public class CharacterHandleSuckShoot : CharacterAbility
    {
        // === State
        [Header("State")]
        // the current state: Idle (nothing), Charge (ready for action), or Trigger (execute action)
        [Tooltip("the current state: Idle (nothing), Charge (ready for action), or Trigger (execute action)")]
        [SerializeField, MMReadOnly]
        CharacterActionState CurrentState = CharacterActionState.Idle;
        // the next action to be executed on triggered
        [Tooltip("the next action to be executed on triggered")]
        [MMReadOnly]
        public CharacterActionType NextAction = CharacterActionType.Suck;

        // === Inputs
        [Header("Inputs")]
        // the minimum inputs to determine if the character should 'charge' for next action
        [Tooltip("the threhold inputs to determine if the character should 'charge' for next action")]
        public Vector2 ThresholdInputCharge = new(0.5f, 0.5f);

        // the maximum inputs to determine if the character should 'trigger' next action
        [Tooltip("the threhold inputs to determine if the character should 'trigger' next action")]
        public Vector2 ThresholdInputTrigger = Vector2.zero;

        // === Actions
        [Header("Actions")]
        // the shoot action in case of enough sucked objects
        [Tooltip("the shoot action in case of enough sucked objects")]
        public CharacterShootAction ShootAction;
        // the suck action in case of no sucked objects
        [Tooltip("the suck action in case of no sucked objects")]
        public CharacterSuckAction SuckAction;

        // === Feedbacks
        [Header("Feedbacks")]
        public MMF_Player SuckActionFeedback;
        public MMF_Player ShootActionFeedback;


        // === Debug
        [Header("Debug")]
        [MMReadOnly, SerializeField]
        protected bool IsSuckNext = false;
        [MMReadOnly, SerializeField]
        protected bool IsShootNext = false;

        protected Vector2 _currentInput = Vector2.zero;



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
        }

        protected override void HandleInput()
        {
            base.HandleInput();

            _currentInput.x = _horizontalInput;
            _currentInput.y = _verticalInput;

            if (CurrentState == CharacterActionState.Idle)
            {
                bool isPassThresholdCharge = _currentInput.magnitude >= ThresholdInputCharge.magnitude;
                if (isPassThresholdCharge)
                {
                    ChargeAction();
                }
            }
            else if (CurrentState == CharacterActionState.Charge)
            {
                bool isPassThresholdTrigger = _currentInput.magnitude <= ThresholdInputTrigger.magnitude;
                if (isPassThresholdTrigger)
                {
                    TriggerAction();
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

            if (NextAction == CharacterActionType.Suck)
            {
                bool suckResult = SuckAction.SuckTargets();
                Debug.Log($"Trigger sucking: {suckResult}");

                if (!suckResult)
                {
                    OnCompleteAction();
                }
            }
            else
            {
                ShootAction.ShootAllSuckedTargets();
                Debug.Log("Trigger shooting");
            }

            //OnCompleteAction();
        }


        public virtual void OnCompleteAction()
        {
            Debug.Log("OnCompleteAction");
            CurrentState = CharacterActionState.Idle;

            var countEnums = System.Enum.GetValues(typeof(CharacterActionType)).Length;
            NextAction = (CharacterActionType)(((int)NextAction + 1) % countEnums);
        }
    }

    enum CharacterActionState
    {
        Idle = 0,
        Charge = 1,
        Trigger = 2,
    }

    public enum CharacterActionType
    {
        Suck = 0,
        Shoot = 1,
    }
}
