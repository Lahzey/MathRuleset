using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.TreeGenerator {
public class TreeLeaf {
	
	public readonly TreeBranchNode Origin;
	private readonly Quaternion LocalRotation;
	public Quaternion WorldRotation => Origin.Branch.WorldRotation * LocalRotation;
	public readonly Vector2 Size;


	public TreeLeaf(TreeBranchNode origin, Vector3 direction, Vector2 size, TreeGenConfig treeConfig) {
		Origin = origin;
		LocalRotation = Quaternion.FromToRotation(Vector3.up, direction);
		Size = size;
		
		BranchGenConfig branchConfig = treeConfig[origin.Branch.Depth];
	}
	
	public void GenerateMeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals) {
		Quaternion rotation = WorldRotation;
		Vector3 originPosition = Origin.WorldPosition + rotation * new Vector3(0, Origin.Radius, 0); // TODO: find a way to go out of node radius on the node normal, not the full direction of the leaf
		int vertexOffset = vertices.Count;
		
		// generate vertices of square going up from origin (x centered at origin)
		Vector3 bottomLeft = originPosition + rotation * new Vector3(-Size.x / 2, 0, 0);
		Vector3 bottomRight = originPosition + rotation * new Vector3(Size.x / 2, 0, 0);
		Vector3 topLeft = originPosition + rotation * new Vector3(-Size.x / 2, Size.y, 0);
		Vector3 topRight = originPosition + rotation * new Vector3(Size.x / 2, Size.y, 0);
		vertices.Add(bottomLeft);
		vertices.Add(bottomRight);
		vertices.Add(topLeft);
		vertices.Add(topRight);
		
		// generate triangles
		triangles.Add(1 + vertexOffset);
		triangles.Add(0 + vertexOffset);
		triangles.Add(3 + vertexOffset);
		triangles.Add(0 + vertexOffset);
		triangles.Add(2 + vertexOffset);
		triangles.Add(3 + vertexOffset);
		
		// generate uvs
		uvs.Add(new Vector2(0, 0));
		uvs.Add(new Vector2(1, 0));
		uvs.Add(new Vector2(0, 1));
		uvs.Add(new Vector2(1, 1));
		
		// generate normals
		normals.Add(rotation * Vector3.forward);
		normals.Add(rotation * Vector3.forward);
		normals.Add(rotation * Vector3.forward);
		normals.Add(rotation * Vector3.forward);
	}
}
}