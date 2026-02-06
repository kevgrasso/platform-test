using Godot;

[GlobalClass] public partial class InfoManager : Node {
	static private PlayerBody _playerBody;
	static private ReferenceRect _aperture;

	static public void RegisterPlayerBody(PlayerBody body) {
		if (_playerBody == null)
		{
			_playerBody = body;
		}
		else
		{
			throw new System.Exception("player is already registered!");
		}
	}

	static public void RegisterAperture(ReferenceRect aperture) {
		if (_aperture == null)
		{
			_aperture = aperture;
		}
		else
		{
			throw new System.Exception("aperture is already registered!");
		}
	}

	static public Vector2I GetPlayerCell(Vector2 grid_offset, Vector2 cell_size) {
		return (Vector2I)((grid_offset+_playerBody.GlobalPosition)/cell_size).Floor();
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
