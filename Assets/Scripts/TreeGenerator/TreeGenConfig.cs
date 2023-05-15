using System;
using UnityEngine;

namespace DefaultNamespace.TreeGenerator {
[CreateAssetMenu(fileName = "TreeConfig", menuName = "ScriptableObjects/TreeGeneratorConfig", order = 1)]
public class TreeGenConfig : ScriptableObject {
	
	[SerializeField] private Vector2Int branchingDepthRange = new Vector2Int(4, 4);
	public int BranchingDepth => UnityEngine.Random.Range(branchingDepthRange.x, branchingDepthRange.y);
	
	[SerializeField] private BranchGenConfig[] branchConfigs;
	public BranchGenConfig this[int depth] => branchConfigs[Math.Min(depth, branchConfigs.Length - 1)];
}
}