using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = Simulation.Random;

namespace DefaultNamespace.TreeGenerator {

[CreateAssetMenu(fileName = "BranchConfig", menuName = "ScriptableObjects/BranchGeneratorConfig", order = 2)]
public class BranchGenConfig : ScriptableObject {
	
	[SerializeField] private Vector2 maxLengthRange = new Vector2(50f, 50f);
	public float MaxLength(Random random) => random.Range(maxLengthRange.x, maxLengthRange.y);

	[Header("Radius")]
	[SerializeField] private Vector2 radiusRange = new Vector2(0.5f, 1.5f);
	public float Radius(Random random) => random.Range(radiusRange.x, radiusRange.y);
	
	[SerializeField] private Vector2 radiusDecayRange = new Vector2(0.05f, 0.15f);
	public float RadiusDecay(Random random) => random.Range(radiusDecayRange.x, radiusDecayRange.y);
	public RadiusDecayMode RadiusDecayMode = RadiusDecayMode.PER_BRANCH_TRANSFER_RADIUS;
	
	[Header("Node Generation")]
	[SerializeField] private Vector2 nodeFrequencyRange = new Vector2(0.3f, 1f);
	public float NodeFrequency(Random random) => random.Range(nodeFrequencyRange.x, nodeFrequencyRange.y);
	
	[SerializeField] private Vector2 nodeXAngleRange = new Vector2(-15f, 15f);
	[SerializeField] private Vector2 nodeZAngleRange = new Vector2(-15f, 15f);
	public Vector3 NodeAngle(Random random) => new Vector3(
		random.Range(nodeXAngleRange.x, nodeXAngleRange.y),
		0f,
		random.Range(nodeZAngleRange.x, nodeZAngleRange.y)
	);
	
	public Vector2 MaxNodeXAngleDeviation = new Vector2(-45f, 45f);
	public Vector2 MaxNodeZAngleDeviation = new Vector2(-45f, 45f);
	
	[Header("Leaf Generation")]
	[SerializeField] private Vector2 leafSpawnRadiusThreshold = new Vector2(0.05f, 0.1f);
	public float LeafSpawnRadiusThreshold(Random random) => random.Range(leafSpawnRadiusThreshold.x, leafSpawnRadiusThreshold.y);
	
	[SerializeField] private Vector2 minLeafSize = new Vector2(0.1f, 0.2f);
	[SerializeField] private Vector2 maxLeafSize = new Vector2(0.1f, 0.2f);
	public Vector2 LeafSize(Random random) => new Vector2(
		random.Range(minLeafSize.x, maxLeafSize.x),
		random.Range(minLeafSize.y, maxLeafSize.y)
	);
	
	[SerializeField] private Vector2 leafProbabilityRange = new Vector2(0.3f, 1f);
	public float LeafProbability(Random random) => random.Range(leafProbabilityRange.x, leafProbabilityRange.y);
	
	[SerializeField] private Vector2Int leafCountRange = new Vector2Int(1, 2);
	public int LeafCount(Random random) => random.Range(leafCountRange.x, leafCountRange.y);
	
	[SerializeField] private Vector2 leafRadiusDecayRange = new Vector2(0.005f, 0.01f);
	public float LeafRadiusDecay(Random random) => random.Range(leafRadiusDecayRange.x, leafRadiusDecayRange.y);
	
	[SerializeField] private Vector2 leafAngleRange = new Vector2(-70f, 70f);
	[SerializeField] private AnimationCurve leafPositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	
	[Header("Branching")]
	[SerializeField] private Vector2 minBranchingHeightRange = new Vector2(0f, 0f);
	public float MinBranchingHeight(Random random) => random.Range(minBranchingHeightRange.x, minBranchingHeightRange.y);
	
	[SerializeField] private Vector2 branchingProbabilityRange = new Vector2(0.3f, 1f);
	public float BranchingProbability(Random random) => random.Range(branchingProbabilityRange.x, branchingProbabilityRange.y);
	
	[SerializeField] private Vector2Int branchingCountRange = new Vector2Int(1, 2);
	public int BranchingCount(Random random) => random.Range(branchingCountRange.x, branchingCountRange.y);
	
	[SerializeField] private Vector2 branchingAngleRange = new Vector2(-70f, 70f);
	[SerializeField] private AnimationCurve branchingPositionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Quaternion[] GenerateBranchingDirections(Random random, int count) {
		return GenerateDirections(random, branchingAngleRange, branchingPositionCurve, count);
	}
	
	public Quaternion[] GenerateLeafDirections(Random random, int count) {
		return GenerateDirections(random, leafAngleRange, leafPositionCurve, count);
	}
	
	private Quaternion[] GenerateDirections(Random random, Vector2 angleRange, AnimationCurve positionCurve, int count) {
		Quaternion[] directions = new Quaternion[count];
		if (count == 0) return directions; // early return safes performance and prevents divide-by-zero
		
		float timeSectorSize = 1f / count; // distribute the time on an animation curve (0-1) into sectors for each angle
		for (int i = 0; i < count; i++) {
			// float xRotation = Random.Range(angleRange.x, angleRange.y) * 2f * Mathf.PI / 360f;
			float time = timeSectorSize * i + random.Range(0, timeSectorSize); // randomize the time within the sector
			// float yRotation = positionCurve.Evaluate(time) * 2f * Mathf.PI;
			// directions[i] = new Vector3(Mathf.Sin(xRotation) * Mathf.Sin(yRotation), Mathf.Cos(xRotation), Mathf.Sin(xRotation) * Mathf.Cos(yRotation));
			directions[i] = Quaternion.Euler(random.Range(angleRange.x, angleRange.y), positionCurve.Evaluate(time) * 360f, 0f);
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