using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AutoUI.Data;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace CellularAutomata {
[RequireComponent(typeof(MeshRenderer))]
public class BaseBuilderAutomata : MonoBehaviour {
	
	private static readonly int CELL_SIZE = 16;

	[SerializeField] private int width = 256;
	[SerializeField] private int height = 256;

	public int Seed { get; private set; }
	public bool Running { get; private set; }
	public float TickDelay = 1f;
	public int TicksSimulated { get; private set; }
	private float storedDeltaTime = 0f;
	
	private Dictionary<CellRule, int> cellRuleMultipliers = new Dictionary<CellRule, int>();

	private Texture2D texture;
	private CellGrid grid;
	
	public void ToggleRunning() {
		Running = !Running;
		// playButtonText.text = running ? "II" : "►";
	}
	
	public void ResetWorld() {
		if (Running) ToggleRunning();
		Seed = Random.Range(0, 100000);
		GenerateWorld();
	}

	private void Awake() {
		DataStore.Instance.Set(DataKeys.AUTOMATA, this);
		texture = new Texture2D(width * CELL_SIZE, height * CELL_SIZE);
		GetComponent<MeshRenderer>().material.mainTexture = texture;
		GetComponent<MeshRenderer>().material.mainTexture.filterMode = FilterMode.Point;
	}

	// Start is called before the first frame update
	void Start() {
		Seed = Random.Range(0, 100000);
		
		GenerateWorld();
	}

	private void GenerateWorld() {
		grid = new CellGrid();
		grid.Resize(width, height);
		cellRuleMultipliers.Clear();
		TicksSimulated = 0;
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
		
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				grid[x, y] = Cell.GRASS;
				int offset = Seed;
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
		if (!Running) return;
		
		storedDeltaTime += Time.deltaTime;
		if (storedDeltaTime >= TickDelay) {
			storedDeltaTime = 0f;
			EvaluateRules();
			TicksSimulated++;
		}
	}

	private void EvaluateRules() {
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		float bestScore = 0f;
		CellRule bestRule = null;
		int bestX = 0, bestY = 0;

		Cell cell;
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				cell = grid[x, y];
				if (!cell.IsBuildable()) continue;

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
		stopwatch.Stop();
		Debug.Log("Evaluation took " + stopwatch.ElapsedMilliseconds + "ms");
		stopwatch.Reset();

		if (bestRule != null && bestScore > 0.1f) { // if bestScore is too low, the rule is not applied as such a placement would be too random
			stopwatch.Start();
			PartialRedraw(bestRule.Apply(grid, bestX, bestY));
			cellRuleMultipliers[bestRule] /= 2; // discourage the rule from being applied again for a while
			stopwatch.Stop();
			Debug.Log("Redraw took " + stopwatch.ElapsedMilliseconds + "ms");
		} else Debug.Log("No Rule could be applied ("+ (bestRule == null ? "no matching rule" : "rule score too low: " + bestScore) + ").");

		foreach (CellRule rule in CellRule.RULES) cellRuleMultipliers[rule] += 1; // encourage all rules to be applied, effectively pushing them forwards until they are applied and their multiplier is halved again
	}

	private void PartialRedraw(int[] coordsToRedraw) {
		for (int i = 0; i < coordsToRedraw.Length; i += 2) {
			int x = coordsToRedraw[i];
			int y = coordsToRedraw[i + 1];
			for (int pixelY = 0; pixelY < CELL_SIZE; pixelY++) {
				for (int pixelX = 0; pixelX < CELL_SIZE; pixelX++) {
					texture.SetPixel(x * CELL_SIZE + pixelX, y * CELL_SIZE + pixelY, grid[x, y].GetColor(pixelX, pixelY, grid.GetInfo(x, y), CELL_SIZE));
				}
			}
		}
		
		texture.Apply();
	}

	private void Redraw() {
		Color[] colors = new Color[width * CELL_SIZE * height * CELL_SIZE];
		int index = 0;
		for (int y = 0; y < height; y++) {
			for (int pixelY = 0; pixelY < CELL_SIZE; pixelY++) {
				for (int x = 0; x < width; x++) {
					for (int pixelX = 0; pixelX < CELL_SIZE; pixelX++) {
						colors[index++] = grid[x, y].GetColor(pixelX, pixelY, grid.GetInfo(x, y), CELL_SIZE);
					}
				}
			}
		}
		
		texture.SetPixels(colors);
		texture.Apply();
	}
}
}