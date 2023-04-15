using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace CellularAutomata {
public class CellGrid {

	private Cell[,] cells = new Cell[0,0];
	private int[,] info = new int[0,0];

	// storing the speed up parameter check in indexer
	public int Width { get; private set; }
	public int Height { get; private set; }

	public void Resize(int width, int height) {
		Width = width;
		Height = height;
		cells = new Cell[width, height];
		info = new int[width, height];
	}

	public Cell this[int x, int y] {
		get => x >= 0 && x < Width && y >= 0 && y < Height ? cells[x, y] : default;
		set {
			if (x >= 0 && x < Width && y >= 0 && y < Height) cells[x, y] = value;
		}
	}
	
	public int GetInfo(int x, int y) {
		return x >= 0 && x < Width && y >= 0 && y < Height ? info[x, y] : default;
	}
	
	public void SetInfo(int x, int y, int info) {
		if (x >= 0 && x < Width && y >= 0 && y < Height) this.info[x, y] = info;
	}
}
}