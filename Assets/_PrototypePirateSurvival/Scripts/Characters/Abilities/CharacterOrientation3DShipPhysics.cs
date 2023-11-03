using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class CharacterOrientation3DShipPhysics : CharacterOrientation3D
{
    [Header("Ship Physics")]
    [MMInformation("\nThe following settings are only applied if rotation mode is Movement Direction\n", MMInformationAttribute.InformationType.Info, false)]
    // whether to apply modifications made for real-life ship physics simulation
    [Tooltip("whether to apply modifications made for real-life ship physics simulation")]
    public bool IsAppliedShipPhysics;

    // the threshold angle (in degrees) between successive input direction, above which an exclusive rotate speed was used to rotate the model
    [Tooltip("the threshold angle (in degrees) between successive input direction, above which an exclusive rotate speed was used to rotate the model")]
    public float ThresholdAngle = 100f;

    // the rotation speed for rotating direction vector an angle exceeding the threshold
    [Tooltip("the rotation speed for rotating direction vector an angle exceeding the threshold")]
    public float SpeedRotateDirectionVector = 0.5f;

    // the acceleration (or deacceleration if negative) for rotating direction vector
    [Tooltip("the acceleration (or deacceleration if negative) for rotating direction vector")]
    public float AccelerationRotateDirectionVector = -0.05f;

    protected Vector3 _previousDirection = Vector3.zero;
    protected Vector3 _currentControllerDirection = Vector3.zero;

    protected Vector2 _previousInput = Vector2.zero;
    protected Vector2 _currentInput = Vector2.zero;


    protected override void RotateToFaceMovementDirection()
    {
        _previousDirection = _currentDirection;

        base.RotateToFaceMovementDirection();

        if (!IsAppliedShipPhysics || !ShouldRotateToFaceMovementDirection)
        {
            return;
        }

        if ((RotationMode != RotationModes.MovementDirection) && (RotationMode != RotationModes.Both))
        {
            return;
        }

        // TODO: ...


        //_previousDirection = _currentControllerDirection;
        //_previousInput = _currentInput;

        //_currentControllerDirection = _controller.CurrentDirection;
        //_currentInput.x = _horizontalInput;
        //_currentInput.y = _verticalInput;

        // TODO: lerp angle instead
        //var originRotation = Quaternion.identity;
        //var targetAngle = Vector2.SignedAngle(_previousInput, _currentInput);
        //Debug.Log($"Target angle: {targetAngle}");
        //var targetRotation = Quaternion.AngleAxis(targetAngle, transform.up);


        //var baseSpeed = SpeedRotateDirectionVector;
        //var acceleration = AccelerationRotateDirectionVector;

        //var speed = Mathf.Lerp(baseSpeed, baseSpeed * (1 + acceleration), Mathf.Abs(acceleration) * Time.deltaTime);

        //var lerpRotation = Quaternion.Lerp(originRotation, targetRotation, 0.1f * Time.deltaTime);
        ////_currentControllerDirection = lerpRotation * _previousDirection;
        ////_currentInput = lerpRotation * _previousInput;

        //var rotation = Quaternion.RotateTowards(originRotation, targetRotation, 0.1f * 2 * Mathf.PI);
        //_currentControllerDirection = rotation * _previousDirection;
        //_currentInput = rotation * _previousInput;

        // 




        //Debug.Log($"Angle: {Vector3.Angle(_currentInput, _previousInput)}");
        //if (Vector3.Angle(_currentControllerDirection, _previousDirection) >= ThresholdAngle)
        //if (Vector3.Angle(_currentInput, _previousInput) >= ThresholdAngle)
        //{
        //    var baseSpeed = SpeedRotateDirectionVector;
        //    var acceleration = AccelerationRotateDirectionVector;

        //    var speed = Mathf.Lerp(baseSpeed, baseSpeed * (1 + acceleration), Time.deltaTime);
        //    Debug.Log($"Speed: {speed * Time.deltaTime}");

        //    _currentControllerDirection = Vector3.Lerp(_previousDirection, _currentControllerDirection, speed * Time.deltaTime);
        //    _currentInput = Vector2.Lerp(_previousInput, _currentInput, speed * Time.deltaTime);
        //}

        //_controller.CurrentDirection = _currentControllerDirection;

        //base.RotateToFaceMovementDirection();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + 3 * _currentControllerDirection);
        //Gizmos.DrawLine(transform.position, transform.position + 3 * _currentInput);
    }
}
