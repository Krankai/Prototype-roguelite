using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDecisionRotationAligned : AIDecision
{
    public enum TargetDirection { Vertical, Horizontal }

    // the direction of target to compared to
    [Tooltip("the direction of target to compared to")]
    public TargetDirection Direction = TargetDirection.Vertical;
    // the threshold to decide if rotations are aligned
    [Tooltip("the threshold to decide if rotations are aligned (in degrees)")]
    public float ThresholdAngle = 10f;

    protected CharacterOrientation3D _characterOrientation3D;


    public override void Initialization()
    {
        base.Initialization();

        var character = gameObject.GetComponentInParent<Character>();
        if (character != default)
        {
            _characterOrientation3D = character.FindAbility<CharacterOrientation3D>();
        }
    }

    public override bool Decide()
    {
        if (Direction == TargetDirection.Vertical)
        {
            var angle = Vector3.Angle(_characterOrientation3D.ModelDirection, _brain.Target.transform.forward);
            Debug.Log($"Rotation: {angle}");
            return (angle <= ThresholdAngle || (180 - angle) <= ThresholdAngle);
        }
        else
        {
            var angle = Vector3.Angle(_characterOrientation3D.ModelDirection, _brain.Target.transform.right);
            return (angle <= ThresholdAngle || (180 - angle) <= ThresholdAngle);
        }
    }
}
