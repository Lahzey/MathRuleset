using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.TreeGenerator {
public class TreeBranch {
	
	private static readonly Vector3[] CIRCLE_VERTICES = {
		new Vector3(1, 0, 0),
		new Vector3(0, 0, 1),
		new Vector3(-1, 0, 0),
		new Vector3(0, 0, -1)
	};
	
	public readonly TreeBranchNode Origin;
	private readonly Quaternion LocalRotation;
	public Quaternion WorldRotation => Origin.Branch?.WorldRotation * LocalRotation ?? LocalRotation;
	public readonly float StartRadius;

	public readonly int Depth; // 0 = trunk, 1 = first branch, 2 = second branch, etc.

	public readonly List<TreeBranchNode> Nodes = new List<TreeBranchNode>();
	
	
	public TreeBranch(TreeBranchNode origin, Vector3 direction, float radius, TreeGenConfig treeConfig, int depth) {
		Origin = origin;
		LocalRotation = Quaternion.FromToRotation(Vector3.up, direction);
		StartRadius = radius;
		Depth = depth;
		
		if (radius > origin.Radius) Debug.LogWarning("Branch radius is larger than origin radius!");
		
		BranchGenConfig branchConfig = treeConfig[depth];
		bool canSpawnSubBranches = depth < treeConfig.BranchingDepth;
		float minBranchingHeight = branchConfig.MinBranchingHeight + origin.Radius;
		float maxLength = branchConfig.MaxLength;

		Vector3 currentNodePosition = Vector3.zero; // will apply origin position later when applying rotation
		Vector3 angleToNextNode = Vector3.zero;
		float branchLength = 0;
		
		while (radius > 0 && branchLength < maxLength) {
			// position the next node
			float distanceToNextNode = 1 / (branchConfig.NodeFrequency / radius);
			Quaternion rotation = Quaternion.Euler(angleToNextNode);
			Vector3 nextNodePosition = currentNodePosition + rotation * Vector3.up * distanceToNextNode;
			branchLength += distanceToNextNode;

			// radius decay if applied by distance
			if (branchConfig.RadiusDecayMode.IsPerDistance()) radius -= StartRadius - ReduceRadiusByAreaPart(StartRadius, branchConfig.RadiusDecay * distanceToNextNode, out float _);
			
			// create the node
			TreeBranchNode nextNode = new TreeBranchNode(this, nextNodePosition, rotation, radius);
			Nodes.Add(nextNode);
			
			// handle branching
			if (canSpawnSubBranches && branchLength >= minBranchingHeight && nextNode.Radius > branchConfig.LeafSpawnRadiusThreshold && Random.value <= branchConfig.BranchingProbability) {
				int branchingCount = branchConfig.BranchingCount;
				Vector3[] branchingDirections = branchConfig.GenerateBranchingDirections(branchingCount);
				for (int i = 0; i < branchingCount; i++) {
					// calculate radius and apply radius decay if applied by branch
					float branchRadius = treeConfig[depth + 1].Radius;
					float branchArea = 0;
					if (branchConfig.RadiusDecayMode.IsPerBranch()) radius = ReduceRadiusByAreaPart(radius, branchConfig.RadiusDecay, out branchArea);
					if (branchConfig.RadiusDecayMode == RadiusDecayMode.PER_BRANCH_TRANSFER_RADIUS) {
						branchRadius *= ToRadius(branchArea);
					}
					
					// create branch
					TreeBranch branch = new TreeBranch(nextNode, branchingDirections[i], branchRadius, treeConfig, depth + 1);
					nextNode.SubBranches.Add(branch);
				}
			}
			
			// TODO: handle leaf generation
			if (nextNode.Radius <= branchConfig.LeafSpawnRadiusThreshold && Random.value <= branchConfig.LeafProbability) {
				int leafCount = branchConfig.LeafCount;
				// Vector3[] leafDirections = branchConfig.GenerateLeafDirections(leafCount);
				for (int i = 0; i < leafCount; i++) {
					radius -= branchConfig.LeafRadiusDecay;
				}
			}
			
			// calculations for next iteration
			currentNodePosition = nextNodePosition;
			Vector3 nodeAngle = branchConfig.NodeAngle;
			Vector3 anglePerDistance = nodeAngle / (radius / 2);
			angleToNextNode += anglePerDistance * distanceToNextNode;
			angleToNextNode.x = Mathf.Clamp(angleToNextNode.x, branchConfig.MaxNodeXAngleDeviation.x, branchConfig.MaxNodeXAngleDeviation.y);
			angleToNextNode.z = Mathf.Clamp(angleToNextNode.z, branchConfig.MaxNodeZAngleDeviation.x, branchConfig.MaxNodeZAngleDeviation.y);
		}
		
		// TODO: create topping for the branch
	}

	public List<TreeBranch> GetSubBranches() {
		List<TreeBranch> subBranches = new List<TreeBranch>();
		foreach (TreeBranchNode node in Nodes) {
			foreach (TreeBranch branch in node.SubBranches) {
				subBranches.Add(branch);
				subBranches.AddRange(branch.GetSubBranches());
			}
		}
		return subBranches;
	}
	
	public List<Vector3> GetVertices() {
		List<Vector3> vertices = new List<Vector3>();
		if (Nodes.Count == 0) return vertices;

		for (int i = -1; i < Nodes.Count; i++) {
			TreeBranchNode node;
			float radius;
			if (i < 0) {
				node = Origin;
				radius = StartRadius;
			}
			else {
				node = Nodes[i];
				radius = node.Radius;
			}
			foreach (Vector3 circleVertex in CIRCLE_VERTICES) {
				vertices.Add(node.WorldPosition + node.WorldRotation * circleVertex * radius);
			}
		}

		return vertices;
	}
	
	public List<int> GetTriangles() {
		List<int> triangles = new List<int>();
		if (Nodes.Count == 0) return triangles;

		int nodeSize = CIRCLE_VERTICES.Length;
		for (int i = 0; i < Nodes.Count; i++) {
			// generate two triangles for each vertex on the current node (connecting to previous node or branch origin if first node)
			for (int j = 0; j < nodeSize; j++) {
				int a = i * nodeSize + j; // current vertex (j) on previous node
				int b = i * nodeSize + (j + 1) % nodeSize; // next vertex on previous node
				int c = (i + 1) * nodeSize + j; // current vertex (j) on same node (i)
				int d = (i + 1) * nodeSize + (j + 1) % nodeSize; // next vertex on same node (i)
				
				triangles.Add(a);
				triangles.Add(b);
				triangles.Add(c);
				
				triangles.Add(b);
				triangles.Add(d);
				triangles.Add(c);
			}
		}

		return triangles;
	}

	public List<Vector3> GetNormals() {
		List<Vector3> normals = new List<Vector3>();
		if (Nodes.Count == 0) return normals;

		for (int i = -1; i < Nodes.Count; i++) {
			TreeBranchNode node = i < 0 ? Origin : Nodes[i];
			foreach (Vector3 circleVertex in CIRCLE_VERTICES) {
				normals.Add(node.WorldRotation * circleVertex);
			}
		}

		return normals;
	}

	public void GenerateLeafMeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs) {
		foreach (TreeBranchNode node in Nodes) {
			foreach (TreeLeaf leaf in node.Leaves) {
				leaf.GenerateMeshData(vertices, triangles, uvs);
			}
		}
	}

	private static float ReduceRadiusByAreaPart(float radius, float areaPart, out float reducedArea) {
		float area = ToArea(radius);
		reducedArea = area * areaPart;
		return ToRadius(area - reducedArea);
	}
	
	private static float ToArea(float radius) {
		return Mathf.PI * radius * radius;
	}
	
	private static float ToRadius(float area) {
		return Mathf.Sqrt(area / Mathf.PI);
	}

}
}