using System;
using AI.Behaviour;
using DefaultNamespace.AI.PathFinding;
using UnityEngine;

namespace AI {
public class Seeker : Agent {

	[SerializeField] private uint sensitivity = 10;

	private float minInfluenceValue;


	private void Awake() {
		minInfluenceValue = Mathf.Pow(influenceGrid.InfluenceDecay, sensitivity); // calculates the expected value at a cell after sensitivity steps away from a 1

		BehaviourTree.Add(
			// flee from danger
			new Condition(() => influenceGrid.GetInfluence(transform.position, InfluenceType.DANGER) > 0.1f).Add(
				new GoTo(this, () => influenceGrid.GetInfluenceDirection(transform.position, InfluenceType.DANGER, false))
			)
		).Add(
			// seek target
			new Condition(() => influenceGrid.GetInfluence(transform.position, InfluenceType.TARGET) >= minInfluenceValue).Add(
				new GoTo(this, () => influenceGrid.GetInfluenceDirection(transform.position, InfluenceType.TARGET, true))
			)
		).Add(
			// wander
			new Wander(this, 45)
		);
	}
}
}