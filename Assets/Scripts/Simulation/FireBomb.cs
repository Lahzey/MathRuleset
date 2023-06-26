using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using VectorFieldPhysics;

namespace Simulation {
public class FireBomb : MonoBehaviour {

	[SerializeField] private float radius;
	[SerializeField] private float scorchMarkRadius;
	
	[SerializeField] private GameObject scorchMarkPrefab;

	[SerializeField] private Tornado tornado;
	[SerializeField] private Transform floor;


	private void FixedUpdate() {
		CheckTornadoInteraction();
		CheckFloorInteraction();
	}

	private void CheckFloorInteraction() {
		Plane plane = new Plane(floor.rotation * Vector3.up, floor.position);
		if (plane.GetSide(transform.position)) return;
		
		// create scorch mark decal
		Vector3 scorchedPos = plane.ClosestPointOnPlane(transform.position);
		GameObject scorchMark = Instantiate(scorchMarkPrefab, scorchedPos, Quaternion.identity);
		scorchMark.transform.localScale = Vector3.one * scorchMarkRadius;
		Destroy(gameObject);
	}

	private void CheckTornadoInteraction() {
		if (tornado == null) return;

		// check if current position is inside tornado (and return if not)
		Vector2 tornadoPos = new Vector2(tornado.transform.position.x, tornado.transform.position.z);
		Vector2 pos = new Vector2(transform.position.x, transform.position.z);
		float distSqr = (tornadoPos - pos).sqrMagnitude;
		float tornadoRadiusSqr = tornado.GetRadius(transform.position.y);
		tornadoRadiusSqr *= tornadoRadiusSqr;
		if (!(tornadoRadiusSqr > distSqr)) return;
		
		// copy colors from this vfx to tornado vfx
		VisualEffect tornadoVfx = tornado.GetComponent<VisualEffect>();
		VisualEffect thisVfx = GetComponent<VisualEffect>();
		tornadoVfx.SetGradient("Colors", thisVfx.GetGradient("Colors"));
		tornado = null; // no need to keep checking now, might remove later if tornado can be reset
	}
	
	private void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, radius);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, scorchMarkRadius);
	}
}
}