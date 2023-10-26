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
		// Rough estimate: since start at initial height h0, the actual distance will be longer
		// x = (v0^2 * sin(2a)) / g
		var initialVelocity = (InitialForce / _rigidBody.mass) * Time.fixedDeltaTime;
		Debug.Log($"Velocity: {initialVelocity}");

		var tempDirection = newDirection;
		var projectedDirection = tempDirection;
		projectedDirection.y = 0;

		var angle = Vector3.Angle(tempDirection, projectedDirection);
		Debug.Log($"Angle: {angle}");

		var gravity = Physics.gravity;
		var g = Physics.gravity.magnitude;
		Debug.Log($"g: {gravity}");

		var maxHeightDelta = initialVelocity * initialVelocity / (2 * g);
		Debug.Log($"Max height: {maxHeightDelta}");

		var maxDistanceDelta = initialVelocity * initialVelocity * Mathf.Sin(2 * Mathf.Deg2Rad * angle) / g;
		Debug.Log($"Max distance: {maxDistanceDelta}");

		var initialDistanceDelta = (transform.position.y + 0.5) / Mathf.Tan(Mathf.Deg2Rad * angle);
		Debug.Log($"Initial distance: {initialDistanceDelta}");

		var finalRadius = 0.6f + maxDistanceDelta + initialDistanceDelta;
		Debug.Log($"Final redius: {finalRadius}");


		var fireZone = GameObject.Find("FireZone");
		if (fireZone != default)
		{
			var circle = fireZone.GetComponentInChildren<SpriteRenderer>();
			Debug.Log($"Bounds: {circle.bounds.extents}");
		}
	}

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

		Debug.Log($"Position: {transform.position}");
    }
}
