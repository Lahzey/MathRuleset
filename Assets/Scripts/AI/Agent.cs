using AI.Behaviour;
using DefaultNamespace.AI.PathFinding;
using UnityEngine;

namespace AI {
public class Agent : MonoBehaviour {
	
	[SerializeField] protected InfluenceGrid influenceGrid;
	[SerializeField] private float moveSpeed = 1;

	protected readonly BehaviourTree BehaviourTree = new BehaviourTree();
	
	private void Update() {
		BehaviourTree.Evaluate();
	}

	public void MoveTo(Vector3 target) {
		Vector3 direction = (target - transform.position).normalized;
		transform.position += direction * (moveSpeed * Time.deltaTime);
	}
	
}
}