using System;
using AI.Behaviour;
using DefaultNamespace.AI.PathFinding;
using UnityEngine;

namespace AI {
public class Seeker : Agent {

	[SerializeField] private uint sensitivity = 10;
	[SerializeField] private float energy = 1f;

	private float minInfluenceValue;


	private void Awake() {
		minInfluenceValue = Mathf.Pow(influenceGrid.InfluenceDecay, sensitivity); // calculates the expected value at a cell after sensitivity steps away from a 1

		float energyTransferRange = Mathf.Pow(influenceGrid.InfluenceDecay, 1);

		BehaviourTree.Add(
			// refill energy
			new Condition(() => influenceGrid.GetInfluence(transform.position, InfluenceType.ENERGY) >= energyTransferRange).Add(
				new DynamicNode(() => {
					energy += Time.deltaTime * 0.3f;
					if (energy > 1f) energy = 1f;
					return energy >= 1f ? NodeState.FAILURE : NodeState.SUCCESS; // while energy is not full, keep refilling (success does not execute other actions), otherwise continue (failure executes other actions)
				})
			)
		).Add(
			// flee from danger
			new Condition(() => influenceGrid.GetInfluence(transform.position, InfluenceType.DANGER) > 0.1f).Add(
				new GoTo(this, () => influenceGrid.GetInfluenceDirection(transform.position, InfluenceType.DANGER, false))
			)
		).Add(
			// seek energy
			new Condition(() => energy <= 0f && influenceGrid.GetInfluence(transform.position, InfluenceType.ENERGY) > 0f).Add(
				new GoTo(this, () => influenceGrid.GetInfluenceDirection(transform.position, InfluenceType.ENERGY, true))
			)
		).Add(
			// seek target
			new Condition(() => influenceGrid.GetInfluence(transform.position, InfluenceType.TARGET) >= minInfluenceValue).Add(
				new GoTo(this, () => influenceGrid.GetInfluenceDirection(transform.position, InfluenceType.TARGET, true))
			)
		).Add(
			// wander
			new Wander(this, 0.5f)
		);
	}

	public override bool MoveTo(Vector3 target) {
		if (!base.MoveTo(target)) return false;
		energy -= Time.deltaTime / 10f;
		return true;
	}
}
}