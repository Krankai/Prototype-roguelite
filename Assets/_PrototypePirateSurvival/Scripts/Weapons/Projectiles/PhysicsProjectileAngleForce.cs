using MoreMountains.TopDownEngine;
using UnityEngine;

public class PhysicsProjectileAngleForce : PhysicsProjectile
{
    public override void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
    {
		base.SetDirection(newDirection, newRotation, spawnerIsFacingRight);

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
