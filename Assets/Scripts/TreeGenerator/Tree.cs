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

		List<Vector3> branchVertices = new List<Vector3>();
		List<Vector3> branchNormals = new List<Vector3>();
		List<int> branchTriangles = new List<int>();
		List<Vector3> leafVertices = new List<Vector3>();
		List<int> leafTriangles = new List<int>();
		List<Vector2> leafUVs = new List<Vector2>();
		foreach (TreeBranch branch in Branches) {
			int offset = branchVertices.Count;
			branchVertices.AddRange(branch.GetVertices());
			branchNormals.AddRange(branch.GetNormals());
			foreach (int vertexIndex in branch.GetTriangles()) {
				branchTriangles.Add(offset + vertexIndex);
			}
			branch.GenerateLeafMeshData(leafVertices, leafTriangles, leafUVs);
		}

		branchVertices.AddRange(leafVertices);
		mesh.vertices = branchVertices.ToArray();
		mesh.triangles = branchTriangles.ToArray();
		mesh.normals = branchNormals.ToArray();
		
		mesh.SetVertices(leafVertices, 1, 1);
		
		Vector2[] uv = new Vector2[branchVertices.Count];
		for (int i = 0; i < uv.Length; i++) {
			uv[i] = new Vector2(i / (float) uv.Length, i / (float) uv.Length);
		}
		
		return mesh;
	}

}
}