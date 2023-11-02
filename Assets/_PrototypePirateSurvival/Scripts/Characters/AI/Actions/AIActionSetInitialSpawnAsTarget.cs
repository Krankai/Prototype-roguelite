using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIActionSetInitialSpawnAsTarget : AIAction
{
    // the transform that will be set to initial position
    [Tooltip("the transform that will be set to initial position")]
    public Transform _initialTransform;

    protected Vector3 _initialPosition;


    public override void Initialization()
    {
        base.Initialization();

        Invoke(nameof(SetIntialPosition), 1f);
    }

    private void SetIntialPosition()
    {
        _initialPosition = transform.position;
    }

    public override void PerformAction()
    {
        _initialTransform.position = _initialPosition;
        _brain.Target = _initialTransform;
    }
}
