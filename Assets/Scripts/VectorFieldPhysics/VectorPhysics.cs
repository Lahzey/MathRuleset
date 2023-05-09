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

	private double dragConstMods; // just a collection of all modifiers in the drag equation that don't change

	private void OnValidate() {
		if (dragCoefficient < 0) dragCoefficient = 0;

		dragConstMods = -0.5f * AIR_DENSITY * dragCoefficient * areaM2 / weightKg;
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
		
		// air drag using integral of drag equation
		Debug.DrawLine(transform.position, transform.position + airSpeed * 10000, Color.red);
		Vector3 relativeVelocity = velocity - airSpeed;
		float relativeMagnitude = relativeVelocity.magnitude;
		float newRelativeMagnitude = (float) (-1 / (dragConstMods * time - 1 / relativeMagnitude));
		velocity = airSpeed + newRelativeMagnitude * (relativeVelocity / relativeMagnitude);
		Debug.Log($"{PV(relativeVelocity)}[{relativeMagnitude}] -> {PV(relativeVelocity / relativeMagnitude)} * {newRelativeMagnitude} = {PV(newRelativeMagnitude * (relativeVelocity / relativeMagnitude))}[{(newRelativeMagnitude * (relativeVelocity / relativeMagnitude)).magnitude}]");
		
		// prevent really small numbers, just stop the object
		if (velocity.x < 0.0001f && velocity.x > -0.0001f) velocity.x = 0;
		if (velocity.y < 0.0001f && velocity.y > -0.0001f) velocity.y = 0;
		if (velocity.z < 0.0001f && velocity.z > -0.0001f) velocity.z = 0;
		
		// move
		transform.position += velocity * time;
		if (transform.position.y < 0) {
			transform.position = new Vector3(transform.position.x, 0, transform.position.z);
			velocity.y = 0;
		}
	}
	
	private string PV(Vector3 v) => $"({v.x}, {v.y}, {v.z})";
}
}