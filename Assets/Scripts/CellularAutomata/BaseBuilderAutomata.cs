using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CellularAutomata {
[RequireComponent(typeof(MeshRenderer))]
public class BaseBuilderAutomata : MonoBehaviour {
	
	private static readonly int CELL_SIZE = 16;

	[SerializeField] private int width = 256;
	[SerializeField] private int height = 256;
	
	private int seed;
	private bool running = false;
	private float tickDelay = 1f;
	private float storedDeltaTime = 0f;
	
	private Dictionary<CellRule, int> cellRuleMultipliers = new Dictionary<CellRule, int>();

	private Texture2D texture;
	private InfiniteGrid<Cell> grid = new InfiniteGrid<Cell>();

	private void Awake() {
		texture = new Texture2D(width * CELL_SIZE, height * CELL_SIZE);
		GetComponent<MeshRenderer>().material.mainTexture = texture;
		GetComponent<MeshRenderer>().material.mainTexture.filterMode = FilterMode.Point;
	}

	// Start is called before the first frame update
	void Start() {
		seed = Random.Range(0, 100000);
		foreach (CellRule rule in CellRule.RULES) {
			switch (rule.cell) {
				case Cell.LUMBERJACK:
					cellRuleMultipliers.Add(rule, 10);
					break;
				case Cell.STONE_MINE:
					cellRuleMultipliers.Add(rule, 5);
					break;
				case Cell.FARM:
					cellRuleMultipliers.Add(rule, 5);
					break;
				case Cell.FISHING_HUT:
					cellRuleMultipliers.Add(rule, 5);
					break;
				case Cell.GOLD_MINE:
					cellRuleMultipliers.Add(rule, 0);
					break;
				default:
					cellRuleMultipliers.Add(rule, -5);
					break;
			}
		}
		
		
		grid.MinX = 0;
		grid.MaxX = width - 1;
		grid.MinY = 0;
		grid.MaxY = height - 1;
		
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				grid[x, y] = Cell.GRASS;
				int offset = seed;
				foreach (Cell cell in new Cell[] { Cell.TREE, Cell.STONE, Cell.GOLD, Cell.WATER }) {
					offset += 10000;
					float noise = Mathf.PerlinNoise(x * cell.GetNoiseScale() + offset, y * cell.GetNoiseScale() + offset);
					if (noise < cell.GetNoiseThreshold()) {
						grid[x, y] = cell;
					}
				}
			}
		}
		
		Redraw();
	}

	// Update is called once per frame
	void Update() {
		if (!running) return;
		
		storedDeltaTime += Time.deltaTime;
		if (storedDeltaTime >= tickDelay) {
			storedDeltaTime = 0f;
			EvaluateRules();
		}
	}

	private void EvaluateRules() {
		float bestScore = 0f;
		CellRule bestRule = null;
		int bestX = 0, bestY = 0;
		
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				Cell cell = grid[x, y];
				if (cell.IsBuildable()) continue;

				foreach (CellRule rule in CellRule.RULES) {
					float score = rule.Evaluate(grid, x, y) * cellRuleMultipliers[rule];
					if (score > bestScore) {
						bestScore = score;
						bestRule = rule;
						bestX = x;
						bestY = y;
					}
				}
			}
		}

		if (bestRule != null && bestScore > 0.1f) { // if bestScore is too low, the rule is not applied as such a placement would be too random
			bestRule.Apply(grid, bestX, bestY);
			cellRuleMultipliers[bestRule] /= 2;
		} else Debug.Log("No Rule could be applied.");

		Redraw();
	}

	private void Redraw() {
		Color32[] colors = new Color32[width * CELL_SIZE * height * CELL_SIZE];
		int index = 0;
		for (int y = height - 1; y >= 0; y--) { // because unity textures begin in the lower left corner
			for (int cellY = 0; cellY < CELL_SIZE; cellY++) {
				for (int x = 0; x < width; x++) {
					for (int cellX = 0; cellX < CELL_SIZE; cellX++) {
						Texture2D cellTexture = grid[x, y].GetTexture();
						colors[index++] = cellTexture.GetPixel(cellX, cellY);
					}
				}
			}
		}
		
		texture.SetPixels32(colors);
		texture.Apply();
	}
}
}