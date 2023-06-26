namespace AI.BehaviourTree {
public class Selector : CompositeNode {
	
	public bool AllowInterruption { get; set; }
	private int runningIndex = 0;
	
	public Selector(bool allowInterruption = false) {
		AllowInterruption = allowInterruption;
		OnExit += () => runningIndex = 0; // we want to forget the last running index if the sequencer itself is interrupted
	}
	
	protected override NodeState EvaluateImpl() {
		int startIndex = AllowInterruption ? 0 : runningIndex;
		for (int i = startIndex; i < Children.Count; i++) {
			Node child = Children[i];
			switch (child.Evaluate()) {
				case NodeState.SUCCESS:
					return NodeState.SUCCESS;
				case NodeState.RUNNING:
					runningIndex = i;
					return NodeState.RUNNING;
				default:
					continue;
			}
		}
		return NodeState.FAILURE;
	}
}
}