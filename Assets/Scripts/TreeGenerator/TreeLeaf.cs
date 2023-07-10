using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.TreeGenerator {
public class TreeLeaf {
	
	public readonly TreeBranchNode Origin;
	private readonly Quaternion LocalRotation;
	public Quaternion WorldRotation => Origin.Branch.WorldRotation * LocalRotation;
	public readonly Vector2 Size;


	public TreeLeaf(TreeBranchNode origin, Quaternion rotation, Vector2 size, TreeGenConfig treeConfig) {
		Origin = origin;
		LocalRotation = rotation;
		Size = size;
		
		BranchGenConfig branchConfig = treeConfig[origin.Branch.Depth];
	}
	
	public Vector2Int GenerateMeshData(Vector3[] vertices, int[] triangles, Vector2[] uvs, Vector3[] normals, Vector2Int offsets) {
		Quaternion rotation = WorldRotation;
		Vector3 originPosition = Origin.WorldPosition + rotation * new Vector3(0, Origin.Radius, 0); // TODO: find a way to go out of node radius on the node normal, not the full direction of the leaf
		
		int vertexOffset = offsets.x;
		int triangleOffset = offsets.y;

		// generate vertices of square going up from origin (x centered at origin)
		Vector3 bottomLeft = originPosition + rotation * new Vector3(-Size.x / 2, 0, 0);
		Vector3 bottomRight = originPosition + rotation * new Vector3(Size.x / 2, 0, 0);
		Vector3 topLeft = originPosition + rotation * new Vector3(-Size.x / 2, Size.y, 0);
		Vector3 topRight = originPosition + rotation * new Vector3(Size.x / 2, Size.y, 0);
		
		// generate triangles
		triangles[triangleOffset++] = 1 + vertexOffset;
		triangles[triangleOffset++] = 0 + vertexOffset;
		triangles[triangleOffset++] = 3 + vertexOffset;
		
		triangles[triangleOffset++] = 0 + vertexOffset;
		triangles[triangleOffset++] = 2 + vertexOffset;
		triangles[triangleOffset++] = 3 + vertexOffset;
		
		Vector3 normal = rotation * Vector3.forward;
		
		// add vertices, uvs and normals
		vertices[vertexOffset] = bottomLeft;
		uvs[vertexOffset] = new Vector2(0, 0);
		normals[vertexOffset] = normal;
		vertexOffset++;
		vertices[vertexOffset] = bottomRight;
		uvs[vertexOffset] = new Vector2(1, 0);
		normals[vertexOffset] = normal;
		vertexOffset++;
		vertices[vertexOffset] = topLeft;
		uvs[vertexOffset] = new Vector2(0, 1);
		normals[vertexOffset] = normal;
		vertexOffset++;
		vertices[vertexOffset] = topRight;
		uvs[vertexOffset] = new Vector2(1, 1);
		normals[vertexOffset] = normal;
		vertexOffset++;

		return new Vector2Int(vertexOffset, triangleOffset);
	}
}
}