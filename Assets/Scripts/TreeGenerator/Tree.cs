using System.Collections.Generic;
using System.Linq;
using Simulation;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Simulation.Random;

namespace DefaultNamespace.TreeGenerator {
public class Tree {
	
	private TreeBranchNode rootNode;
	private TreeBranch trunk;
	
	public readonly List<TreeBranch> Branches = new List<TreeBranch>();


	public Tree(TreeGenConfig config, int seed) {
		Random random = new Random(seed);

		float radius = config[0].Radius(random);
		rootNode = new TreeBranchNode(null, Vector3.zero, Quaternion.identity, radius);
		trunk = new TreeBranch(random, rootNode, Quaternion.identity, radius, config, 0);

		Branches.Add(trunk);
		trunk.GetSubBranches(Branches);
	}

	public PreparedMeshData PrepareMeshData() {
		// calculate list capacities in advance
		int totalVertexCount = 0;
		int totalBranchTriangleCount = 0;
		int totalLeafTriangleCount = 0;
		foreach (TreeBranch branch in Branches) {
			branch.GetMeshArraySizes(out int vertexCount, out int branchTriangleCount, out int leafTriangleCount);
			totalVertexCount += vertexCount;
			totalBranchTriangleCount += branchTriangleCount;
			totalLeafTriangleCount += leafTriangleCount;
		}
		
		Vector3[] vertices = new Vector3[totalVertexCount];
		Vector2[] uvs = new Vector2[totalVertexCount];
		Vector3[] normals = new Vector3[totalVertexCount];
		int[] branchTriangles = new int[totalBranchTriangleCount];
		int[] leafTriangles = new int[totalLeafTriangleCount];
		Vector2Int branchOffsets = Vector2Int.zero;
		Vector2Int leafOffsets = Vector2Int.zero;
		foreach (TreeBranch branch in Branches) {
			branchOffsets = branch.GenerateBranchMeshData(vertices, branchTriangles, uvs, normals, branchOffsets);
			leafOffsets.x = branchOffsets.x;
			leafOffsets = branch.GenerateLeafMeshData(vertices, leafTriangles, uvs, normals, leafOffsets);
			branchOffsets.x = leafOffsets.x;
		}
		
		return new PreparedMeshData(vertices, uvs, normals, branchTriangles.ToArray(), leafTriangles.ToArray());
	}

	public Mesh GenerateMesh(PreparedMeshData preparedMeshData, bool markMeshNoLongerReadable = true) {
		Mesh mesh = new Mesh();
		mesh.indexFormat = preparedMeshData.Vertices.Length > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
		mesh.subMeshCount = 2;

		mesh.vertices = preparedMeshData.Vertices;
		mesh.SetTriangles(preparedMeshData.BranchTriangles, 0);
		mesh.SetTriangles(preparedMeshData.LeafTriangles, 1);
		mesh.normals = preparedMeshData.Normals;
		mesh.uv = preparedMeshData.Uvs;
		
		// apply
		mesh.RecalculateBounds();
		mesh.RecalculateTangents();
		mesh.UploadMeshData(markMeshNoLongerReadable);
		
		return mesh;
	}

}
	
public struct PreparedMeshData {
	public readonly Vector3[] Vertices;
	public readonly Vector2[] Uvs;
	public readonly Vector3[] Normals;
	public readonly int[] BranchTriangles;
	public readonly int[] LeafTriangles;

	public PreparedMeshData(Vector3[] vertices, Vector2[] uvs, Vector3[] normals, int[] branchTriangles, int[] leafTriangles) {
		Vertices = vertices;
		Uvs = uvs;
		Normals = normals;
		BranchTriangles = branchTriangles;
		LeafTriangles = leafTriangles;
	}
}
}