using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using DefaultNamespace.AI.PathFinding;
using DefaultNamespace.TreeGenerator;
using UnityEngine;
using UnityEngine.VFX;
using VectorFieldPhysics;
using Debug = UnityEngine.Debug;
using Tree = DefaultNamespace.TreeGenerator.Tree;

namespace Simulation {
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TreeSpawn : MonoBehaviour {
	
	[SerializeField] private TreeGenConfig config;
	[SerializeField] private int seed;
	[SerializeField] private Tornado tornado;
	[SerializeField] private VisualEffectAsset leafLossEffect;
	[SerializeField] private InfluenceGrid influenceGrid;
		
	[SerializeField] private Material barkMaterial;
	[SerializeField] private Material leafMaterial;
	[SerializeField] private float spawnDuration;
	[SerializeField] private float leafLossRadius;


	private Tree tree = null;
	private PreparedMeshData? treeMeshData = null;
	private GraphicsBuffer leafPositions = null;
	private GraphicsBuffer leafRotations = null;
	private GraphicsBuffer leafSizes = null;
	private int leafCount;
	
	private float? spawnTime = null;
	private bool fullyGrown = false;
	private bool checkedProximity = false;
	private bool meshDataGenerated = false;
	private bool texturesGenerated = false;
	private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
	
	// utility for setting up all the data when spawning from script
	public void InitFromScript(int seed, Tornado tornado, InfluenceGrid influenceGrid) {
		this.seed = seed;
		this.tornado = tornado;
		this.influenceGrid = influenceGrid;
	}

	private void Start() {
		GenerateTreeMeshAsync();
	}

	private void GenerateTreeMeshAsync() {
		Thread thread = new Thread(() => {
			tree = new Tree(config, seed);
			treeMeshData = tree.PrepareMeshData();
		});
		thread.Start();
	}

	private void Update() {
		if (spawnTime is null) CheckSpawn();
		else if (!fullyGrown) CheckGrow();
		else if (!checkedProximity) {
			leafCount = GenerateLeafData(GetComponent<MeshFilter>().mesh, out leafPositions, out leafRotations, out leafSizes);
			CheckTornadoProximity();
			checkedProximity = true;
		}
	}

	private void CheckSpawn() {
		// wait for the the spawner to be on the ground and the mesh to be generated
		if (transform.position.y > 0 || treeMeshData is null) return; // no need to do unity null check here (==null), we just wanna check if the thread has finished generating the mesh
		Mesh treeMesh = tree.GenerateMesh(treeMeshData.Value, false);
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = treeMesh;
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.materials = new Material[] {barkMaterial, leafMaterial};
		spawnTime = Time.time;
		Obstacle obstacle = gameObject.AddComponent<Obstacle>();
		obstacle.influenceGrid = influenceGrid;
		obstacle.bounds = new Vector2(tree.Branches[0].Nodes[0].Radius * 2, tree.Branches[0].Nodes[0].Radius * 2);
	}

	private void CheckGrow() {
		// grow the scale of the tree over spawnDuration seconds
		float scale = (Time.time - spawnTime.Value) / spawnDuration;
		if (scale >= 1) {
			scale = 1;
			fullyGrown = true;
		}
		transform.localScale = Vector3.one * scale;
	}
	
	private void CheckTornadoProximity() {
		// if tree is within tornado's leafLossRadius, remove leaf material and play leaf loss effect
		if (tornado == null) return;
		float distanceSqr = (transform.position - tornado.transform.position).sqrMagnitude;
		if (distanceSqr > leafLossRadius * leafLossRadius) return;
		
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.materials = new Material[] {barkMaterial};
		
		// get data from tornado vfx
		VisualEffect tornadoVfx = tornado.GetComponent<VisualEffect>();
		Texture3D tornadoVectorField = (Texture3D) tornadoVfx.GetTexture("VectorField");
		float tornadoVectorFieldScale = tornadoVfx.GetFloat("VectorFieldScale");
		float tornadoVectorFieldOffset = tornadoVfx.GetFloat("VectorFieldOffset");
		float tornadoHeight = tornadoVfx.GetFloat("Height");
		Vector2 tornadoRadiusRange = tornadoVfx.GetVector2("RadiusRange");

		VisualEffect leafLossEffectInstance = gameObject.AddComponent<VisualEffect>();
		leafLossEffectInstance.visualEffectAsset = leafLossEffect;
		leafLossEffectInstance.SetInt("LeafCount", leafCount);
		leafLossEffectInstance.SetGraphicsBuffer("LeafPositions", leafPositions);
		leafLossEffectInstance.SetGraphicsBuffer("LeafRotations", leafRotations);
		leafLossEffectInstance.SetGraphicsBuffer("LeafSizes", leafSizes);
		leafLossEffectInstance.SetFloat("MaxRotationSpeed", 1f);
		leafLossEffectInstance.SetTexture("LeafTexture", leafMaterial.GetTexture(MAIN_TEX));
		leafLossEffectInstance.SetFloat("PullSpeed", tornado.MaxSpeed);
		leafLossEffectInstance.SetVector3("TornadoPosition", tornado.transform.position);
		leafLossEffectInstance.SetTexture("TornadoVectorField", tornadoVectorField);
		leafLossEffectInstance.SetFloat("TornadoVectorFieldScale", tornadoVectorFieldScale);
		leafLossEffectInstance.SetFloat("TornadoVectorFieldOffset", tornadoVectorFieldOffset);
		leafLossEffectInstance.SetFloat("TornadoHeight", tornadoHeight);
		leafLossEffectInstance.SetVector2("TornadoRadiusRange", tornadoRadiusRange);
		leafLossEffectInstance.Play();
	}

	private int GenerateLeafData(Mesh mesh, out GraphicsBuffer leafPositions, out GraphicsBuffer leafRotations, out GraphicsBuffer leafSizes) {
		int[] leafTriangles = mesh.GetTriangles(1);
		Vector3[] vertices = mesh.vertices;
		int leafCount = leafTriangles.Length / 6;
		Matrix4x4 toWorld = transform.localToWorldMatrix;
		
		Vector3[] leafPositionsArray = new Vector3[leafCount];
		Vector3[] leafRotationsArray = new Vector3[leafCount];
		Vector2[] leafSizesArray = new Vector2[leafCount];
		
		int index = 0;
		for (int i = 0; i < leafTriangles.Length; i += 6) { // 2 triangles per leaf
			// only use unique vertices, THIS DEPENDS ON THE MESH GENERATION ALGORITHM, dirty but performance friendly
			Vector3 bottomRight = vertices[leafTriangles[i]];
			Vector3 bottomLeft = vertices[leafTriangles[i + 1]];
			Vector3 bottomCenter = (bottomRight + bottomLeft) / 2;
			Vector3 center = bottomRight + bottomLeft + vertices[leafTriangles[i + 2]] + vertices[leafTriangles[i + 4]];
			center /= 4;
			Vector3 centerWorld = toWorld.MultiplyPoint(center);
			leafPositionsArray[index] = centerWorld;

			Vector3 up = center - bottomCenter;
			Vector3 right = bottomRight - bottomLeft;
			Vector3 forward = Vector3.Cross(up, right);
			Vector3 rotation = Quaternion.LookRotation(forward, up).eulerAngles;
			leafRotationsArray[index] = rotation;
			
			float xSize = Vector3.Distance(bottomLeft, bottomRight);
			float ySize = Vector3.Distance(bottomCenter, center) * 2;
			leafSizesArray[index] = new Vector2(xSize, ySize);

			index++;
		}
		
		leafPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, leafCount, 3 * sizeof(float));
		leafPositions.SetData(leafPositionsArray);
		leafRotations = new GraphicsBuffer(GraphicsBuffer.Target.Structured, leafCount, 3 * sizeof(float));
		leafRotations.SetData(leafRotationsArray);
		leafSizes = new GraphicsBuffer(GraphicsBuffer.Target.Structured, leafCount, 2 * sizeof(float));
		leafSizes.SetData(leafSizesArray);
		
		// save leaf positions texture to png file in downloads folder
		// File.WriteAllBytes("C:\\Users\\Arno\\Downloads\\leafPositionsTexture.png", leafPositionsTexture.EncodeToPNG());

		return leafCount;
	}

	private void OnDestroy() {
		leafPositions?.Release();
		leafRotations?.Release();
		leafSizes?.Release();
	}
}
}