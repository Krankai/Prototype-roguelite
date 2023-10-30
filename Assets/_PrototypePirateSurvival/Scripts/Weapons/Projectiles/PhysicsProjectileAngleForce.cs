using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class PhysicsProjectileAngleForce : PhysicsProjectile
{
    public override void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
    {
        if (InitialForce is float.NaN || newDirection.x is float.NaN || newDirection.y is float.NaN || newDirection.z is float.NaN)
        {
            base.SetDirection(newDirection, newRotation, spawnerIsFacingRight);
            return;
        }

        _spawnerIsFacingRight = spawnerIsFacingRight;

        if (DirectionCanBeChangedBySpawner)
        {
            Direction = newDirection;
        }
        if (ProjectileIsFacingRight != spawnerIsFacingRight)
        {
            Flip();
        }
        if (FaceDirection)
        {
            transform.rotation = newRotation;
        }

        if (_damageOnTouch != null)
        {
            _damageOnTouch.SetKnockbackScriptDirection(newDirection);
        }

        if (FaceMovement)
        {
            switch (MovementVector)
            {
                case MovementVectors.Forward:
                    transform.forward = newDirection;
                    break;
                case MovementVectors.Right:
                    transform.right = newDirection;
                    break;
                case MovementVectors.Up:
                    transform.up = newDirection;
                    break;
            }
        }

        this.transform.Rotate(InitialRotation, Space.Self);

        // To kill remaining velocity
        SetRigidbody(true);
        SetRigidbody(false);

        if (_rigidBody != null)
        {
            _rigidBody.AddForce(newDirection * InitialForce, InitialForceMode);
        }

        if (_rigidBody2D != null)
        {
            _rigidBody2D.AddForce(newDirection * InitialForce, InitialForceMode2D);
        }
    }
}
