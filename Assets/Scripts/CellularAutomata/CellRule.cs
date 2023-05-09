using System;
using UnityEngine;

namespace CellularAutomata {
public abstract class CellRule {

	public static readonly CellRule[] RULES = new CellRule[] {
		new SimpleCellRule(Cell.LUMBERJACK, 2, 2, Cell.TREE, 9, 9),
		new SimpleCellRule(Cell.STONE_MINE, 2, 2, Cell.STONE, 3, 3),
		new SimpleCellRule(Cell.GOLD_MINE, 2, 2, Cell.GOLD, 3, 3),
		new SimpleCellRule(Cell.FARM, 3, 3, Cell.GRASS, 7, 7),
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
	
	public abstract float Evaluate(CellGrid grid, int x, int y);
	
	protected float GetResourceScore(CellGrid grid, int cellX, int cellY, int xRange, int yRange, Cell resourceCell) {
		float score = 0;
		float maxScore = 0;
		Cell cellToCheck; // moving this outside the loop actually improves performance considerably
		for (int y = cellY - yRange; y < cellY + yExtent + yRange; y++) {
			for (int x = cellX - xRange; x < cellX + xExtent + xRange; x++) {
				cellToCheck = grid[x, y];
				// for coordinates withing the building, check if place can be built upon
				if (y >= cellY && y < cellY + yExtent && x >= cellX && x < cellX + xExtent) {
					if (cellToCheck.IsBuildable()) continue;
					else return -1; // can abort early, will not be able to build here
				}
				
				// for coordinates outside the building, count up resource score
				int distanceX = Math.Abs(x - cellX);
				int distanceY = Math.Abs(y - cellY);
				int distance = distanceX > distanceY ? distanceX : distanceY;
				float scoreToAdd = 1f / distance; // score depends on proximity to cell
				maxScore += scoreToAdd;
				if (cellToCheck == resourceCell) score += scoreToAdd;
				else if (!CanBeInRangeOfItsOwnType && cellToCheck == cell) return 0;
			}
		}
		score /= maxScore; // normalize score
		return score;
	}

	public int[] Apply(CellGrid grid, int cellX, int cellY) {
		int[] changedCells = new int[xExtent * yExtent * 2];
		int i = 0;
		for (int x = cellX; x < cellX + xExtent; x++) {
			for (int y = cellY; y < cellY + yExtent; y++) {
				grid[x, y] = cell;
				grid.SetInfo(x, y, (x-cellX) + (y-cellY) * xExtent);
				changedCells[i++] = x;
				changedCells[i++] = y;
			}
		}
		return changedCells;
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

	public override float Evaluate(CellGrid grid, int x, int y) {
		return GetResourceScore(grid, x, y, xRange, yRange, resourceCell);
	}
}

// like a SimpleCellRule for being close to walls, but cannot be next to the edge (so it doesnt block wall spots)
public class WatchTowerRule : SimpleCellRule {
	public WatchTowerRule() : base(Cell.WATCH_TOWER, 2, 2, Cell.WALL, 4, 4) { }

	public override float Evaluate(CellGrid grid, int x, int y) {
		return GetResourceScore(grid, x, y, 1, 1, Cell.EDGE) > 0 ? 0 : base.Evaluate(grid, x, y); // return 0 if the cell touching the edge
	}
}

// like a SimpleCellRule for being next to other houses, but 50% of the score is determined by its proximity to the center of the map
public class HouseRule : SimpleCellRule {
	public HouseRule() : base(Cell.HOUSE, 2, 2, Cell.HOUSE, 1, 1) {
		CanBeInRangeOfItsOwnType = true; // technically not necessary due to implementation details, but makes it more clear
	}

	public override float Evaluate(CellGrid grid, int x, int y) {
		float resourceScore = base.Evaluate(grid, x, y);
		if (resourceScore < 0) return 0; // abort early if the cell is not buildable
		
		// technically there might be some -1 or +1 errors here, but they would be negligible
		float centerDistScore = Math.Min(Math.Min(x, grid.Width - x) / (float) (grid.Width / 2), Math.Min(y, grid.Height - y) / (float) (grid.Height / 2));
		return (resourceScore + centerDistScore) / 2;
	}
}
}