using UnityEngine;

namespace AI.Behaviour {
public class BehaviourTree {

	private readonly Sequencer root = new Sequencer();
	
	public BehaviourTree Remove(Node child) {
		root.Remove(child);
		return this;
	}

	public BehaviourTree Add(Node child) {
		root.Add(child);
		return this;
	}

	public void Evaluate() {
		root.Evaluate();
		AfterTraversal(root);
	}
	
	private static void AfterTraversal(Node node) {
		node.AfterTreeTraversal();
		foreach (Node child in node) {
			AfterTraversal(child);
		}
	}
}
}