using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Security.Cryptography;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class CharacterMovementShipPhysics : CharacterMovement
{
    [Header("Ship Physics")]
    [MMInformation("\nMovement Inertia takes priority over Input Inertia. Only one will be applied\n", MMInformationAttribute.InformationType.Info, false)]
    // whether to apply modifications made for real-life ship physics simulation
    [Tooltip("whether to apply modifications made for real-life ship physics simulation")]
    public bool IsAppliedShipPhysics;

    // the inertia resistance against the abrupt change in movement direction (higher means longer to adapt to new movement)
    [Tooltip("the inertia resistance against the abrupt change in movement direction (higher means longer to adapt to new movement)")]
    [Min(1f)]
    public float MovementInertiaResistance = 2f;

    // the inertia speed in case of abrupt change in movement direction (higher value means slower transition to new movement direction)
    [Tooltip("the inertia speed in case of abrupt change in movement direction (higher value means slower transition to new movement direction)")]
    public Vector2 MovementInertiaSpeed = new(2f, 2f);

    // the deceleration to gradually cancel out the movement inertia
    [Tooltip("the deceleration to gradually cancel out the movement inertia")]
    public Vector2 MovementInertiaDeceleration = new(0.2f, 0.2f);

    // the limit to cancel the movement inertia resistance
    [Tooltip("the limit to cancel the movement inertia resistance")]
    public Vector2 LimitToCancelInertia = new(0.1f, 0.1f);


    // whether to apply extra modifications in case of large angles
    [Tooltip("whether to apply extra modifications in case of large angles")]
    public bool IsLargeAngleModified;

    // the threshold angle (in degrees) between successive movement/input vectors, above which movement vector modifications will be applied
    [MMCondition(nameof(IsLargeAngleModified), Hidden = true)]
    [Tooltip("the threshold angle (in degrees) between successive movement/input vectors, above which extra intertia modifications will be applied")]
    public float ThresholdAngle = 0f;

    // the modifiers for the movement vector increase in case of large angles
    [MMCondition(nameof(IsLargeAngleModified), Hidden = true)]
    [Tooltip("the modifiers for the movement vector increase in case of large angles")]
    public Vector2 ModifierIncreaseMovement = new(1f, 1f);

    // the speed at which movement vector changes in case of large angles
    [MMCondition(nameof(IsLargeAngleModified), Hidden = true)]
    [Tooltip("the speed at which movement vector changes in case of large angles")]
    public Vector2 SpecializedMovementInertiaSpeed = new(1f, 1f);

    // the deceleration to gradually cancel out the specialized movement inertia
    [MMCondition(nameof(IsLargeAngleModified), Hidden = true)]
    [Tooltip("the deceleration to gradually cancel out the specialized movement inertia")]
    public Vector2 SpecializedMovementInertiaDeceleration = new(0.2f, 0.2f);


    // whether to clamp magnitude of movement vector
    [Tooltip("whether to clamp magnitude of movement vector")]
    public bool IsClampMagnitude;

    // TODO: clamp magnitude movement vector


    protected Vector3 _previousMovementVector;
    protected Vector3 _newMovementVector;

    protected Vector2 _inertiaSpeed = Vector2.zero;
    protected Vector2 _specializedInertiaSpeed = Vector2.zero;


    protected override void SetMovement()
    {
        if (!IsAppliedShipPhysics)
        {
            base.SetMovement();
            return;
        }

        _previousMovementVector = _movementVector;

        // Reset
        if (_previousMovementVector.x == 0)
        {
            _inertiaSpeed.x = MovementInertiaSpeed.x;
        }

        if (_previousMovementVector.y == 0)
        {
            _inertiaSpeed.y = MovementInertiaSpeed.y;
        }


        base.SetMovement();

        bool isCancelMovementX = Mathf.Approximately(_horizontalMovement, 0f);
        bool isCancelMovementZ = Mathf.Approximately(_verticalMovement, 0f);

        var vector3CurrentInput = new Vector3(_currentInput.x, 0, _currentInput.y);
        vector3CurrentInput = Vector3.ClampMagnitude(vector3CurrentInput, _previousMovementVector.magnitude);

        var angle = Vector3.Angle(vector3CurrentInput, _previousMovementVector);

        // TODO: check how to turn ship from stationary (or near zero movement)
        var isBackwardFromStationary = _previousMovementVector.magnitude <= 0.5f && _currentInput.magnitude >= 0.5f;

        if (IsLargeAngleModified && (angle >= ThresholdAngle || isBackwardFromStationary))
        {
            //Debug.LogError($"Angle: {angle}");

            var modifiedMovementVector = _movementVector;

            var increaseMovementValue = Mathf.Abs(_previousMovementVector.magnitude);
            var movementModifierVector = new Vector3(ModifierIncreaseMovement.x, 0, ModifierIncreaseMovement.y);
            var modifierAngle = Vector3.SignedAngle(transform.forward, _previousMovementVector, transform.up);

            movementModifierVector = Quaternion.AngleAxis(modifierAngle, transform.up) * movementModifierVector;

            //Debug.Log($"Modifers: {movementModifierVector}");

            modifiedMovementVector.x += increaseMovementValue * movementModifierVector.x;
            modifiedMovementVector.z += increaseMovementValue * movementModifierVector.z;
            // TODO: clamp magnitude only, but keep the sign

            //Debug.Log($"Modified: {modifiedMovementVector}");

            var acceleration = SpecializedMovementInertiaDeceleration.x;
            _specializedInertiaSpeed.x = Mathf.Lerp(_specializedInertiaSpeed.x, _specializedInertiaSpeed.x + (1 + acceleration), acceleration * Time.deltaTime);

            acceleration = SpecializedMovementInertiaDeceleration.y;
            _specializedInertiaSpeed.y = Mathf.Lerp(_specializedInertiaSpeed.y, _specializedInertiaSpeed.y + (1 + acceleration), acceleration * Time.deltaTime);


            if (Mathf.Abs(_previousMovementVector.x) <= 0.5f)
            {
                _previousMovementVector.x = 0.5f;
            }

            if (Mathf.Abs(_previousMovementVector.z) <= 0.2f)
            {
                _previousMovementVector.z = 0.2f;
            }

            _newMovementVector.x = Mathf.Lerp(_previousMovementVector.x, modifiedMovementVector.x, _specializedInertiaSpeed.x * Time.deltaTime);
            _newMovementVector.z = Mathf.Lerp(_previousMovementVector.z, modifiedMovementVector.z, _specializedInertiaSpeed.y * Time.deltaTime);
        }
        else
        {
            // Reset
            _specializedInertiaSpeed = SpecializedMovementInertiaSpeed;

            _inertiaSpeed.x = Mathf.Lerp(_inertiaSpeed.x, _inertiaSpeed.x * (1 + MovementInertiaDeceleration.x), MovementInertiaDeceleration.x * Time.deltaTime);
            _inertiaSpeed.y = Mathf.Lerp(_inertiaSpeed.y, _inertiaSpeed.y * (1 + MovementInertiaDeceleration.y), MovementInertiaDeceleration.y * Time.deltaTime);

            _newMovementVector.x = Mathf.Lerp(_previousMovementVector.x, _movementVector.x, _inertiaSpeed.x * Time.deltaTime);
            _newMovementVector.z = Mathf.Lerp(_previousMovementVector.z, _movementVector.z, _inertiaSpeed.y * Time.deltaTime);
        }

        _movementVector.x = isCancelMovementX && Mathf.Abs(_newMovementVector.x) < LimitToCancelInertia.x ? _movementVector.x : _newMovementVector.x;
        _movementVector.z = isCancelMovementZ && Mathf.Abs(_newMovementVector.z) < LimitToCancelInertia.y ? _movementVector.z : _newMovementVector.z;

        _controller.SetMovement(_movementVector);
    }

    private void OnDrawGizmos()
    {
        if (IsAppliedShipPhysics)
        {
            var origin = transform.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + 3 * _movementVector);

            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(origin, origin + 3 * _characterOrientation.ModelDirection);
        }
    }
}
