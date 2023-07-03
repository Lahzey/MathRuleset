using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.TreeGenerator {
public class TreeBranchNode {
	
	public readonly TreeBranch Branch;
	private Vector3 LocalPosition;
	public Vector3 WorldPosition;
	private Quaternion LocalRotation;
	public Quaternion WorldRotation;
	public readonly float Radius;
	
	public readonly List<TreeBranch> SubBranches = new List<TreeBranch>();
	public readonly List<TreeLeaf> Leaves = new List<TreeLeaf>();

	public TreeBranchNode(TreeBranch branch, Vector3 localPosition, Quaternion localRotation, float radius) {
		Branch = branch;
		LocalPosition = localPosition;
		WorldPosition = branch != null ? branch.Origin.WorldPosition + branch.WorldRotation * localPosition : localPosition;
		LocalRotation = localRotation;
		WorldRotation = branch != null ? branch.WorldRotation * localRotation : localRotation;
		Radius = radius;
	}
}
}