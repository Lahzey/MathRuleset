using System;

namespace CellularAutomata {
public abstract class CellRule {

	public static readonly CellRule[] RULES = new CellRule[] {
		new SimpleCellRule(Cell.LUMBERJACK, 2, 2, Cell.TREE, 9, 9),
		new SimpleCellRule(Cell.STONE_MINE, 2, 2, Cell.STONE, 3, 3),
		new SimpleCellRule(Cell.GOLD_MINE, 2, 2, Cell.GOLD, 3, 3),
		new SimpleCellRule(Cell.FARM, 3, 3, Cell.GRASS, 3, 3),
		new SimpleCellRule(Cell.FISHING_HUT, 1, 1, Cell.WATER, 5, 5),
		new SimpleCellRule(Cell.WALL, 1, 1, Cell.EDGE, 1, 1) { CanBeInRangeOfItsOwnType = true },
		new WatchTowerRule(),
		new HouseRule(),
		new SimpleCellRule(Cell.BARACKS, 3, 3, Cell.HOUSE, 5, 5)
	};

	public readonly Cell cell;
	public readonly int xExtent;
	public readonly int yExtent;
	
	protected bool CanBeInRangeOfItsOwnType = false;

	public CellRule(Cell cell, int xExtent, int yExtent) {
		this.cell = cell;
		this.xExtent = xExtent;
		this.yExtent = yExtent;
	}
	
	public abstract float Evaluate(InfiniteGrid<Cell> grid, int x, int y);
	
	protected float GetResourceScore(InfiniteGrid<Cell> grid, int cellX, int cellY, int xRange, int yRange, Cell resourceCell) {
		float score = 0;
		float possibleScore = 0;
		for (int y = cellY - yRange; y < cellY + yExtent + yRange; y++) {
			int yDist = y > cellY ? y - (cellY + yExtent - 1) : cellY - y;
			for (int x = cellX - xRange; x < cellX + xExtent + xRange; x++) {
				// for coordinates withing the building, check if place can be built upon
				if (y >= cellY && y < cellY + yExtent && x >= cellX && x < cellX + xExtent) {
					if (grid[x, y].IsBuildable()) continue;
					else return 0; // can abort early, will not be able to build here
				}
				
				// for coordinates outside the building, count up resource score
				int xDist = x > cellX ? x - (cellX + xExtent - 1) : cellX - x;
				float scoreToAdd = 1 / SqrtCache.INSTANCE[x*x + y*y]; // score depends on proximity to cell
				possibleScore += scoreToAdd;
				if (grid[x, y] == resourceCell) score += scoreToAdd;
				else if (!CanBeInRangeOfItsOwnType && grid[x, y] == cell) return 0;
			}
		}
		score /= possibleScore; // normalize score
		return score;
	}

	public void Apply(InfiniteGrid<Cell> grid, int cellX, int cellY) {
		for (int x = cellX; x < cellX + xExtent; x++) {
			for (int y = cellY; y < cellY + yExtent; y++) {
				grid[x, y] = cell;
			}
		}
	}
}

// any rule that wants to have a certain resource in a certain range
public class SimpleCellRule : CellRule {
	private readonly Cell resourceCell;
	private readonly int xRange;
	private readonly int yRange;

	public SimpleCellRule(Cell cell, int xExtent, int yExtent, Cell resourceCell, int xRange, int yRange) : base(cell, xExtent, yExtent) {
		this.resourceCell = resourceCell;
		this.xRange = xRange;
		this.yRange = yRange;
	}

	public override float Evaluate(InfiniteGrid<Cell> grid, int x, int y) {
		return GetResourceScore(grid, x, y, xRange, yRange, resourceCell);
	}
}

// like a SimpleCellRule for being close to walls, but cannot be next to the edge (so it doesnt block wall spots)
public class WatchTowerRule : SimpleCellRule {
	public WatchTowerRule() : base(Cell.WATCH_TOWER, 2, 2, Cell.WALL, 4, 4) { }

	public override float Evaluate(InfiniteGrid<Cell> grid, int x, int y) {
		return GetResourceScore(grid, x, y, 1, 1, Cell.EDGE) > 0 ? 0 : base.Evaluate(grid, x, y); // return 0 if the cell touching the edge
	}
}

// like a SimpleCellRule for being next to other houses, but 50% of the score is determined by its proximity to the center of the map
public class HouseRule : SimpleCellRule {
	public HouseRule() : base(Cell.HOUSE, 2, 2, Cell.HOUSE, 1, 1) {
		CanBeInRangeOfItsOwnType = true; // technically not necessary due to implementation details, but makes it more clear
	}

	public override float Evaluate(InfiniteGrid<Cell> grid, int x, int y) {
		// technically there are some -1 or +1 errors here, but they are negligible
		float centerDistScore = Math.Min(Math.Min(x - grid.MinX, grid.MaxX - x) / (grid.MaxX - grid.MinX) / 2, Math.Min(y - grid.MinY, grid.MaxY - y) / (grid.MaxY - grid.MinY) / 2);
		float resourceScore = base.Evaluate(grid, x, y);
		return (resourceScore + centerDistScore) / 2;
	}

	private static int Min(params int[] a) {
		if (a.Length == 0) throw new ArgumentException("Cannot get the min of 0 values.");
		int min = a[0];
		for (int i = 1; i < a.Length; i++) {
			if (a[i] < min) min = a[i];
		}

		return min;
	}
}
}