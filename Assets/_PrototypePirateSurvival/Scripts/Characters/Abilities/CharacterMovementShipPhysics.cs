using MoreMountains.TopDownEngine;
using UnityEngine;

public class CharacterMovementShipPhysics : CharacterMovement
{
    [Header("Ship Physics")]
    // whether to apply modifications made for real-life ship physics simulation
    [Tooltip("whether to apply modifications made for real-life ship physics simulation")]
    public bool IsAppliedShipPhysics;

    // the inertia resistance against the abrupt change in movement direction (higher means more resistance)
    [Tooltip("the inertia resistance against the abrupt change in movement direction (higher means more resistance)")]
    [Min(1f)]
    public float InertiaResistance = 2f;

    // the inertia deacceleration
    [Tooltip("the inertia deacceleration")]
    public float InertiaDeacceleration = 0.2f;
    // the limit to full stop inertia movement
    [Tooltip("the limit to full stop inertia movement")]
    public float LimitToCancelInertia = 0.005f;

    //// the threshold angle (in degress) between successive inputs to apply inertia resistance
    //[Tooltip("the threshold angle (in degress) between successive inputs to apply inertia resistance")]
    //public float ThresholdInertiaAngle = 30f;

    protected Vector2 _previousInput;
    protected Vector2 _nextInput = Vector2.zero;


    protected override void SetMovement()
    {
        if (!IsAppliedShipPhysics)
        {
            base.SetMovement();
            return;
        }

        _previousInput = _currentInput;

        _nextInput.x = _horizontalMovement;
        _nextInput.y = _verticalMovement;

        bool isCancelInputX = Mathf.Approximately(_nextInput.x, 0f);
        bool isCancelInputY = Mathf.Approximately(_nextInput.y, 0f);

        var speed = 1f / InertiaResistance + InertiaDeacceleration * Time.deltaTime;
        _nextInput = Vector2.Lerp(_previousInput, _nextInput, speed * Time.deltaTime);

        //bool isSlowingDownMovementX = isCancelInputX
        //    && Mathf.Abs(_nextInput.x) < Mathf.Abs(_previousInput.x)
        //    && Mathf.Abs(_nextInput.x) < LimitToCancelInertia;

        //bool isSlowingDownMovementY = isCancelInputY
        //    && Mathf.Abs(_nextInput.y) < Mathf.Abs(_previousInput.y)
        //    && Mathf.Abs(_nextInput.y) < LimitToCancelInertia;

        _horizontalMovement = isCancelInputX && Mathf.Abs(_nextInput.x) < LimitToCancelInertia ? 0f : _nextInput.x;
        _verticalMovement = isCancelInputY && Mathf.Abs(_nextInput.y) < LimitToCancelInertia ? 0 : _nextInput.y;

        base.SetMovement();
    }
}
