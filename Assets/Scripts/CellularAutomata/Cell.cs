using System;
using System.Collections.Generic;
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

	private static TextureInfo[] cellTextures = new TextureInfo[Enum.GetValues(typeof(Cell)).Length];

	public static bool IsBuildable(this Cell cell) {
		return cell == Cell.GRASS;
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
			case Cell.TREE: return 0.1f;
			case Cell.WATER: return 0.04f;
			case Cell.STONE: return 0.2f;
			case Cell.GOLD: return 0.4f;
			default: return 0;
		}
	}

	public static Color GetColor(this Cell cell, int pixelX, int pixelY, int tileIndex, int cellSize) {
		TextureInfo texture = cell.GetTexture();
		int width = texture.width / cellSize;
		int height = texture.height / cellSize;
		int x = pixelX + (tileIndex % height) * cellSize;
		int y = pixelY + (tileIndex / width) * cellSize;
		return texture.colors[x + y * texture.width];
	}
	
	private static TextureInfo GetTexture(this Cell cell) {
		TextureInfo textureInfo = cellTextures[(int) cell];
		if (textureInfo.colors == null) {
			Texture2D texture = Resources.Load<Texture2D>("Textures/Cells/" + cell.ToString());
			textureInfo.colors = texture.GetPixels();
			textureInfo.width = texture.width;
			textureInfo.height = texture.height;
		}
		return textureInfo;
	}
	
	private struct TextureInfo {
		public Color[] colors;
		public int width;
		public int height;
	}
}
}