using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace.TreeGenerator {
public class Tree {
	
	private TreeBranchNode rootNode;
	private TreeBranch trunk;
	
	public readonly List<TreeBranch> Branches = new List<TreeBranch>();


	public Tree(TreeGenConfig config, int seed) {
		Random.InitState(seed);

		float radius = config[0].Radius;
		rootNode = new TreeBranchNode(null, Vector3.zero, Quaternion.identity, radius);
		trunk = new TreeBranch(rootNode, Vector3.up, radius, config, 0);
		
		Branches.Add(trunk);
		trunk.GetSubBranches(Branches);
	}

	public Mesh GenerateMesh() {
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
		List<int> branchTriangles = new List<int>(totalBranchTriangleCount);
		List<int> leafTriangles = new List<int>(totalLeafTriangleCount);
		int vertexOffset = 0;
		foreach (TreeBranch branch in Branches) {
			vertexOffset = branch.GenerateBranchMeshData(vertices, branchTriangles, uvs, normals, vertexOffset);
			vertexOffset = branch.GenerateLeafMeshData(vertices, leafTriangles, uvs, normals, vertexOffset);
		}

		Mesh mesh = new Mesh();
		mesh.indexFormat = vertices.Length > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
		mesh.subMeshCount = 2;

		mesh.vertices = vertices;
		mesh.SetTriangles(branchTriangles, 0);
		mesh.SetTriangles(leafTriangles, 1);
		mesh.normals = normals;
		mesh.uv = uvs;
		
		// apply
		mesh.RecalculateBounds();
		mesh.RecalculateTangents();
		mesh.UploadMeshData(true);
		
		return mesh;
	}

}
}