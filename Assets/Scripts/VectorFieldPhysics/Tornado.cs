using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

namespace VectorFieldPhysics {
[RequireComponent(typeof(VisualEffect))]
public class Tornado : MonoBehaviour, VectorField {
	private static readonly Vector3[] DIRECTIONS = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

	[SerializeField] private float maxSpeed = 10f;
	[SerializeField] private float height = 10f;
	[SerializeField] [Range(0, 1)] private float startHeight = 0.1f;
	[SerializeField] private bool clockwise = true;

	private Vector3 offset = new Vector3(0, 1, 0);

	private void OnValidate() {
		offset = new Vector3(0, startHeight * height, 0);
		UpdateVisualEffect();
	}

	public float GetRadius() => GetRadius(height);

	public float GetRadius(float y) {
		return GetEyeSize(y) + y * 0.4f; // to be corrected if the GetVector function is changed
	}

	public Vector3 GetVector(Vector3 position) {
		position += offset;
		if (position.y <= 0 || position.y > height || (position.x == 0 && position.z == 0)) return Vector3.zero;

		float maxComponentSize = ConvertToLogGrowth((height - position.y) / height, 10f); // make wind fall of at the top (value between 0 and 1, will be scaled with maxSpeed later)
		float eyeSize = GetEyeSize(position.y);
		float ringCenter = eyeSize + position.y * 0.2f;
		float ringThickness = ringCenter - eyeSize;
		float distToCenter = Mathf.Sqrt(position.x * position.x + position.z * position.z);
		float distToRingCenter = Mathf.Abs(distToCenter - ringCenter);
		float distToRingCenterPercent = 1 - Mathf.Min(distToRingCenter / ringThickness, 1);

		// wind ring
		Vector2 direction = new Vector2(position.z * (clockwise ? 1 : -1), position.x * (clockwise ? -1 : 1)); // creates a circle around 0/0
		direction /= distToCenter; // normalize
		float componentSize = distToRingCenterPercent * maxComponentSize;
		Vector3 unscaledVector = new Vector3(direction.x * componentSize, 0, direction.y * componentSize);

		// suction towards the center
		Vector3 suctionToCenter = new Vector3(-position.x / distToCenter, 0, -position.z / distToCenter);
		float suctionStrength = distToCenter > ringCenter ? LogGrowthConstantFalloff(1 - (distToCenter - ringCenter) / ringThickness, 0.5f, 3f) : 0;
		unscaledVector += suctionToCenter * (suctionStrength * maxComponentSize);

		// suction upwards
		unscaledVector += Vector3.up * (distToRingCenterPercent * 0.5f * maxComponentSize); // upwards speed can only be 0.5f as fast as the horizontal speed

		return unscaledVector * maxSpeed;
	}

	private float GetEyeSize(float y) {
		return 0.01f + 0.05f * y;
	}

	private void UpdateVisualEffect() {
		VisualEffect visualEffect = GetComponent<VisualEffect>();
		Vector3 resolution = visualEffect.GetVector3("Resolution");
		Vector3 halfRes = resolution / 2;
		Vector3Int resInt = new Vector3Int((int)resolution.x, (int)resolution.y, (int)resolution.z);

		Texture3D texture = new Texture3D(resInt.x, resInt.y, resInt.z, TextureFormat.RGBAFloat, false);
		Vector3[,,] pixels = new Vector3[resInt.x, resInt.y, resInt.z];

		// used to convert image coordinates to world coordinates
		float diameter = GetRadius(height) * 2;
		float xMod = diameter / resolution.x;
		float yMod = (height - offset.y) / resolution.y;
		float zMod = diameter / resolution.z;
		
		// calculate the vector for each pixel and store the min/max values so we can scale the vectors later
		float minVal = float.MaxValue;
		float maxVal = -float.MaxValue;
		for (int x = 0; x < resolution.x; x++) {
			for (int y = 0; y < resolution.y; y++) {
				for (int z = 0; z < resolution.z; z++) {
					Vector3 position = new Vector3((x - halfRes.x) * xMod, y * yMod, (z - halfRes.z) * zMod);
					Vector3 vector = GetVector(position);
					if (vector.x > maxVal) maxVal = vector.x;
					if (vector.y > maxVal) maxVal = vector.y;
					if (vector.z > maxVal) maxVal = vector.z;
					if (vector.x < minVal) minVal = vector.x;
					if (vector.y < minVal) minVal = vector.y;
					if (vector.z < minVal) minVal = vector.z;
					pixels[x, y, z] = vector;
				}
			}
		}
		
		// scale down the vectors so the max value is 1 (colors only accept values between 0 and 1) and apply them to the texture
		float scale = maxVal - minVal;
		for (int x = 0; x < resolution.x; x++) {
			for (int y = 0; y < resolution.y; y++) {
				for (int z = 0; z < resolution.z; z++) {
					Vector3 color = (pixels[x, y, z] - new Vector3(minVal, minVal, minVal)) / scale;
					texture.SetPixel(x, y, z, new Color(color.x, color.y, color.z));
				}
			}
		}
		texture.Apply();

		visualEffect.SetTexture("VectorField", texture);
		visualEffect.SetFloat("VectorFieldScale", scale);
		visualEffect.SetFloat("VectorFieldOffset", minVal);
		visualEffect.SetFloat("Height", height - offset.y);
		visualEffect.SetVector2("RadiusRange", new Vector2(GetRadius(offset.y), GetRadius(height)));
	}

	private void OnDrawGizmos() {
		float minY = offset.y;
		float maxY = height;
		float effectiveHeight = height - offset.y;

		Gizmos.DrawCube(Vector3.zero, Vector3.one);

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

	private static float LogGrowthConstantFalloff(float value, float peak, float growth) {
		if (peak < 0 || peak > 1) throw new ArgumentException("peak must be between 0 and 1", nameof(peak));
		if (value < 0 || value > 1) return 0;
		float growthPartMod = 1 / peak;
		float falloffPartMod = 1 / (1 - peak);
		if (value < peak) return ConvertToLogGrowth(value * growthPartMod, growth);
		else return 1 - (value - peak) * falloffPartMod;
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