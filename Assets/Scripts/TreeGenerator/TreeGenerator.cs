using UnityEditor;
using UnityEngine;

namespace DefaultNamespace.TreeGenerator {
[RequireComponent(typeof(MeshFilter))]
public class TreeGenerator : MonoBehaviour {
	
	[SerializeField] private TreeGenConfig config;
	[SerializeField] private int seed;
	[SerializeField] private bool drawSkeleton;
	[SerializeField] private bool drawBranchOrigins;
	[SerializeField] private bool drawBranchNodes;
	[SerializeField] private bool drawBranchDirections;
	
	private Tree tree;

	private void Start() {
		tree = new Tree(config, seed);
		Mesh mesh = tree.GenerateMesh();
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;
	}

	private void OnDrawGizmos() {
		if (tree == null) return;
		
		// draw skeleton
		DrawSkeleton();
		
		// draw shell
		DrawShell();
	}

	private void DrawSkeleton() {
		if (!drawSkeleton) return;
		Handles.color = Color.red;
		foreach (TreeBranch branch in tree.Branches) {
			Vector3 position = branch.Origin.WorldPosition;
			foreach (TreeBranchNode node in branch.Nodes) {
				Vector3 worldPosition = node.WorldPosition;
				Handles.DrawLine(position, worldPosition);
				position = worldPosition;
			}
		}
	}

	private void DrawShell() {
		foreach (TreeBranch branch in tree.Branches) {
			if (branch.Nodes.Count == 0) continue;
			Handles.color = Color.cyan;
			if (drawBranchOrigins) Handles.DrawWireDisc(branch.Origin.WorldPosition, branch.WorldRotation * Vector3.up, branch.StartRadius);
			if (drawBranchDirections) Handles.DrawLine(branch.Origin.WorldPosition, branch.Origin.WorldPosition + branch.WorldRotation * Vector3.up * branch.StartRadius * 10f);
			
			if (!drawBranchNodes) continue;
			Handles.color = Color.green;
			foreach (TreeBranchNode node in branch.Nodes) {
				Handles.DrawWireDisc(node.WorldPosition, node.WorldRotation * Vector3.up, node.Radius);
			}
		}
	}
}
}