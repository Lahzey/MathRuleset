using System.Collections.Generic;
using UnityEngine;

namespace CellularAutomata {
public class SqrtCache {
	
	public static readonly SqrtCache INSTANCE = new SqrtCache();

	private readonly Dictionary<float, float> cache = new Dictionary<float, float>();
	
	private SqrtCache() { }

	public float this[float f] {
		get {
			if (cache.ContainsKey(f)) return cache[f];
			else {
				float sqrt = Mathf.Sqrt(f);
				cache.Add(f, sqrt);
				return sqrt;
			}
		}
		set { }
	}
	
}
}