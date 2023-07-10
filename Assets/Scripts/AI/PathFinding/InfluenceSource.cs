using System;
using UnityEngine;

namespace DefaultNamespace.AI.PathFinding {
public class InfluenceSource : MonoBehaviour {
	
	[SerializeField] private InfluenceGrid influenceGrid;
	[SerializeField] private InfluenceType influenceType;
	[SerializeField] private float influence = 1;
	[SerializeField] private Vector2 size = Vector2.one;

	private void Update() {
		for (float x = 0; x <= size.x; x += influenceGrid.Scale) {
			for (float y = 0; y <= size.x; y += influenceGrid.Scale) {
				Vector3 position = transform.position + new Vector3(x, 0, y);
				influenceGrid.SetInfluence(position, influenceType, influence);
			}
		}
	}
}
}