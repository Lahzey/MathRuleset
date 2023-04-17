using System;
using System.Collections.Generic;
using UnityEngine;

namespace VectorFieldPhysics {
public class VectorPhysics : MonoBehaviour {
	
	private static readonly Vector3 GRAVITY = new Vector3(0, -9.81f, 0);
	private static readonly double AIR_DENSITY = 1.293f;

	[SerializeField] public List<MonoBehaviour> vectorFields = new List<MonoBehaviour>();
	[SerializeField] public double dragCoefficient = 0.5f;
	[SerializeField] public double weightKg = 1f;
	[SerializeField] public double areaM2 = 1f;
	[SerializeField] public bool useGravity = true;
	[SerializeField] public Vector3 velocity;

	private void OnValidate() {
		if (dragCoefficient < 0) dragCoefficient = 0;
	}

	private void FixedUpdate() {
		float time = Time.fixedDeltaTime;
		
		// gravity
		if (useGravity) velocity += GRAVITY * time;
		
		// vector fields
		Vector3 airSpeed = Vector3.zero;
		foreach (MonoBehaviour vectorField in vectorFields) {
			if (vectorField is VectorField field) {
				airSpeed += field.GetVector(transform.position);
			}
		}
		
		// air drag
		Debug.DrawLine(transform.position, transform.position + airSpeed * 10000, Color.red);
		Vector3 relativeVelocity = velocity - airSpeed;
		double airDragPercent = 0.5f * AIR_DENSITY * relativeVelocity.sqrMagnitude * dragCoefficient * areaM2 * time / weightKg;
		if (airDragPercent > 1) airDragPercent = 1;
		velocity -= (float) airDragPercent * relativeVelocity.normalized;
		
		// prevent really small numbers, just stop the object
		if (velocity.x < 0.0001f && velocity.x > -0.0001f) velocity.x = 0;
		if (velocity.y < 0.0001f && velocity.y > -0.0001f) velocity.y = 0;
		if (velocity.z < 0.0001f && velocity.z > -0.0001f) velocity.z = 0;
		
		// move
		transform.position += velocity * time;
	}
}
}