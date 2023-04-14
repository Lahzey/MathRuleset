using System;
using UnityEngine;

namespace CellularAutomata {
public enum Cell {
	EDGE = 0,
	
	// natural types
	GRASS,
	TREE,
	WATER,
	STONE,
	GOLD,
	
	// building types
	LUMBERJACK,
	STONE_MINE,
	GOLD_MINE,
	FARM,
	FISHING_HUT,
	WALL,
	WATCH_TOWER,
	HOUSE,
	BARACKS
}

static class CellExtensions {
	
	public static bool IsBuildable(this Cell cell) {
		return cell is Cell.GRASS or Cell.WATER;
	}
	public static float GetNoiseThreshold(this Cell cell) {
		switch (cell) {
			case Cell.TREE: return 0.4f;
			case Cell.WATER: return 0.2f;
			case Cell.STONE: return 0.05f;
			case Cell.GOLD: return 0.02f;
			default: return 0;
		}
	}
	public static float GetNoiseScale(this Cell cell) {
		switch (cell) {
			case Cell.TREE: return 0.05f;
			case Cell.WATER: return 0.01f;
			case Cell.STONE: return 0.05f;
			case Cell.GOLD: return 0.1f;
			default: return 0;
		}
	}

	public static Color GetColor(this Cell cell) {
		switch (cell) {
			case Cell.EDGE:
				return Color.black;
			case Cell.GRASS:
				return new Color(0.79f, 1f, 0.24f);
			case Cell.TREE:
				return new Color(0f, 0.39f, 0.11f);
			case Cell.WATER:
				return new Color(0.14f, 0.11f, 0.77f);
			case Cell.STONE:
				return new Color(0.62f, 0.62f, 0.62f);
			case Cell.GOLD:
				return new Color(0.87f, 0.84f, 0.19f);
			case Cell.LUMBERJACK:
				return new Color(1, 0, 0);
			case Cell.STONE_MINE:
				return new Color(1, 0, 0);
			case Cell.GOLD_MINE:
				return new Color(1, 0, 0);
			case Cell.FARM:
				return new Color(1, 0, 0);
			case Cell.FISHING_HUT:
				return new Color(1, 0, 0);
			case Cell.WALL:
				return new Color(1, 0, 0);
			case Cell.WATCH_TOWER:
				return new Color(1, 0, 0);
			case Cell.HOUSE:
				return new Color(1, 0, 0);
			case Cell.BARACKS:
				return new Color(1, 0, 0);
			default:
				throw new ArgumentOutOfRangeException(nameof(cell), cell, null);
		}
	}
}
}