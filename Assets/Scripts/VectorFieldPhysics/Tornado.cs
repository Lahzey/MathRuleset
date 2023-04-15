using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace VectorFieldPhysics {
public class Tornado : MonoBehaviour, VectorField {

	private static readonly Vector3[] DIRECTIONS = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

	[SerializeField] private float maxSpeed = 10f;
	[SerializeField] private float height = 10f;
	[SerializeField][Range(0, 1)] private float startHeight = 0.1f;
	[SerializeField] private bool clockwise = true;
	
	private Vector3 offset = new Vector3(0, 1, 0);

	private void OnValidate() {
		offset = new Vector3(0, startHeight * height, 0);
	}

	public float GetRadius() => GetRadius(height);

	public float GetRadius(float y) {
		return GetEyeSize(y) + y * 0.4f; // to be corrected if the GetVector function is changed
	}

	// the returned vector has components between -1 and 1, where an x and z of +-1 is the max horizontal strength
	public Vector3 GetVector(Vector3 position) {
		position += offset;
		if (position.y <= 0 || position.y > height || (position.x == 0 && position.z == 0)) return Vector3.zero;

		float maxComponentSize = ConvertToLogGrowth((height - position.y) / height, 10f);
		float eyeSize = GetEyeSize(position.y);
		float ringCenter = eyeSize + position.y * 0.2f;
		float ringThickness = ringCenter - eyeSize;
		float distToCenter = Mathf.Sqrt(position.x * position.x + position.z * position.z);
		float distToRingCenter = Mathf.Abs(distToCenter - ringCenter);
		float componentSize = (1 - Mathf.Min(distToRingCenter / ringThickness, 1)) * maxComponentSize;
		
		Vector2 direction = new Vector2(position.z * (clockwise ? 1 : -1), position.x * (clockwise ? -1 : 1)); // creates a circle around 0/0
		direction /= distToCenter; // normalize
		float turnedToCenter = (distToCenter - ringCenter) / ringThickness * 0.5f; // how much the direction should turn towards the ring center (0 - 0.3), negative if inside the ring so we turn the other way
		direction.x = direction.x * (1 - turnedToCenter) + direction.y * turnedToCenter;
		direction.y = direction.y * (1 - turnedToCenter) + direction.x * turnedToCenter;

		Vector3 unscaledVector = new Vector3(direction.x * componentSize, componentSize / 3, direction.y * componentSize);
		return unscaledVector * maxSpeed;
	}

	private float GetEyeSize(float y) {
		return 0.01f + 0.05f * y;
	}

	private void OnDrawGizmos() {
		float minY = offset.y;
		float maxY = height;
		float effectiveHeight = height - offset.y;

		// draw the eye
		float minEyeSize = GetEyeSize(minY);
		float maxEyeSize = GetEyeSize(maxY);
		DrawConeCylinder(transform.position, minEyeSize, maxEyeSize, effectiveHeight);

		// draw the outer edge
		float minRadius = GetRadius(minY);
		float maxRadius = GetRadius(maxY);
		DrawConeCylinder(transform.position, minRadius, maxRadius, effectiveHeight);

		// connect the two shapes at top and bottom
		foreach (Vector3 dir in DIRECTIONS) {
			Handles.DrawLine(transform.position + dir * minEyeSize, transform.position + dir * minRadius);
			Handles.DrawLine(transform.position + new Vector3(0, effectiveHeight, 0) + dir * maxEyeSize, transform.position + new Vector3(0, effectiveHeight, 0) + dir * maxRadius);
		}
	}

	private static void DrawConeCylinder(Vector3 position, float minRadius, float maxRadius, float height) {
		Handles.DrawWireDisc(position, Vector3.up, minRadius);
		Handles.DrawWireDisc(position + new Vector3(0, height, 0), Vector3.up, maxRadius);
		foreach (Vector3 dir in DIRECTIONS) {
			Handles.DrawLine(position + dir * minRadius, position + new Vector3(0, height, 0) + dir * maxRadius);
		}
	}

	/// <summary>
	/// Converts a "X" value to a "Y" value using a log growth function so that X=0 -> Y=0 and X=1 -> Y=1.
	/// </summary>
	/// <param name="value">the value to convert</param>
	/// <param name="growth">a value of zero gives an even growth. The bigger the growth the steeper the curve gets initially (and the flatter at the end). Negative values instead create exponential growth.</param>
	/// <returns></returns>
	private static float ConvertToLogGrowth(float value, float growth) {
		return (1 - Mathf.Exp(-growth * value)) / (1 - Mathf.Exp(-growth));
	}

}
}