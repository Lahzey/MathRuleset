using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.TreeGenerator {
public class TreeBranchNode {
	
	public readonly TreeBranch Branch;
	private Vector3 LocalPosition;
	public Vector3 WorldPosition => Branch != null ? Branch.Origin.WorldPosition + Branch.WorldRotation * LocalPosition : LocalPosition;
	private Quaternion LocalRotation;
	public Quaternion WorldRotation => Branch != null ? Branch.WorldRotation * LocalRotation : LocalRotation;
	public readonly float Radius;
	
	public readonly List<TreeBranch> SubBranches = new List<TreeBranch>();
	public readonly List<TreeLeaf> Leaves = new List<TreeLeaf>();

	public TreeBranchNode(TreeBranch branch, Vector3 localPosition, Quaternion localRotation, float radius) {
		Branch = branch;
		LocalPosition = localPosition;
		LocalRotation = localRotation;
		Radius = radius;
	}
}
}