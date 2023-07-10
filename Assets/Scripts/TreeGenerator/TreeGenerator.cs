using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace.TreeGenerator {
[RequireComponent(typeof(MeshFilter))]
public class TreeGenerator : MonoBehaviour {
	
	[SerializeField] private TreeGenConfig config;
	[SerializeField] private int seed;
	[SerializeField] private bool drawSkeleton;
	[SerializeField] private bool drawBranchOrigins;
	[SerializeField] private bool drawBranchNodes;
	[SerializeField] private bool drawBranchDirections;
	[SerializeField] private bool drawLeafTriangles;
	
	[SerializeField] private bool spawnNext = false;
	[SerializeField] private Vector3 nextSpawnLocation = Vector3.zero;
	
	private Tree tree;

	private void Update() {
		if (!spawnNext) return;
		spawnNext = false;
		int seed = Random.Range(0, 1000000);
		Tree newTree = new Tree(config, seed);
		Mesh mesh = newTree.GenerateMesh(newTree.PrepareMeshData());
		GameObject newTreeObject = new GameObject("Tree" + seed);
		newTreeObject.transform.position = nextSpawnLocation;
		newTreeObject.AddComponent<MeshFilter>().mesh = mesh;
		newTreeObject.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
	}

	private void Start() {
		tree = new Tree(config, seed);
		Mesh mesh = tree.GenerateMesh(tree.PrepareMeshData());
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;
	}

	private void OnDrawGizmos() {
		if (tree == null) return;
		
		DrawSkeleton();
		DrawShell();
		DrawLeafTriangles();
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
	
	private void DrawLeafTriangles() {
		if (!drawLeafTriangles) return;
		foreach (TreeBranch branch in tree.Branches) {
			foreach (TreeBranchNode node in branch.Nodes) {
				foreach (TreeLeaf leaf in node.Leaves) {
					Quaternion rotation = leaf.WorldRotation;
					Vector3 originPosition = leaf.Origin.WorldPosition + rotation * new Vector3(0, leaf.Origin.Radius, 0);
					Vector3 size = leaf.Size;
					Vector3 bottomLeft = originPosition + rotation * new Vector3(-size.x / 2, 0, 0);
					Vector3 bottomRight = originPosition + rotation * new Vector3(size.x / 2, 0, 0);
					Vector3 topLeft = originPosition + rotation * new Vector3(-size.x / 2, size.y, 0);
					Vector3 topRight = originPosition + rotation * new Vector3(size.x / 2, size.y, 0);
					
					
					Handles.color = new Color(0.95f, 1f, 0.38f);
					Handles.DrawLine(bottomRight, bottomLeft);
					Handles.DrawLine(bottomLeft, topRight);
					Handles.DrawLine(topRight, bottomRight);
					Handles.color = new Color(1f, 0.55f, 0.2f);
					Handles.DrawLine(bottomLeft, topLeft);
					Handles.DrawLine(topLeft, topRight);
					Handles.DrawLine(topRight, bottomLeft);
				}
			}
		}
	}
}
}