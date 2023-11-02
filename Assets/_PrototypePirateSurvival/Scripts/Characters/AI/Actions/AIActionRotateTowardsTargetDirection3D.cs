using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIActionRotateTowardsTargetDirection3D : AIAction
{
    public enum TargetDirection { Vertical, Horizontal }

    // the direction of target to rotate to
    [Tooltip("the direction of target to rotate to")]
    public TargetDirection Direction = TargetDirection.Vertical;
    // the maximum offset randomized rotation to apply
    [Tooltip("the maximum offset randomized rotation (around Y axis) to apply (in degrees)")]
    public float MaxOffsetRotationY = 10f;

    [SerializeField, MMReadOnly]
    protected float _randomizedRotationY = 0f;

    protected CharacterOrientation3D _characterOrientation3D;
    protected bool _cachedForcedRotation;


    public override void Initialization()
    {
        if (!ShouldInitialize)
        {
            return;
        }

        base.Initialization();

        var character = gameObject.GetComponentInParent<Character>();
        if (character != default)
        {
            _characterOrientation3D = character.FindAbility<CharacterOrientation3D>();
        }
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (_characterOrientation3D != default)
        {
            _cachedForcedRotation = _characterOrientation3D.ForcedRotation;
            _characterOrientation3D.ForcedRotation = true;
        }
    }

    public override void OnExitState()
    {
        base.OnExitState();

        if (_characterOrientation3D != default)
        {
            _characterOrientation3D.ForcedRotation = _cachedForcedRotation;
        }
    }

    public override void PerformAction()
    {
        RotateTowardsDirection();
    }

    protected virtual void RotateTowardsDirection()
    {
        if (_brain.Target == default)
        {
            return;
        }

        Vector3 targetDirection;
        switch (Direction)
        {
            case TargetDirection.Horizontal:
                {
                    //var angleRight = Vector3.Angle(_characterOrientation3D.CurrentDirection, _brain.Target.transform.right);
                    //var angleLeft = Vector3.Angle(_characterOrientation3D.CurrentDirection, -_brain.Target.transform.right);

                    var angleRight = Vector3.SignedAngle(_characterOrientation3D.ModelDirection, _brain.Target.transform.right, transform.up);
                    var angleLeft = Vector3.SignedAngle(_characterOrientation3D.ModelDirection, -_brain.Target.transform.right, transform.up);

                    targetDirection = Mathf.Abs(angleRight) < Mathf.Abs(angleLeft)
                        ? _brain.Target.transform.right
                        : -_brain.Target.transform.right;

                    // DEBUG
                    string color = Mathf.Abs(angleRight) < Mathf.Abs(angleLeft) ? "green" : "blue";
                    MMDebug.LogDebugToConsole($"Target: {targetDirection}", color, 3, true);

                    if (Mathf.Abs(angleRight) < Mathf.Abs(angleLeft))
                    {
                        Debug.LogError($"Target: {targetDirection}");
                    }
                    else
                    {
                        Debug.LogWarning($"Target: {targetDirection}");
                    }
                }
                break;
            case TargetDirection.Vertical:
            default:
                {
                    //var angleForward = Vector3.Angle(_characterOrientation3D.CurrentDirection, _brain.Target.transform.forward);
                    //var angleBackward = Vector3.Angle(_characterOrientation3D.CurrentDirection, -_brain.Target.transform.forward);

                    var angleForward = Vector3.SignedAngle(_characterOrientation3D.ModelDirection, _brain.Target.transform.forward, transform.up);
                    var angleBackward = Vector3.SignedAngle(_characterOrientation3D.ModelDirection, -_brain.Target.transform.forward, transform.up);

                    targetDirection = Mathf.Abs(angleForward) < Mathf.Abs(angleBackward)
                        ? _brain.Target.transform.forward
                        : -_brain.Target.transform.forward;

                    // DEBUG
                    //string color = Mathf.Abs(angleForward) < Mathf.Abs(angleBackward) ? "green" : "blue";
                    //MMDebug.LogDebugToConsole($"Target: {targetDirection}", color, 3, true);

                    //if (Mathf.Abs(angleForward) < Mathf.Abs(angleBackward))
                    //{
                    //    Debug.LogError($"Forward: {angleForward}, Backward: {angleBackward}");
                    //}
                    //else
                    //{
                    //    Debug.LogWarning($"Forward: {angleForward}, Backward: {angleBackward}");
                    //}
                }
                break;
        }

        _randomizedRotationY = UnityEngine.Random.Range(-MaxOffsetRotationY, MaxOffsetRotationY);
        var rotation = Quaternion.AngleAxis(_randomizedRotationY, transform.up);

        _characterOrientation3D.ForcedRotationDirection = rotation * targetDirection;
    }
}
