using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace.AI.PathFinding {
public class InfluenceGrid : MonoBehaviour {

	private static readonly int MAX_INFLUENCE_RANGE = 20;
	
	[SerializeField] private Vector2Int size;
	[SerializeField] private float scale;
	[SerializeField] private float updateInterval = 1;
	[SerializeField] [Range(0f, 1f)] private float propagationMod = 0.8f;
	[SerializeField] private float influenceDecay = 0.2f;
	
	public float InfluenceDecay => influenceDecay;
	public float Scale => scale;
	
	private Cell[,] cells;
	private float minInfluenceValue;
	
	private float storedDeltaTime;

	private void Awake() {
		cells = new Cell[size.x, size.y];
		minInfluenceValue = Mathf.Pow(influenceDecay, MAX_INFLUENCE_RANGE);
		
		int obstacleMask = LayerMask.GetMask("Obstacle");
		
		for (int x = 0; x < size.x; x++) {
			for (int y = 0; y < size.y; y++) {
				Cell cell = new Cell(new Vector2Int(x, y), this);
				cells[x, y] = cell;
			}
		}
	}

	private void OnDrawGizmos() {
		if (cells == null) return;
		
		foreach (Cell cell in cells) {
			if (cell.IsObstacle) Gizmos.color = Color.red;
			else Gizmos.color = Color.green;
			Gizmos.DrawWireCube(CellToWorld(cell.Position), Vector3.one * scale);
		}
	}

	private void Update() {
		storedDeltaTime += Time.deltaTime;
		
		if (storedDeltaTime >= updateInterval) {
			storedDeltaTime -= updateInterval;
			UpdateInfluences();
		}
	}

	private void UpdateInfluences() {
		Cell cell;
		for (int x = 0; x < size.x; x++) {
			for (int y = 0; y < size.y; y++) {
				cell = cells[x, y];
				foreach (InfluenceType type in Cell.InfluenceTypes) {
					float influence = cell.Influences[type];
					if (cell.DecayInfluence[type]) influence -= influenceDecay;
					else cell.DecayInfluence[type] = true;
					cell.Influences[type] = influence;
				}
			}
		}
	}

	public Vector3 GetInfluenceDirection(Vector3 position, InfluenceType influenceType, bool seekMore) {
		Vector2Int cellPosition = WorldToCell(position);
		Cell cell = cells[cellPosition.x, cellPosition.y];
		Cell targetCell = cell;
		float targetInfluence = cell.Influences[influenceType];
		foreach (Cell neighbour in cell.GetNeighboursArray(false)) {
			float influence = neighbour?.Influences[influenceType] ?? 0;
			if (neighbour != null && (seekMore ? influence > targetInfluence : influence < targetInfluence)) {
				targetCell = neighbour;
				targetInfluence = influence;
			}
		}
		
		return CellToWorld(targetCell.Position);
	}
	
	public float GetInfluence(Vector3 position, InfluenceType influenceType) {
		Vector2Int cellPosition = WorldToCell(position);
		return cells[cellPosition.x, cellPosition.y].Influences[influenceType];
	}

	public void SetInfluence(Vector3 position, InfluenceType influenceType, float influence, bool overwriteIfLess = false) {
		Vector2Int cellPosition = WorldToCell(position);
		SetInfluence(cells[cellPosition.x, cellPosition.y], influenceType, influence, overwriteIfLess);
	}

	private void SetInfluence(Cell cell, InfluenceType influenceType, float influence, bool overwriteIfLess = false) {
		if (!overwriteIfLess && cell.Influences[influenceType] > influence) return;
		if (influence < minInfluenceValue) return; // ignored so we don't propagate through the whole map with tiny values
		
		cell.Influences[influenceType] = influence;
		cell.DecayInfluence[influenceType] = false; // prevent it from decaying in the next update
		foreach (Cell neighbour in cell.GetNeighboursArray(false)) {
			SetInfluence(neighbour, influenceType, influence * propagationMod, overwriteIfLess);
		}
	}

	public void SetObstacle(Vector3 position, bool isObstacle) {
		Vector2Int cellPosition = WorldToCell(position);
		cells[cellPosition.x, cellPosition.y].IsObstacle = isObstacle;
	}

	private Vector2Int WorldToCell(Vector3 position) {
		position -= transform.position;
		Vector2Int cellPos = new Vector2Int(Mathf.FloorToInt(position.x / scale), Mathf.FloorToInt(position.z / scale));
		cellPos.Clamp(Vector2Int.zero, size - Vector2Int.one);
		return cellPos;
	}
	
	private Vector3 CellToWorld(Vector2Int position, int y = 0) {
		float halfScale = scale * 0.5f;
		return new Vector3(position.x * scale + halfScale, y, position.y * scale + halfScale) + transform.position;
	}

	private class Cell {
	
		internal static readonly InfluenceType[] InfluenceTypes = (InfluenceType[]) Enum.GetValues(typeof(InfluenceType));

		internal readonly Vector2Int Position;
		private readonly InfluenceGrid Grid;
	
		public bool IsObstacle { get; internal set; }
		internal readonly Dictionary<InfluenceType, float> Influences;
		internal readonly Dictionary<InfluenceType, bool> DecayInfluence; // by default values decay each update, but when freshly set the don't decay for one update
	
		public Cell(Vector2Int position, InfluenceGrid grid) {
			Position = position;
			Grid = grid;
			Influences = new Dictionary<InfluenceType, float>();
			DecayInfluence = new Dictionary<InfluenceType, bool>();
			foreach (InfluenceType type in InfluenceTypes) {
				Influences[type] = 0;
				DecayInfluence[type] = true;
			}
		}
	
		public void GetNeighbours(bool includeObstacles, out Cell left, out Cell right, out Cell top, out Cell bottom) {
			left = Position.x > 0 ? Grid.cells[Position.x - 1, Position.y] : null;
			right = Position.x < Grid.size.x - 1 ? Grid.cells[Position.x + 1, Position.y] : null;
			top = Position.y > 0 ? Grid.cells[Position.x, Position.y - 1] : null;
			bottom = Position.y < Grid.size.y - 1 ? Grid.cells[Position.x, Position.y + 1] : null;
			if (!includeObstacles) {
				if (left?.IsObstacle??false) left = null;
				if (right?.IsObstacle??false) right = null;
				if (top?.IsObstacle??false) top = null;
				if (bottom?.IsObstacle??false) bottom = null;
			}
		}
		
		public Cell[] GetNeighboursArray(bool includeObstacles) {
			Cell[] neighbours = new Cell[4];
			GetNeighbours(includeObstacles, out neighbours[0], out neighbours[1], out neighbours[2], out neighbours[3]);
			return neighbours;
		}
	}
}
}