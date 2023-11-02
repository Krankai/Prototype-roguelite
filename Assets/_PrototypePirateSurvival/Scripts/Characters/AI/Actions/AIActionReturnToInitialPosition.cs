using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIActionReturnToInitialPosition : AIAction
{
    protected CharacterMovement _characterMovement;
    protected Vector3 _initialPosition;
    protected Vector3 _directionToTarget;
    protected Vector2 _movementVector;


    public override void Initialization()
    {
        base.Initialization();

        Invoke(nameof(SetIntialPosition), 1f);

        var character = gameObject.GetComponentInParent<Character>();
        if (character != default)
        {
            _characterMovement = character.FindAbility<CharacterMovement>();
        }
    }

    private void SetIntialPosition()
    {
        //var character = gameObject.GetComponentInParent<Character>();
        //if (character == default)
        //{
        //    _intialPosition = transform.position;
        //    return;
        //}

        //_intialPosition = character.transform.position;

        _initialPosition = transform.position;
    }

    public override void PerformAction()
    {
        MoveToInitialPosition();
    }

    protected virtual void MoveToInitialPosition()
    {
        if (_characterMovement == default)
        {
            return;
        }

        _directionToTarget = _initialPosition - transform.position;

        _movementVector.x = _directionToTarget.x;
        _movementVector.y = _directionToTarget.z;

        _characterMovement.SetMovement(_movementVector);

        if (Mathf.Approximately(transform.position.x, _initialPosition.x))
        {
            _characterMovement.SetHorizontalMovement(0f);
        }

        if (Mathf.Approximately(transform.position.y, _initialPosition.y))
        {
            _characterMovement.SetVerticalMovement(0f);
        }
    }

    public override void OnExitState()
    {
        base.OnExitState();

        if (_characterMovement != default)
        {
            _characterMovement.SetHorizontalMovement(0f);
            _characterMovement.SetVerticalMovement(0f);
        }
    }
}
