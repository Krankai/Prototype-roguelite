using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class CharacterMovementShipPhysics : CharacterMovement
{
    [Header("Ship Physics")]
    [MMInformation("\nMovement Inertia takes priority over Input Inertia. Only one will be applied\n", MMInformationAttribute.InformationType.Info, false)]
    // whether to apply modifications made for real-life ship physics simulation
    [Tooltip("whether to apply modifications made for real-life ship physics simulation")]
    public bool IsAppliedShipPhysics;

    // the threshold angle (in degrees) between successive movement/input vectors, above which inertia will be applied
    //[Tooltip("the threshold angle (in degrees) between successive movement/input vectors, above which inertia will be applied")]
    //public float ThresholdAngle = 0f;

    // whether to apply inertia for movement vectors
    [Tooltip("whether to apply inertia for movement vectors")]
    public bool IsMovementInertia;

    // the inertia resistance against the abrupt change in movement direction (higher means longer to adapt to new movement)
    [MMCondition(nameof(IsMovementInertia), Negative = false)]
    [Tooltip("the inertia resistance against the abrupt change in movement direction (higher means longer to adapt to new movement)")]
    [Min(1f)]
    public float MovementInertiaResistance = 2f;

    // the deceleration to gradually cancel out the movement inertia resistance
    [MMCondition(nameof(IsMovementInertia), Negative = false)]
    [Tooltip("the deceleration to gradually cancel out the movement inertia resistance")]
    public float MovementInertiaDeceleration = 0.2f;

    // the limit to full stop the movement inertia resistance
    [MMCondition(nameof(IsMovementInertia), Negative = false)]
    [Tooltip("the limit to full stop the movement inertia resistance")]
    public float LimitToCancelInertia = 0.005f;


    // whether to apply inertia for input vectors
    [Tooltip("whether to apply inertia for input vectors")]
    public bool IsInputInertia;

    // the inertia resistance against the abrupt change in input direction (higher means more resistance)
    [MMCondition(nameof(IsInputInertia), Negative = false)]
    [Tooltip("the inertia resistance against the abrupt change in input direction (higher means more resistance)")]
    [Min(1f)]
    public float InputInertiaResistance = 2f;

    // the (de)acceleration to gradually cancel out the input inertia resistance
    [MMCondition(nameof(IsInputInertia), Negative = false)]
    [Tooltip("the (de)acceleration to gradually cancel out the input inertia resistance")]
    public float InputInertiaDeacceleration = 0.2f;

    // the limit to full stop the input inertia resistance
    [MMCondition(nameof(IsInputInertia), Negative = false)]
    [Tooltip("the limit to full stop the input inertia resistance")]
    public float LimitToCancelInputInertia = 0.005f;


    protected Vector2 _previousInput;
    protected Vector2 _newInput;

    protected Vector3 _previousMovementVector;
    protected Vector3 _newMovementVector;

    protected CharacterOrientation3D _characterOrientation;


    protected override void Initialization()
    {
        base.Initialization();

        _characterOrientation = _character.FindAbility<CharacterOrientation3D>();
    }

    Vector3 _newDirection;
    Vector3 _previousDirection;
    Vector3 _currentDirection;

    // TODO: lerp horizontal input (and do the same for CharacterOrientation3D)
    float _prevHorizontalInput = 0f;
    float _prevVerticalInput = 0f;
    Vector2 _currentInput2 = Vector2.zero;

    protected override void HandleInput()
    {
        if (ScriptDrivenInput || !InputAuthorized)
        {
            return;
        }

        var prevInput = new Vector2(_prevHorizontalInput, _prevVerticalInput);
        _currentInput2 = new Vector2(_horizontalInput, _verticalInput);

        var deltaAngle = Vector2.SignedAngle(prevInput, _currentInput2);
        var angleToRotate = Mathf.Lerp(0, deltaAngle, Time.deltaTime);
        Debug.Log($"Angle: {angleToRotate}");

        if (Mathf.Abs(deltaAngle) >= 90f)
        {
            _currentInput2 = Quaternion.AngleAxis(45f * Mathf.Sign(angleToRotate), transform.up) * _currentInput2;
            _horizontalInput = _currentInput2.x;
            _verticalInput = _currentInput2.y;

            _prevHorizontalInput = _horizontalInput;
            _prevVerticalInput = _verticalInput;
        }

        // TODO: next....

        base.HandleInput();
    }


    protected override void SetMovement()
    {
        if (!IsAppliedShipPhysics)
        {
            base.SetMovement();
            return;
        }

        if (IsMovementInertia)
        {
            _previousMovementVector = _movementVector;

            base.SetMovement();

            //if (Vector3.Angle(_previousMovementVector, _movementVector) < ThresholdAngle)
            //{
            //    return;
            //}

            bool isCancelMovementX = Mathf.Approximately(_horizontalMovement, 0f);
            bool isCancelMovementZ = Mathf.Approximately(_verticalMovement, 0f);

            var baseInertiaSpeed = 1f / MovementInertiaResistance;
            if (_movementVector.magnitude > _previousMovementVector.magnitude)
            {
                baseInertiaSpeed = 1f / Mathf.Clamp(MovementInertiaResistance * 0.5f, 1, MovementInertiaResistance);
            }

            // ===
            //// TODO: calculate x movement to rotate a little bit sideway
            //if (_characterOrientation != default && _characterOrientation.ShouldRotateToFaceMovementDirection)
            //{
            //    var rotatingModel = (_characterOrientation.MovementRotatingModel != default) ? _characterOrientation.MovementRotatingModel : _model;

            //    // TODO: more conditions

            //    _previousDirection = _currentDirection;
            //    _currentDirection = _characterOrientation.ModelDirection; ;

            //    var rotation = Quaternion.AngleAxis(Vector3.SignedAngle(_previousDirection, _currentDirection, transform.up) * 5f / 360f, transform.up);

            //    var tempRotation = Quaternion.LookRotation(_currentDirection);
            //    var newMovementQuaternion = Quaternion.Slerp(rotatingModel.transform.rotation, tempRotation, Time.deltaTime * _characterOrientation.RotateToFaceMovementDirectionSpeed);

            //    var newDirection = newMovementQuaternion * _previousDirection;
            //    //_newDirection = rotation * previousDirection;
            //    Debug.Log($"Rotation: {newMovementQuaternion.eulerAngles.y}");

            //    _movementVector.x += 5 * _newDirection.x;
            //    _movementVector.z += 5 * _newDirection.z;
            //}
            // ===



            var speed = Mathf.Lerp(baseInertiaSpeed, baseInertiaSpeed * (1 + MovementInertiaDeceleration), MovementInertiaDeceleration * Time.deltaTime);
            //Debug.Log($"Applied speed: {speed}");

            _newMovementVector.x = Mathf.Lerp(_previousMovementVector.x, _movementVector.x, speed * Time.deltaTime);
            _newMovementVector.z = Mathf.Lerp(_previousMovementVector.z, _movementVector.z, speed * Time.deltaTime);

            _movementVector.x = isCancelMovementX && Mathf.Abs(_newMovementVector.x) < LimitToCancelInertia ? _movementVector.x : _newMovementVector.x;
            _movementVector.z = isCancelMovementZ && Mathf.Abs(_newMovementVector.z) < LimitToCancelInertia ? _movementVector.z : _newMovementVector.z;

            _controller.SetMovement(_movementVector);
        }
        else if (IsInputInertia)
        {
            _previousInput = _currentInput;

            _newInput.x = _horizontalMovement;
            _newInput.y = _verticalMovement;

            //if (Vector2.Angle(_newInput, _previousInput) < ThresholdAngle)
            //{
            //    base.SetMovement();
            //    return;
            //}

            bool isCancelInputX = Mathf.Approximately(_newInput.x, 0f);
            bool isCancelInputY = Mathf.Approximately(_newInput.y, 0f);

            var baseInertiaSpeed = 1f / InputInertiaResistance;
            var speed = Mathf.Lerp(baseInertiaSpeed, baseInertiaSpeed * (1 + InputInertiaDeacceleration), InputInertiaDeacceleration * Time.deltaTime);
            //Debug.Log($"Applied speed: {speed}");

            _newInput = Vector2.Lerp(_previousInput, _newInput, speed * Time.deltaTime);

            _horizontalMovement = isCancelInputX && Mathf.Abs(_newInput.x) < LimitToCancelInertia ? 0f : _newInput.x;
            _verticalMovement = isCancelInputY && Mathf.Abs(_newInput.y) < LimitToCancelInertia ? 0 : _newInput.y;

            base.SetMovement();
        }
    }

    private void OnDrawGizmos()
    {
        if (IsAppliedShipPhysics)
        {
            var origin = transform.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + 3 * _movementVector);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + 3 * _characterOrientation.ModelDirection);
        }
    }
}
