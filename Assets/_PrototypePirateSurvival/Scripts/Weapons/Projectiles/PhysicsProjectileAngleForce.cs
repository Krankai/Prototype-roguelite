using MoreMountains.Tools;
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

		// TEST
		// x = (v0^2 * sin(2a)) / g
		var initialVelocity = InitialForce * newDirection.y / _rigidBody.mass;
		Debug.Log($"Velocity: {initialVelocity}");

		var weaponAim = _weapon.gameObject.MMGetComponentNoAlloc<WeaponAim3D>();
		if (weaponAim != default)
        {
			var angle = Vector3.Angle(weaponAim.CurrentAim, newDirection);
			Debug.Log($"Angle: {angle}");		// WRONG!!!
        }
	}
}
