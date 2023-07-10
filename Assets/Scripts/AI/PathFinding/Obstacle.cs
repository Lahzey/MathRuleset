using System;
using UnityEngine;

namespace DefaultNamespace.AI.PathFinding {
public class Obstacle : MonoBehaviour {

	[SerializeField] public InfluenceGrid influenceGrid;
	[SerializeField] public Vector2 bounds = Vector2.one;

	private void Start() {
		if (influenceGrid.Scale <= 0) {
			throw new Exception("InfluenceGrid scale must be greater than 0");
		}
		
		for (float x = 0; x <= bounds.x; x += influenceGrid.Scale / transform.lossyScale.x) {
			for (float y = 0; y <= bounds.x; y += influenceGrid.Scale / transform.lossyScale.z) {
				Vector3 position = transform.TransformPoint(new Vector3(x- 0.5f, 0, y - 0.5f));
				influenceGrid.SetObstacle(position, true);
			}
		}
	}
}
}