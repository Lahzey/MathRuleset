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
		Branches.AddRange(trunk.GetSubBranches());
	}

	public Mesh GenerateMesh() {
		// calculate list capacities in advance
		int vertexCount = 0;
		int branchTriangleCount = 0;
		int leafTriangleCount = 0;
		foreach (TreeBranch branch in Branches) {
			vertexCount += branch.Nodes.Count * 2;
			branchTriangleCount += branch.Nodes.Count * 6;
			leafTriangleCount += branch.Leaves.Count * 6;
		}
		
		ICollection<Vector3> vertices = new List<Vector3>(vertexCount);
		ICollection<Vector2> uvs = new List<Vector2>(vertexCount);
		ICollection<Vector3> normals = new List<Vector3>(vertexCount);
		ICollection<int> branchTriangles = new List<int>(branchTriangleCount);
		ICollection<int> leafTriangles = new List<int>(leafTriangleCount);
		foreach (TreeBranch branch in Branches) {
			branch.GenerateBranchMeshData(vertices, branchTriangles, uvs, normals);
			branch.GenerateLeafMeshData(vertices, leafTriangles, uvs, normals);
		}

		Mesh mesh = new Mesh();
		mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
		mesh.subMeshCount = 2;
		
		mesh.vertices = vertices.ToArray();
		mesh.SetTriangles(branchTriangles.ToList(), 0);
		mesh.SetTriangles(leafTriangles.ToList(), 1);
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