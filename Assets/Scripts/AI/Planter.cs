using System;
using AI.Behaviour;
using DefaultNamespace.AI.PathFinding;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

namespace AI {
public class Planter : Agent {
	
	[SerializeField] private GameObject treeSpawnPrefab;
	[SerializeField] private float plantInterval = 10;
	
	private float nextPlantTime;

	private void Awake() {
		nextPlantTime = Time.time + plantInterval;

		BehaviourTree.Add(
			// flee from danger
			new Condition(() => influenceGrid.GetInfluence(transform.position, InfluenceType.DANGER) > 0.1f).Add(
				new GoTo(this, () => influenceGrid.GetInfluenceDirection(transform.position, InfluenceType.DANGER, false))
			)
		).Add(
			// plant trees
			new Condition(() => Time.time >= nextPlantTime).Add(
				new DynamicNode(() => {
					PlantTree();
					return NodeState.SUCCESS;
				})
			)
		).Add(
			// wander
			new Wander(this, 45)
		);
	}
	
	private void PlantTree() {
		Instantiate(treeSpawnPrefab, transform.position, Quaternion.identity);
		nextPlantTime = Time.time + plantInterval;
	}
}
}