namespace AI.BehaviourTree {
public abstract class CompositeNode : Node {
	public override void Add(Node child) {
		Children.Add(child);
	}
}
}