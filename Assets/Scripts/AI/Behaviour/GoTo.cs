using System;
using UnityEngine;

namespace AI.Behaviour {
public class GoTo : Node {
	
	private readonly Agent agent;
	private readonly Func<Vector3> target;
	
	public GoTo(Agent agent, Func<Vector3> target) {
		this.agent = agent;
		this.target = target;
	}
	
	protected override NodeState EvaluateImpl() {
		Vector3 target = this.target();
		if ((agent.transform.position - target).sqrMagnitude < 0.1f) return NodeState.SUCCESS;
		if (!agent.MoveTo(target)) return NodeState.FAILURE;
		return NodeState.RUNNING;
	}
}
}