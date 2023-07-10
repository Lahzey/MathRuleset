using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.TreeGenerator {
public class TreeBranchNode {
	
	public readonly TreeBranch Branch;
	public readonly Vector3 LocalPosition;
	public readonly Vector3 WorldPosition;
	public readonly Quaternion LocalRotation;
	public readonly Quaternion WorldRotation;
	public readonly float Radius;
	
	public float DistanceToPreviousNode = 0;
	
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