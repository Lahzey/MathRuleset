using System;

namespace AI.Behaviour {
public class Condition : DecoratorNode {

	private readonly Func<bool> condition;
	
	public Condition(Func<bool> condition) {
		this.condition = condition;
	}
	
	protected override NodeState EvaluateImpl() {
		return condition() ? Child.Evaluate() : NodeState.FAILURE;
	}
}
}