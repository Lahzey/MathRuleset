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

	public virtual bool MoveTo(Vector3 target) {
		Vector3 direction = (target - transform.position).normalized;
		Vector3 nextPosition = transform.position + direction * (moveSpeed * Time.deltaTime);
		if (influenceGrid.IsObstacle(nextPosition)) return false;
		transform.position = nextPosition;
		return true;
	}
	
}
}