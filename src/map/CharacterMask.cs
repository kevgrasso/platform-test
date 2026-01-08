using Godot;

[Tool]
public partial class CharacterMask : TileMapLayer {
	[Export] public TileMapLayer _fg_map;

    // potential optimization idea:
    // break every screen into quadrants arranged in an X shape 
    // -- if distance from center X > Y
    // then break the axis of the larger axis distance into thirds and if the player
    // enters the non-center thirds then check for updates on the adjacent screen
    // also maintain a cache

    public override void _Ready() {
		_fg_map.Hide();
    }

    private Color GetReferenceModulate(Vector2I coords) {
		return _fg_map?.GetCellTileData(coords)?.Modulate ?? Colors.White;
	}

    public override bool _UseTileDataRuntimeUpdate(Vector2I coords) {
		Color current_modulate = GetCellTileData(coords).Modulate;
		Color reference_modulate = GetReferenceModulate(coords);

		return current_modulate != reference_modulate;
	}

	public override void _TileDataRuntimeUpdate(Vector2I coords, TileData tile_data) {
		tile_data.Modulate = GetReferenceModulate(coords);
	}
}
