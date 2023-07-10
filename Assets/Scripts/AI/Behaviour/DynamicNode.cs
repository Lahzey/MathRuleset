using System;

namespace AI.Behaviour {
public class DynamicNode : Node {
	
	private readonly Func<NodeState> action;

	public DynamicNode(Func<NodeState> action) {
		this.action = action;
	}
	
	protected override NodeState EvaluateImpl() {
		return action?.Invoke() ?? NodeState.FAILURE;
	}
}
}