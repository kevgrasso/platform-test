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

		public bool IsOpaque(Vector2I coords) {
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

	private void SetupGrid() {
		Vector2I grid_size = (Vector2I)InfoManager.GetApertureRect().Size / CellSize;
		grid = new TileVisGrid(grid_size, CollisionMap);
	}

	public override void _Process(double delta) {
		if (grid == null) {
			SetupGrid();
		}

		//TODO: make sure this doesn't need to be wrapped in an if statement
		Vector2I player_pos = LocalToMap(ToLocal(InfoManager.GetPlayerPos()));
		int source_id = TileSet.GetSourceId(0);

		ShadowCast.ComputeVisibility(grid, player_pos, grid.Dim.X + grid.Dim.Y - 2);
		//TODO: refactor grid iteration to be simple
		var columns = grid
			.Chunk(grid.Dim.Y) // group cells into columns
			.Select((bool[] column, int x) =>
				// bundle columns and cells with their coordinates
				(column.Select((bool cell, int y) => (cell, y)), x)
			);
		
		TileMapPattern pattern = new();
		pattern.SetSize(grid.Dim);
		foreach ((var column, int x) in columns) {
			foreach ((bool cell, int y) in column) {
				Vector2I coords = new(x, y);

				bool dark_tile = GetCellSourceId(coords) != -1;
				if (!cell && dark_tile) {
					// lighten cell by erasing tile
					pattern.SetCell(coords, -1, new Vector2I(-1, -1), -1);
				} else if (cell && !dark_tile) {
					// darken cell by creating tile
					pattern.SetCell(coords, source_id, new Vector2I(0, 0), 0);
				}
			}
		}
		Vector2I board_pos = player_pos/grid.Dim * grid.Dim;
		SetPattern(board_pos, pattern);
	}
}
