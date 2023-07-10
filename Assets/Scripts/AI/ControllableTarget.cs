using System;
using DefaultNamespace;
using DefaultNamespace.AI.PathFinding;
using UnityEngine;

namespace AI {
public class ControllableTarget : MonoBehaviour {
	
	[SerializeField] private InfluenceGrid influenceGrid;
	[SerializeField] private float moveSpeed = 5;

	private void Update() {
		Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		transform.position += movement.To3D() * (moveSpeed * Time.deltaTime);

		influenceGrid.SetInfluence(transform.position, InfluenceType.TARGET, 1);
	}
}
}