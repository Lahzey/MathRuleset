using UnityEngine;

namespace DefaultNamespace.AI.PathFinding {
public enum InfluenceType {
	ENERGY,
	DANGER,
	TARGET
}

public static class InfluenceTypeExtensions {

	public static Color GetColor(this InfluenceType influenceType) {
		switch (influenceType) {
			case InfluenceType.ENERGY:
				return Color.green;
			case InfluenceType.DANGER:
				return Color.red;
			case InfluenceType.TARGET:
				return Color.blue;
			default:
				return Color.white;
		}
	}
}
}