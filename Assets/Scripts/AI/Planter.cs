using System;
using AI.Behaviour;
using DefaultNamespace.AI.PathFinding;
using Simulation;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using VectorFieldPhysics;
using Random = UnityEngine.Random;

namespace AI {
public class Planter : Agent {
	
	[SerializeField] private GameObject treeSpawnPrefab;
	[SerializeField] private float plantInterval = 10;
	
	[SerializeField] private Tornado tornado;
	
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
			new Wander(this, 0.25f)
		);
	}
	
	private void PlantTree() {
		GameObject treeSpawn = Instantiate(treeSpawnPrefab, transform.position, Quaternion.identity);
		treeSpawn.GetComponent<TreeSpawn>().InitFromScript((int)(Random.value * int.MaxValue), tornado, influenceGrid);
		nextPlantTime = Time.time + plantInterval;
	}
}
}