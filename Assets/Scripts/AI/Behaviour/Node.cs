using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Behaviour {
public abstract class Node : IEnumerable<Node> {

	protected readonly List<Node> Children = new List<Node>();
	
	protected Action<NodeState> AfterEvaluate = null;
	protected Action OnExit = null;

	protected NodeState LastState { get; private set; }
	private bool hasBeenEvaluated = false;

	public NodeState Evaluate() {
		NodeState state = EvaluateImpl();
		AfterEvaluate?.Invoke(state);
		if (LastState == NodeState.RUNNING && state != NodeState.RUNNING) {
			OnExit?.Invoke();
		}

		hasBeenEvaluated = true;
		LastState = state;
		return state;
	}
	
	public void AfterTreeTraversal() {
		if (!hasBeenEvaluated) {
			if (LastState == NodeState.RUNNING) OnExit?.Invoke();
			LastState = NodeState.INACTIVE;
		}
		hasBeenEvaluated = false;
	}
	
	protected abstract NodeState EvaluateImpl();
	
	public virtual Node Add(Node child) {
		throw new NotImplementedException(); // will be thrown by default, but can be overriden
	}
	
	public virtual Node Remove(Node child) {
		Children.Remove(child);
		return this;
	}

	public IEnumerator<Node> GetEnumerator() {
		return Children.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}
}