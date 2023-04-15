using UnityEngine;

namespace VectorFieldPhysics {
public interface VectorField {

	public Vector3 GetVector(Vector3 position);
}
}