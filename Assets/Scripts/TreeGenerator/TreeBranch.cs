using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Random = UnityEngine.Random;

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
	public readonly Quaternion WorldRotation;
	public readonly float StartRadius;

	public readonly int Depth; // 0 = trunk, 1 = first branch, 2 = second branch, etc.

	public readonly List<TreeBranchNode> Nodes = new List<TreeBranchNode>();
	
	private int leafCount = 0; // saves some performance when generating the mesh as we do not need loops to calculate the array sizes
	
	public TreeBranch(TreeBranchNode origin, Vector3 direction, float radius, TreeGenConfig treeConfig, int depth) {
		Origin = origin;
		Quaternion forwardCorrection = Quaternion.AngleAxis(Vector3.Angle(Vector3.forward, new Vector3(direction.x, 0, direction.z)), Vector3.up); // rotates the forward towards direction, so when we tilt the branch later, the forward will be tilted downwards
		LocalRotation = forwardCorrection * Quaternion.AngleAxis(Vector3.Angle(Vector3.up, direction), Vector3.Cross(Vector3.up, direction));
		WorldRotation = origin.Branch?.WorldRotation * LocalRotation ?? LocalRotation;
		StartRadius = radius;
		Depth = depth;
		
		if (radius > origin.Radius) Debug.LogWarning("Branch radius is larger than origin radius!");
		
		BranchGenConfig branchConfig = treeConfig[depth];
		bool canSpawnSubBranches = depth < treeConfig.BranchingDepth;
		float minBranchingHeight = branchConfig.MinBranchingHeight * radius; // min branching height relative
		minBranchingHeight += origin.Radius * 2; // offset min branching height to make sure it does not intersect origin branch
		float maxLength = branchConfig.MaxLength;

		Vector3 currentNodePosition = Vector3.zero; // will apply origin position later when applying rotation
		Vector3 angleToNextNode = Vector3.zero;
		float branchLength = 0;

		int loopCount = 0;
		while (radius > 0 && branchLength < maxLength) {
			// safety so infinitely looping settings do not cause a unity crash
			loopCount++;
			if (loopCount > 1000)
				throw new ArgumentException("TreeGen settings may an infinite loop, gave up after 1000 nodes.");

			// position the next node
			float distanceToNextNode = 1 / (branchConfig.NodeFrequency / radius);
			Quaternion rotation = Quaternion.Euler(angleToNextNode);
			Vector3 nextNodePosition = currentNodePosition + rotation * Vector3.up * distanceToNextNode;
			branchLength += distanceToNextNode;

			// radius decay if applied by distance
			if (branchConfig.RadiusDecayMode.IsPerDistance()) radius -= StartRadius - ReduceRadiusByAreaPart(StartRadius, branchConfig.RadiusDecay * distanceToNextNode, out float _);
			
			// create the node
			TreeBranchNode nextNode = new TreeBranchNode(this, nextNodePosition, rotation, radius);
			nextNode.DistanceToPreviousNode = distanceToNextNode;
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
			
			// handle leaf generation
			if (nextNode.Radius <= branchConfig.LeafSpawnRadiusThreshold && Random.value <= branchConfig.LeafProbability) {
				int leafCount = branchConfig.LeafCount;
				Vector3[] leafDirections = branchConfig.GenerateLeafDirections(leafCount);
				for (int i = 0; i < leafCount; i++) {
					radius -= branchConfig.LeafRadiusDecay;
					TreeLeaf leaf = new TreeLeaf(nextNode, leafDirections[i], branchConfig.LeafSize, treeConfig);
					nextNode.Leaves.Add(leaf);
				}
				this.leafCount += leafCount;
			}

			// calculations for next iteration
			currentNodePosition = nextNodePosition;
			Vector3 nodeAngle = branchConfig.NodeAngle;
			Vector3 anglePerDistance = nodeAngle / (radius / 2);
			angleToNextNode += anglePerDistance * distanceToNextNode;
			angleToNextNode.x = Mathf.Clamp(angleToNextNode.x, branchConfig.MaxNodeXAngleDeviation.x, branchConfig.MaxNodeXAngleDeviation.y);
			angleToNextNode.z = Mathf.Clamp(angleToNextNode.z, branchConfig.MaxNodeZAngleDeviation.x, branchConfig.MaxNodeZAngleDeviation.y);
		}
		
		// could also create topping for branch here, but usually branches get very small at the end so it's fine
	}

	public void GetSubBranches(List<TreeBranch> subBranches) {
		foreach (TreeBranchNode node in Nodes) {
			foreach (TreeBranch branch in node.SubBranches) {
				subBranches.Add(branch);
				branch.GetSubBranches(subBranches);
			}
		}
	}
	
	public void GetMeshArraySizes(out int vertexCount, out int branchTriangleCount, out int leafTriangleCount) {
		int nodeCount = Nodes.Count;
		vertexCount = (nodeCount + 1) * CIRCLE_VERTICES.Length + leafCount * 4;
		branchTriangleCount = nodeCount * CIRCLE_VERTICES.Length * 6;
		leafTriangleCount = leafCount * 6;
	}

	public int GenerateBranchMeshData(Vector3[] vertices, List<int> triangles, Vector2[] uvs, Vector3[] normals, int vertexOffset) {
		if (Nodes.Count == 0) return 0;
		int nodeSize = CIRCLE_VERTICES.Length;
		int halfNodeSize = nodeSize / 2;

		// generate vertices (creates a circle for each node, including the origin), uvs and normals
		int vertexIndex = vertexOffset;
		TreeBranchNode node;
		for (int i = -1; i < Nodes.Count; i++) {
			float radius;
			if (i < 0) {
				node = Origin;
				radius = StartRadius;
			}
			else {
				node = Nodes[i];
				radius = node.Radius;
			}
			
			float nodeHeight = i < 0 ? Nodes[0].DistanceToPreviousNode : node.DistanceToPreviousNode;
			float circumference = 2 * Mathf.PI * radius;
			float uvSizePerPart = Mathf.Round(circumference / nodeHeight) / (nodeSize / 2f);
			
			for (int j = 0; j < nodeSize; j++) {
				Vector3 circleVertex = CIRCLE_VERTICES[j];
				Quaternion worldRotation = node.WorldRotation;
				vertices[vertexIndex] = node.WorldPosition + worldRotation * circleVertex * radius;
				float uvX = j <= halfNodeSize ? uvSizePerPart * j : 1 - uvSizePerPart * (j - halfNodeSize);
				uvs[vertexIndex] = new Vector2(uvX,i % 2 == 0 ? 1 : 0);
				normals[vertexIndex] = worldRotation * circleVertex;
				vertexIndex++;
			}
		}
		
		// generate two triangles for each vertex on each node (connecting to previous node or branch origin if first node)
		for (int i = 0; i < Nodes.Count; i++) {
			for (int j = 0; j < nodeSize; j++) {
				int a = i * nodeSize + j; // current vertex (j) on previous node
				int b = i * nodeSize + (j + 1) % nodeSize; // next vertex on previous node
				int c = (i + 1) * nodeSize + j; // current vertex (j) on same node (i)
				int d = (i + 1) * nodeSize + (j + 1) % nodeSize; // next vertex on same node (i)
				
				// this stuff is counter clockwise, but we current show both sides anyways. Fix later maybe?
				
				triangles.Add(a + vertexOffset);
				triangles.Add(b + vertexOffset);
				triangles.Add(c + vertexOffset);
				
				triangles.Add(b + vertexOffset);
				triangles.Add(d + vertexOffset);
				triangles.Add(c + vertexOffset);
			}
		}

		return vertexIndex;
	}
	
	public int GenerateLeafMeshData(Vector3[] vertices, List<int> triangles, Vector2[] uvs, Vector3[] normals, int vertexOffset) {
		TreeBranchNode node;
		for (int i = 0; i < Nodes.Count; i++) {
			node = Nodes[i];
			for (int j = 0; j < node.Leaves.Count; j++) {
				vertexOffset = node.Leaves[j].GenerateMeshData(vertices, triangles, uvs, normals, vertexOffset);
			}
		}
		return vertexOffset;
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