using UnityEngine;

namespace AI.Behaviour {
public class Wander : Node {
	
	private readonly Agent agent;
	private readonly float jitter;

	private float time;
	
	public Wander(Agent agent, float jitter) {
		this.agent = agent;
		this.jitter = jitter;
	}
	
	protected override NodeState EvaluateImpl() {
		bool repeat = true;
		
		int tries = 0;
		while (repeat) {
			if (tries > 10) return NodeState.FAILURE;
			
			time += Time.deltaTime * jitter;
			float angle = Noise1D(time) * 2f * Mathf.PI;
			Vector3 target = agent.transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
			repeat = !agent.MoveTo(target);
			tries++;
		}
		
		return NodeState.RUNNING;
	}
	
	// copied from https://stackoverflow.com/questions/8798771/perlin-noise-for-1d
	private static float Noise1D(float x) {
		return (Mathf.Sin(2 * x) + Mathf.Sin(Mathf.PI * x)) * 0.25f + 0.5f;
	}
}
}