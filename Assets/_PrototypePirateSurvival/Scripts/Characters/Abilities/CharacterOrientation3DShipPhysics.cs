using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterOrientation3DShipPhysics;

public class CharacterOrientation3DShipPhysics : CharacterOrientation3D
{
    [System.Serializable]
    public struct ExclusiveRotationSpeedForAngle
    {
        public float ThresholdAngle;
        public float RotationSpeed;
        public float RotationAcceleration;
    }


    [Header("Ship Physics")]
    // whether to apply modifications made for real-life ship physics simulation
    [Tooltip("whether to apply modifications made for real-life ship physics simulation")]
    public bool IsAppliedShipPhysics;

    // the list of exclusive rotation speeds (to face movement) for rotation above specified set of angles only (in degrees)
    [Tooltip("the list of exclusive rotation speeds (to face movement) for rotation above specified set of angles only (in degrees)")]
    public List<ExclusiveRotationSpeedForAngle> ExclusiveRotateToFaceMovementDirectionSpeeds = new();

    public List<ExclusiveRotationSpeedForAngle> ListRotationsOnIncrease = new();


    //// the threshold angle (in degrees) above which a separate speed was used to rotate the model
    //[Tooltip("the threshold angle (in degrees) above which a separate speed was used to rotate the model")]
    //public float ThresholdAngle = 50f;

    //// the exclusive rotation speed (to face movement) only applied for rotation above the threshold angle
    //[Tooltip("the exclusive rotation speed only applied for rotation above the threshold angle")]
    //public float ExclusiveRotateToFaceMovementDirectionSpeed = 0.4f;

    internal ExclusiveRotationSpeedForAngleComparer _comparer;
    protected float _previousAngle = 0f;


    protected override void Initialization()
    {
        base.Initialization();

        _comparer = new();
        ExclusiveRotateToFaceMovementDirectionSpeeds.Sort(_comparer);
        ListRotationsOnIncrease.Sort(_comparer);
    }

    protected override void RotateToFaceMovementDirection()
    {
        base.RotateToFaceMovementDirection();

        if (!IsAppliedShipPhysics)
        {
            return;
        }

        float angle = 0f;

        if (MovementRotationSpeed == RotationSpeeds.Smooth)
        {
            if (_currentDirection != Vector3.zero)
            {
                angle = Quaternion.Angle(MovementRotatingModel.transform.rotation, _tmpRotation);
            }
        }

        if (MovementRotationSpeed == RotationSpeeds.SmoothAbsolute)
        {
            if (_lastMovement != Vector3.zero)
            {
                angle = Quaternion.Angle(MovementRotatingModel.transform.rotation, _tmpRotation);
            }
        }

        //if (angle > ThresholdAngle)
        //{
        //    _newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * ExclusiveRotateToFaceMovementDirectionSpeed);
        //}

        if (_previousAngle < angle && !Mathf.Approximately(angle, 0f))
        {
            // increase
            for (int i = 0, count = ListRotationsOnIncrease.Count; i < count; ++i)
            {
                var rotationSpeedForAngle = ListRotationsOnIncrease[i];

                if (angle <= rotationSpeedForAngle.ThresholdAngle)
                {
                    Debug.Log($"[Increase] Applied: {angle} with threshold: {rotationSpeedForAngle.ThresholdAngle}");

                    var speed = rotationSpeedForAngle.RotationSpeed + rotationSpeedForAngle.RotationAcceleration * Time.deltaTime;
                    speed = Mathf.Clamp(speed, 0, speed);

                    _newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, speed * Time.deltaTime);

                    break;
                }
            }
        }
        else if (_previousAngle >= angle && !Mathf.Approximately(angle, 0f))
        {
            // decrease
            for (int i = ExclusiveRotateToFaceMovementDirectionSpeeds.Count - 1; i >= 0; --i)
            {
                var rotationSpeedForAngle = ExclusiveRotateToFaceMovementDirectionSpeeds[i];

                if (angle >= rotationSpeedForAngle.ThresholdAngle)
                {
                    Debug.Log($"[Decrease] Applied: {angle} with threshold: {rotationSpeedForAngle.ThresholdAngle}");

                    var speed = rotationSpeedForAngle.RotationSpeed + rotationSpeedForAngle.RotationAcceleration * Time.deltaTime;
                    speed = Mathf.Clamp(speed, 0, speed);

                    _newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, speed * Time.deltaTime);

                    break;
                }
            }
        }

        _previousAngle = angle;
    }
}

internal class ExclusiveRotationSpeedForAngleComparer : IComparer<ExclusiveRotationSpeedForAngle>
{
    public int Compare(ExclusiveRotationSpeedForAngle x, ExclusiveRotationSpeedForAngle y)
    {
        return x.ThresholdAngle.CompareTo(y.ThresholdAngle);
    }
}
