using System.Collections.Generic;
using UnityEngine;

namespace TreeGenerator {
public class TreeBranch {

	public Material material;

	public Vector3 position = Vector3.zero;
	public Quaternion rotation = Quaternion.Euler(0, 0, 0);
	public Vector3 localScale = Vector3.one;
	
	public List<TreeBranch> children = new List<TreeBranch>();

}
}