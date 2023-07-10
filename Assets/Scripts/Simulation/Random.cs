using System;

namespace Simulation {
public class Random {

	private System.Random random;
	public float value => (float) random.NextDouble();

	public Random(int seed) {
		random = new System.Random(seed);
	}
		
	public float Range(float minInclusive, float maxExclusive) {
		if (minInclusive > maxExclusive) (minInclusive, maxExclusive) = (maxExclusive, minInclusive);
		return (float) random.NextDouble() * (maxExclusive - minInclusive) + minInclusive;
	}
		
	public int Range(int minInclusive, int maxExclusive) {
		if (minInclusive > maxExclusive) (minInclusive, maxExclusive) = (maxExclusive, minInclusive);
		return random.Next(minInclusive, maxExclusive);
	}
}
}