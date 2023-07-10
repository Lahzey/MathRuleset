using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace.AI.PathFinding {
public class InfluenceSource : MonoBehaviour {
	
	[SerializeField] public InfluenceGrid influenceGrid;
	[SerializeField] public InfluenceType influenceType;
	[SerializeField] public float radius = 1;

	private void Update() {
		influenceGrid.SetInfluence(transform.position, influenceType, 1);
		
		float scale = influenceGrid.Scale;
		float halfScale = scale / 2;

		float minX = transform.position.x - radius;
		minX -= (minX % scale);
		float maxX = transform.position.x + radius;
		maxX -= (maxX % scale) - scale;
		float minY = transform.position.z - radius;
		minY -= (minY % scale);
		float maxY = transform.position.z + radius;
		maxY -= (maxY % scale) - scale;
		for (float x = minX; x <= maxX; x += scale) {
			for (float y = minY; y <= maxY; y += scale) {
				Vector2 pos = new Vector2(x, y);
				if (CircleIntersectsRect(transform.position.To2D(), radius, new Rect(x + halfScale, y + halfScale, scale, scale))) {
					influenceGrid.SetInfluence(pos.To3D(), influenceType, 1);
				}
			}
		}
	}
	
	// copied from https://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection
	private bool CircleIntersectsRect(Vector2 circlePos, float circleRadius, Rect rect)
	{
		Vector2 circleDistance;
		circleDistance.x = Math.Abs(circlePos.x - rect.x);
		circleDistance.y = Math.Abs(circlePos.y - rect.y);

		if (circleDistance.x > (rect.width/2 + circleRadius)) { return false; }
		if (circleDistance.y > (rect.height/2 + circleRadius)) { return false; }

		if (circleDistance.x <= (rect.width/2)) { return true; } 
		if (circleDistance.y <= (rect.height/2)) { return true; }

		float cornerDistance_sq = Mathf.Pow(circleDistance.x - rect.width/2, 2) + Mathf.Pow(circleDistance.y - rect.height/2, 2);

		return cornerDistance_sq <= circleRadius * circleRadius;
	}
	
	private void OnDrawGizmos() {
		Handles.color = influenceType.GetColor();
		Handles.DrawWireDisc(transform.position, Vector3.up, radius);
	}
}
}