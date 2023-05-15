﻿using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace.TreeGenerator {

[CreateAssetMenu(fileName = "BranchConfig", menuName = "ScriptableObjects/BranchGeneratorConfig", order = 2)]
public class BranchGenConfig : ScriptableObject {
	
	[SerializeField] private Vector2 maxLengthRange = new Vector2(50f, 50f);
	public float MaxLength => Random.Range(maxLengthRange.x, maxLengthRange.y);

	[Header("Radius")]
	[SerializeField] private Vector2 radiusRange = new Vector2(0.5f, 1.5f);
	public float Radius => Random.Range(radiusRange.x, radiusRange.y);
	
	[SerializeField] private Vector2 radiusDecayRange = new Vector2(0.05f, 0.15f);
	public float RadiusDecay => Random.Range(radiusDecayRange.x, radiusDecayRange.y);
	public RadiusDecayMode RadiusDecayMode = RadiusDecayMode.PER_BRANCH_TRANSFER_RADIUS;
	
	[Header("Node Generation")]
	[SerializeField] private Vector2 nodeFrequencyRange = new Vector2(0.3f, 1f);
	public float NodeFrequency => Random.Range(nodeFrequencyRange.x, nodeFrequencyRange.y);
	
	[SerializeField] private Vector2 nodeXAngleRange = new Vector2(-15f, 15f);
	[SerializeField] private Vector2 nodeZAngleRange = new Vector2(-15f, 15f);
	public Vector3 NodeAngle => new Vector3(
		Random.Range(nodeXAngleRange.x, nodeXAngleRange.y),
		0f,
		Random.Range(nodeZAngleRange.x, nodeZAngleRange.y)
	);
	
	public Vector2 MaxNodeXAngleDeviation = new Vector2(-45f, 45f);
	public Vector2 MaxNodeZAngleDeviation = new Vector2(-45f, 45f);
	
	[Header("Leaf Generation")]
	[SerializeField] private Vector2 leafSpawnRadiusThreshold = new Vector2(0.05f, 0.1f);
	public float LeafSpawnRadiusThreshold => Random.Range(leafSpawnRadiusThreshold.x, leafSpawnRadiusThreshold.y);
	
	[SerializeField] private Vector2 leafProbabilityRange = new Vector2(0.3f, 1f);
	public float LeafProbability => Random.Range(leafProbabilityRange.x, leafProbabilityRange.y);
	
	[SerializeField] private Vector2Int leafCountRange = new Vector2Int(1, 2);
	public int LeafCount => Random.Range(leafCountRange.x, leafCountRange.y);
	
	[SerializeField] private Vector2 leafRadiusDecayRange = new Vector2(0.005f, 0.01f);
	public float LeafRadiusDecay => Random.Range(leafRadiusDecayRange.x, leafRadiusDecayRange.y);
	
	[Header("Branching")]
	[SerializeField] private Vector2 minBranchingHeightRange = new Vector2(0f, 0f);
	public float MinBranchingHeight => Random.Range(minBranchingHeightRange.x, minBranchingHeightRange.y);
	
	[SerializeField] private Vector2 branchingProbabilityRange = new Vector2(0.3f, 1f);
	public float BranchingProbability => Random.Range(branchingProbabilityRange.x, branchingProbabilityRange.y);
	
	[SerializeField] private Vector2Int branchingCountRange = new Vector2Int(1, 2);
	public int BranchingCount => Random.Range(branchingCountRange.x, branchingCountRange.y);
	
	[SerializeField] private Vector2 branchingAngleRange = new Vector2(-70f, 70f);
	[SerializeField] private AnimationCurve branchingPositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Vector3[] GenerateBranchingDirections(int count) {
		Vector3[] directions = new Vector3[count];
		float timeSectorSize = 1 / count; // distribute the time on an animation curve (0-1) into sectors for each angle
		for (int i = 0; i < count; i++) {
			float time = timeSectorSize * i + Random.Range(0, timeSectorSize); // randomize the time within the sector
			float curveValue = branchingPositionCurve.Evaluate(time);
			Vector3 direction = Vector3.right;
			direction = Quaternion.Euler(0f, curveValue * 360f, Random.Range(branchingAngleRange.x, branchingAngleRange.y)) * direction;
			directions[i] = direction;
		}
		return directions;
	}

}

public enum RadiusDecayMode {
	PER_BRANCH_TRANSFER_RADIUS,
	PER_BRANCH_STATIC_RADIUS,
	PER_DISTANCE
}

public static class RadiusDecayModeExtension {
	public static bool IsPerBranch(this RadiusDecayMode mode) {
		return mode == RadiusDecayMode.PER_BRANCH_STATIC_RADIUS || mode == RadiusDecayMode.PER_BRANCH_TRANSFER_RADIUS;
	}

	public static bool IsPerDistance(this RadiusDecayMode mode) {
		return mode == RadiusDecayMode.PER_DISTANCE;
	}
}
}