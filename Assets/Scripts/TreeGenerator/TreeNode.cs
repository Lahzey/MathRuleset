using System.Collections.Generic;
using UnityEngine;

namespace TreeGenerator {
public class TreeNode {
	public Vector3 position;
	public float radius;

	public TreeNode previous;
	public TreeNode next;
	
	public List<Branch> branches = new List<Branch>();


	public Vector3[] GetVertices(int count) {
		Vector3[] vertices = new Vector3[count];
	}
}
}