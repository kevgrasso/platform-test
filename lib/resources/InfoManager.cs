using Godot;

[GlobalClass] public partial class InfoManager : Node {
	static private Player _player;
	static private ReferenceRect _aperture;

	static public void RegisterPlayer(Player player) {
		_player = player;
	}

	static public void RegisterAperture(ReferenceRect aperture) {
		_aperture = aperture;
	}

	static public Vector2 GetPlayerPos() {
		return _player.GlobalPosition;
	}

	static public Vector2I GetPlayerCell(Vector2 grid_offset, Vector2 cell_size) {
		return (Vector2I)((grid_offset+_player.GlobalPosition)/cell_size).Floor();
	}

	static public Vector2I GetPlayerBoard() {
		return GetPlayerCell(Vector2.Zero, _aperture.Size);
	}

	static public Vector2 GetPlayerBoardPos() {
		return GetPlayerBoard() * _aperture.Size;
	}

	static public Rect2 GetApertureRect() {
		return new Rect2(_aperture.GlobalPosition, _aperture.Size);
	}

	static public float GetInputDirection() {
		return Mathf.Sign(
			Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")
		);
	}
}
