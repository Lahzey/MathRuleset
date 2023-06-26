using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace.AI {
public class SteeringBehaviour {
	
	private static Vector3 Seek(Transform self, Vector3 target, float maxSpeed, float maxForce) {
		Vector3 desiredVelocity = (target - self.position).normalized * maxSpeed;
		Vector3 steering = desiredVelocity - self.forward * maxSpeed; // steering gets harder the faster you go
		return Vector3.ClampMagnitude(steering, maxForce);
	}

	public static Func<Transform, Vector3> Seek(Transform target, float maxSpeed, float maxForce) {
		return self => Seek(self, target.position, maxSpeed, maxForce);
	}

	private static Vector3 Flee(Transform self, Vector3 target, float maxSpeed, float maxForce) {
		Vector3 desiredVelocity = (self.position - target).normalized * maxSpeed;
		Vector3 steering = desiredVelocity - self.forward * maxSpeed;
		return Vector3.ClampMagnitude(steering, maxForce);
	}

	public static Func<Transform, Vector3> Flee(Transform target, float maxSpeed, float maxForce) {
		return self => Flee(self, target.position, maxSpeed, maxForce);
	}

	public static Func<Transform, Vector3> Arrive(Transform target, float maxSpeed, float maxForce, float slowingDistance) {
		float slowingDistanceSquared = slowingDistance * slowingDistance;
		return self => {
			Vector3 difference = target.position - self.position;
			Vector3 desiredVelocity = difference.normalized * maxSpeed;
			if (difference.sqrMagnitude < slowingDistanceSquared) { // doing the check with squared magnitudes saves performance
				desiredVelocity *= difference.magnitude / slowingDistance;
			}
			
			Vector3 steering = desiredVelocity - self.forward * maxSpeed;
			return Vector3.ClampMagnitude(steering, maxForce);
		};
	}

	public static Func<Transform, Vector3> Pursue(Transform target, float maxSpeed, float maxForce) {
		return self => {
			Vector3 difference = target.position - self.position;

			// check angle between difference and target forward and only calculate future position if the target is not moving towards us
			float dot = Vector3.Dot(difference.normalized, target.forward);
			if (dot < -0.7f) return Seek(self, target.position, maxSpeed, maxForce);

			float distance = difference.magnitude;
			float time = distance / maxSpeed; // the time it would take to reach the target at max speed
			Vector3 futurePosition = target.position + target.forward * maxSpeed * time; // assuming the target moves at the pursuers max speed
			return Seek(self, futurePosition, maxSpeed, maxForce);
		};
	}
	
	// TODO: fix or delete
	// public static Func<Vector3> Evade(Transform target, float maxSpeed, float maxForce) {
	// 	return self => {
	// 		Vector3 difference = target.position - self.position;
	// 		float distance = difference.magnitude;
	// 		float time = distance / maxSpeed; // the time it would take to reach the target at max speed
	// 		Vector3 futurePosition = target.position + target.forward * maxSpeed * time; // assuming the target moves at the pursuers max speed
	// 		return Flee(self.position, futurePosition, maxSpeed, maxForce);
	// 	};
	// }
	
	public static Func<Transform, Vector3> Wander(float maxSpeed, float maxForce, float wanderRadius, float wanderJitter) {
		Vector3 origin = Vector3.zero;
		float circlePos = Random.value;
		return self => {
			if (origin == Vector3.zero) origin = self.position; // a bit hacky, maybe find a better way to initialize origin
			
			circlePos += (Random.value * 2 - 1) * wanderJitter;
			while (circlePos < 0) circlePos += 1000; // prevent negative values because C# modulo is not a modulo but a remainder
			circlePos %= 1;
			
			// get position on unit circle at angle circlePos (https://math.stackexchange.com/questions/260096/find-the-coordinates-of-a-point-on-a-circle)
			Vector3 circlePosition = new Vector3(Mathf.Sin(circlePos * 360), 0, Mathf.Cos(circlePos * 360));
			
			Vector3 wanderTarget = origin + circlePosition * wanderRadius;
			return Seek(self, wanderTarget, maxSpeed, maxForce);
		};
	}
}
}