using System;
using System.Collections.Generic;
using UnityEngine;
using Random = Simulation.Random;

namespace DefaultNamespace.TreeGenerator {
public class TreeBranch {
	
	private static readonly float MIN_CIRCLE_DIST = Mathf.Sqrt(1*1 + 1*1) / 2; // if we have a diamond with rounded normals (to simulate a circle) of radius 1 this is the closest the actual edges will get to the center
	
	private static readonly Vector3[] CIRCLE_VERTICES = {
		new Vector3(1, 0, 0),
		new Vector3(0, 0, -1),
		new Vector3(-1, 0, 0),
		new Vector3(0, 0, 1)
	};
	
	public readonly TreeBranchNode Origin;
	private readonly Quaternion LocalRotation;
	public readonly Quaternion WorldRotation;
	public readonly float StartRadius;

	public readonly int Depth; // 0 = trunk, 1 = first branch, 2 = second branch, etc.

	public readonly List<TreeBranchNode> Nodes = new List<TreeBranchNode>();
	
	private int leafCount = 0; // saves some performance when generating the mesh as we do not need loops to calculate the array sizes
	
	public TreeBranch(Random random, TreeBranchNode origin, Quaternion rotation, float radius, TreeGenConfig treeConfig, int depth) {
		Origin = origin;
		LocalRotation = rotation;
		WorldRotation = origin.Branch?.WorldRotation * LocalRotation ?? LocalRotation;
		StartRadius = radius;
		Depth = depth;
		
		if (radius > origin.Radius) Debug.LogWarning("Branch radius is larger than origin radius!");
		
		BranchGenConfig branchConfig = treeConfig[depth];
		bool canSpawnSubBranches = depth < treeConfig.BranchingDepth(random);
		float minBranchingHeight = branchConfig.MinBranchingHeight(random) * radius; // min branching height relative
		minBranchingHeight += origin.Radius * 2; // offset min branching height to make sure it does not intersect origin branch
		float maxLength = branchConfig.MaxLength(random);

		Vector3 currentNodePosition = Vector3.zero;
		Vector3 angleToNextNode = Vector3.zero;
		float branchLength = 0;

		// for non-trunk branches we need to generate an initial node that steps outside the origin branch
		// if (origin.Branch != null)
		// 	Nodes.Add(new TreeBranchNode(this, Vector3.up * origin.Radius, Quaternion.identity, radius));

		int loopCount = 0;
		while (radius > 0 && branchLength < maxLength) {
			// safety so infinitely looping settings do not cause a unity crash
			loopCount++;
			if (loopCount > 1000)
				throw new ArgumentException("TreeGen settings may cause an infinite loop, gave up after 1000 nodes in the same branch.");

			// position the next node
			float distanceToNextNode = 1 / (branchConfig.NodeFrequency(random) / radius);
			Quaternion nodeRotation = Quaternion.Euler(angleToNextNode);
			Vector3 nextNodePosition = currentNodePosition + nodeRotation * Vector3.up * distanceToNextNode;
			branchLength += distanceToNextNode;

			// radius decay if applied by distance
			if (branchConfig.RadiusDecayMode.IsPerDistance()) radius -= StartRadius - ReduceRadiusByAreaPart(StartRadius, branchConfig.RadiusDecay(random) * distanceToNextNode, out float _);
			
			// create the node
			TreeBranchNode nextNode = new TreeBranchNode(this, nextNodePosition, nodeRotation, radius);
			nextNode.DistanceToPreviousNode = distanceToNextNode;
			Nodes.Add(nextNode);
			
			// handle branching
			if (canSpawnSubBranches && branchLength >= minBranchingHeight && nextNode.Radius > branchConfig.LeafSpawnRadiusThreshold(random) && random.value <= branchConfig.BranchingProbability(random)) {
				int branchingCount = branchConfig.BranchingCount(random);
				Quaternion[] branchingDirections = branchConfig.GenerateBranchingDirections(random, branchingCount);
				
				// generate radius decays so we can apply them all to the full radius (so 2x0.5 decays to 0, not 0.25)
				float[] radiusDecays = new float[branchingCount];
				float totalRadiusDecay = 0;
				float decayedArea = 0;
				for (int i = 0; i < branchingCount; i++) {
					radiusDecays[i] = branchConfig.RadiusDecay(random);
					totalRadiusDecay += radiusDecays[i];
				}
				if (branchConfig.RadiusDecayMode.IsPerBranch()) radius = ReduceRadiusByAreaPart(radius, totalRadiusDecay, out decayedArea);
				
				for (int i = 0; i < branchingCount; i++) {
					// calculate radius and apply radius decay if applied by branch
					float branchRadius = treeConfig[depth + 1].Radius(random);
					if (branchConfig.RadiusDecayMode == RadiusDecayMode.PER_BRANCH_TRANSFER_RADIUS) {	
						branchRadius *= ToRadius(decayedArea * (radiusDecays[i] / totalRadiusDecay));
					}
					
					// create branch
					TreeBranch branch = new TreeBranch(random, nextNode, branchingDirections[i], branchRadius, treeConfig, depth + 1);
					nextNode.SubBranches.Add(branch);
				}
			}
			
			// handle leaf generation
			if (nextNode.Radius <= branchConfig.LeafSpawnRadiusThreshold(random) && random.value <= branchConfig.LeafProbability(random)) {
				int leafCount = branchConfig.LeafCount(random);
				Quaternion[] leafDirections = branchConfig.GenerateLeafDirections(random, leafCount);
				for (int i = 0; i < leafCount; i++) {
					radius -= branchConfig.LeafRadiusDecay(random);
					TreeLeaf leaf = new TreeLeaf(nextNode, leafDirections[i], branchConfig.LeafSize(random), treeConfig);
					nextNode.Leaves.Add(leaf);
				}
				this.leafCount += leafCount;
			}

			// calculations for next iteration
			currentNodePosition = nextNodePosition;
			Vector3 nodeAngle = branchConfig.NodeAngle(random);
			Vector3 anglePerDistance = nodeAngle / (radius / 2);
			angleToNextNode += anglePerDistance * distanceToNextNode;
			angleToNextNode.x = Mathf.Clamp(angleToNextNode.x, branchConfig.MaxNodeXAngleDeviation.x, branchConfig.MaxNodeXAngleDeviation.y);
			angleToNextNode.z = Mathf.Clamp(angleToNextNode.z, branchConfig.MaxNodeZAngleDeviation.x, branchConfig.MaxNodeZAngleDeviation.y);
		}
		
		// create topping node if the last node has a significant radius (leaving a hole in the top)
		TreeBranchNode lastNode = Nodes[^1];
		if (lastNode.Radius > 0.01f) {
			float distanceToNextNode = lastNode.Radius * 0.5f;
			Vector3 toppingNodePosition = lastNode.LocalPosition + lastNode.LocalRotation * Vector3.up * distanceToNextNode;
			TreeBranchNode toppingNode = new TreeBranchNode(this, toppingNodePosition, lastNode.LocalRotation, 0.01f);
			toppingNode.DistanceToPreviousNode = distanceToNextNode;
			Nodes.Add(toppingNode);
		}
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

	public Vector2Int GenerateBranchMeshData(Vector3[] vertices, int[] triangles, Vector2[] uvs, Vector3[] normals, Vector2Int offsets) {
		if (Nodes.Count == 0) return offsets;
		int nodeSize = CIRCLE_VERTICES.Length;
		int halfNodeSize = nodeSize / 2;
		
		int vertexOffset = offsets.x;
		int triangleOffset = offsets.y;

		Vector3 firstBranchAngle = Quaternion.FromToRotation(Nodes[0].WorldRotation * Vector3.up, Origin.WorldRotation * Vector3.up).eulerAngles;
		firstBranchAngle.y = 0;
		Vector3 originPosition = Origin.WorldPosition;
		Quaternion originRotation = Quaternion.Euler(firstBranchAngle);

		// generate vertices (creates a circle for each node, including the origin), uvs and normals
		int vertexIndex = vertexOffset;
		TreeBranchNode node;
		for (int i = -1; i < Nodes.Count; i++) {
			float radius;
			Vector3 position;
			Quaternion rotation;
			if (i < 0) {
				node = Origin;
				radius = node.Radius;
				position = originPosition;
				rotation = originRotation;
			}
			else {
				node = Nodes[i];
				radius = node.Radius;
				position = node.WorldPosition;
				rotation = node.WorldRotation;
			}
			
			float nodeHeight = i < 0 ? Nodes[0].DistanceToPreviousNode : node.DistanceToPreviousNode;
			float circumference = 2 * Mathf.PI * radius;
			float uvSizePerPart = Mathf.Round(circumference / nodeHeight) / (nodeSize / 2f);
			
			for (int j = 0; j < nodeSize; j++) {
				Vector3 circleVertex = CIRCLE_VERTICES[j];
				vertices[vertexIndex] = position + rotation * circleVertex * radius;
				float uvX = j <= halfNodeSize ? uvSizePerPart * j : 1 - uvSizePerPart * (j - halfNodeSize);
				uvs[vertexIndex] = new Vector2(uvX,i % 2 == 0 ? 1 : 0);
				normals[vertexIndex] = rotation * circleVertex;
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
				
				triangles[triangleOffset++] = a + vertexOffset;
				triangles[triangleOffset++] = b + vertexOffset;
				triangles[triangleOffset++] = c + vertexOffset;
				
				triangles[triangleOffset++] = b + vertexOffset;
				triangles[triangleOffset++] = d + vertexOffset;
				triangles[triangleOffset++] = c + vertexOffset;
			}
		}

		return new Vector2Int(vertexIndex, triangleOffset);
	}
	
	public Vector2Int GenerateLeafMeshData(Vector3[] vertices, int[] triangles, Vector2[] uvs, Vector3[] normals, Vector2Int offsets) {
		TreeBranchNode node;
		for (int i = 0; i < Nodes.Count; i++) {
			node = Nodes[i];
			for (int j = 0; j < node.Leaves.Count; j++) {
				try {
					offsets = node.Leaves[j].GenerateMeshData(vertices, triangles, uvs, normals, offsets);
				} catch (Exception e) {
					Debug.Log($"At offset {offsets} with triangle count {triangles.Length} and vertex count {vertices.Length}");
					throw;
				}
			}
		}
		return offsets;
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
		if (area <= 0) return 0;
		return Mathf.Sqrt(area / Mathf.PI);
	}

}
}