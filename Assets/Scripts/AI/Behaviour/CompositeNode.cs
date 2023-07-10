namespace AI.Behaviour {
public abstract class CompositeNode : Node {
	public override Node Add(Node child) {
		Children.Add(child);
		return this;
	}
}
}