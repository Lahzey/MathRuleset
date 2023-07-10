using JetBrains.Annotations;

namespace AI.Behaviour {
public abstract class DecoratorNode : Node {
	
	[CanBeNull] protected Node Child => Children.Count > 0 ? Children[0] : null;
	
	public override Node Add(Node child) {
		if (Children.Count == 0) Children.Add(child);
		else Children[0] = child;
		return this;
	}
}
}