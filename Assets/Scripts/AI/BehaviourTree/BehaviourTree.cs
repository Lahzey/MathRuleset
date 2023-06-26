using System;
using UnityEngine;

namespace AI.BehaviourTree {
public class BehaviourTree : MonoBehaviour {

	private readonly Sequencer root = new Sequencer();
	
	public bool Remove(Node child) {
		return root.Remove(child);
	}

	public void Add(Node child) {
		root.Add(child);
	}

	private void Update() {
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