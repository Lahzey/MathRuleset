using UnityEngine;

namespace DefaultNamespace {
public static class Vector3Extensions {
	public static Vector2 To2D(this Vector3 self) {
		return new Vector2(self.x, self.z);
	}
}

public static class Vector2Extensions {
	public static Vector3 To3D(this Vector2 self, float y = 0) {
		return new Vector3(self.x, y, self.y);
	}
}
}