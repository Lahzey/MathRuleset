using System.Collections.Generic;
using UnityEngine;

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
		Branches.AddRange(trunk.GetSubBranches());
	}

	public Mesh GenerateMesh() {
		Mesh mesh = new Mesh();
		mesh.subMeshCount = 2;

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Vector3> normals = new List<Vector3>();
		List<int> branchTriangles = new List<int>();
		List<int> leafTriangles = new List<int>();
		foreach (TreeBranch branch in Branches) {
			branch.GenerateBranchMeshData(vertices, branchTriangles, uvs, normals);
			branch.GenerateLeafMeshData(vertices, leafTriangles, uvs, normals);
		}
		
		mesh.vertices = vertices.ToArray();
		mesh.SetTriangles(branchTriangles, 0);
		mesh.SetTriangles(leafTriangles, 1);
		mesh.normals = normals.ToArray();
		mesh.uv = uvs.ToArray();
		
		// apply
		mesh.RecalculateBounds();
		mesh.RecalculateTangents();
		mesh.UploadMeshData(true);
		
		return mesh;
	}

}
}