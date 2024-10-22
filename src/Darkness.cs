using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Darkness : TileMapLayer {
	[Export] public Vector2I CellSize = new(8, 8);
	[Export] public TileMapLayer CollisionMap;
	
	private class TileVisGrid(Vector2I dim, TileMapLayer map) : ICellGrid, IEnumerable<bool> {
		public Vector2I Dim { get; private set; } = dim;
		private readonly BitArray grid = new(dim.X*dim.Y);
		private readonly TileMapLayer map = map;

		public void Clear() {
			grid.SetAll(false);
		}

		public bool IsWall(Vector2I coords) {
			int count = map.GetCellTileData(coords)?.GetCollisionPolygonsCount(0) ?? 0;
			return count > 0;
		}

		public void SetLight(Vector2I coords) {
			grid.Set(coords.X*Dim.Y + coords.Y, true);
		}

		public IEnumerator<bool> GetEnumerator() {
			return (IEnumerator<bool>)grid.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return grid.GetEnumerator();
		}
	}

	private TileVisGrid grid;

	public override void _Ready() {
		Vector2I grid_size = (Vector2I)InfoManager.GetApertureRect().Size / CellSize;
		grid = new TileVisGrid(grid_size, CollisionMap);
	}

	public override void _Process(double delta) {
		Vector2 board_pos = InfoManager.GetPlayerBoardPos();
		Vector2I player_pos = InfoManager.GetPlayerCell(-board_pos, CellSize);
		ShadowCast.ComputeVisibility(grid, player_pos, grid.Dim.X + grid.Dim.Y - 2);

		var columns = grid
			.Chunk(grid.Dim.Y) // group cells into columns
			.Select((bool[] column, int x) =>
				// bundle columns and cells with their coordinates
				(column.Select((bool cell, int y) => (cell, y)), x)
			);
		foreach ((var column, int x) in columns) {
			foreach ((bool cell, int y) in column) {
				
			}
		}
	}
}
