using UnityEngine;

namespace AI.Behaviour {
public class Wander : Node {
	
	private readonly Agent agent;
	private readonly float jitter;

	private float angle;
	
	public Wander(Agent agent, float jitter) {
		this.agent = agent;
		this.jitter = jitter;
		
		OnExit += () => angle = Random.value;
	}
	
	protected override NodeState EvaluateImpl() {
		angle += Time.time * jitter;
		Vector3 target = agent.transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
		agent.MoveTo(target);
		return NodeState.RUNNING;
	}
}
}