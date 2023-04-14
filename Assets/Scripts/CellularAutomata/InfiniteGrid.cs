using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace CellularAutomata {
public class InfiniteGrid<T> {

	private Dictionary<Tuple<int, int>, T> grid = new Dictionary<Tuple<int, int>, T>();

	public int MinX = int.MinValue;
	public int MaxX = int.MaxValue;
	public int MinY = int.MinValue;
	public int MaxY = int.MaxValue;

	public T this[int x, int y] {
		get {
			return grid.TryGetValue(Tuple.Create(x, y), out T value) ? value : default;
		}
		set {
			if (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY) grid[Tuple.Create(x, y)] = value;
		}
	}
}
}