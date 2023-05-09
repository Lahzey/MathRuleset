using UnityEngine;

namespace DefaultNamespace.TreeGenerator {
public struct TreeGenConfig {

	/** the height of the tree */
	public Vector2 treeHeightRange;
	
	/** the radius of the trunk at the base */
	public Vector2 trunkRadiusRange;
	
	/** how far out from the center branches can grow */
	public Vector2 branchExtentRange;
	
	/** the height at which the first branch appears */
	public Vector2 firstBranchHeightRange;
	
	/** how many branches an individual branch / trunk can spawn each meter (scaled down to size for branches) */
	public Vector2 branchFrequencyRange;
	
	/** the angle at which branches can spawn */
	public Vector2 branchAngleRange;
	
	/** the length of the branches (scaled down to size of branch) */
	public Vector2 branchLengthRange;
	
	/** how many times the tree can branch, 0 = only trunk, 1 = trunk can spawn branches, 2 = branches from trunk can also spawn branches, etc... */
	public Vector2 branchingCountRange;
	
	/** the size of the spawned leaves */
	public Vector2 leavesSizeRange;

	/** the chance for the tree to spawn without leaves */
	public float deadChance;
	
	/** the  chance for individual branches to be cut off */
	public float branchCutChance;
	
	/** material used for the main trunk */
	public Material trunkMaterial;
	
	/** material used for the branches */
	public Material branchMaterial;
	
	/** material used for any wood exposed at cut off branches */
	public Material woodMaterial;
	
	/** material used for the leaves */
	public Material leavesMaterial;
	
	
	public static TreeGenConfig Default() {
		TreeGenConfig config = new TreeGenConfig();
		config.treeHeightRange = new Vector2(5, 15);
		config.trunkRadiusRange = new Vector2(0.25f, 0.75f);
		config.branchExtentRange = new Vector2(3f, 10f);
		config.firstBranchHeightRange = new Vector2(0.5f, 0.75f);
		config.branchFrequencyRange = new Vector2(0.5f, 4f);
		config.branchAngleRange = new Vector2(45f, 315f);
		config.branchLengthRange = new Vector2(2f, 5f);
		config.branchingCountRange = new Vector2(1, 4);
		config.leavesSizeRange = new Vector2(0.05f, 0.1f);
		config.deadChance = 0.05f;
		config.branchCutChance = 0.125f;
		return config;
	}
}
}