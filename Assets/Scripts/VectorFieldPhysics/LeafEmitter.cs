using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VectorFieldPhysics {
public class LeafEmitter : MonoBehaviour {
	
	[SerializeField] public List<MonoBehaviour> vectorFields = new List<MonoBehaviour>();
	[SerializeField] private GameObject leafPrefab;
	[SerializeField] private float spawnRadius = 1f;
	[SerializeField] private int spawnCount = 100;
	[SerializeField] private float spawnRate = 10f;

	private float spawnDelay => 1 / spawnRate;
	private float storedDeltaTime = 0;
	private GameObject[] leaves;
	private int index = 0;
	
	// Start is called before the first frame update
	void Start() {
		leaves = new GameObject[spawnCount];
	}

	// Update is called once per frame
	void Update() {
		storedDeltaTime += Time.deltaTime;
		if (storedDeltaTime > spawnDelay) {
			storedDeltaTime = 0;
			// get a random position within the spawnRadius
			Vector2 randomPosition = Random.insideUnitCircle * spawnRadius;
			Vector3 spawnPosition = new Vector3(transform.position.x + randomPosition.x, transform.position.y, transform.position.z + randomPosition.y);

			if (leaves[index] != null) {
				leaves[index].transform.position = spawnPosition;
				leaves[index].GetComponent<VectorPhysics>().velocity = Vector3.up * 2f;
			}
			else {
				// spawn new instance of leafPrefab
				leaves[index] = Instantiate(leafPrefab, spawnPosition, Quaternion.identity);
				leaves[index].GetComponent<VectorPhysics>().vectorFields.AddRange(vectorFields);
				leaves[index].GetComponent<VectorPhysics>().velocity = Vector3.up * 2f;
			}

			index++;
			if (index >= spawnCount) index = 0;
		}
	}

	private void OnDrawGizmos() {
		Handles.DrawWireDisc(transform.position, Vector3.up, spawnRadius);
	}
}
}