using Godot;

public partial class MainCamera : Camera2D {
	private CockpitHUD _hud;

	public override void _Ready() {
		_hud = GetNode<CockpitHUD>("/root/Game/GUILayer/CockpitHUD");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void PickActiveScreen(Vector2 pos) {
		Rect2 aperture = _hud.GetApertureRect(); // the "window" of the gui cockpit
		// position camera to correct cell
		Position = (pos/aperture.Size).Floor() * aperture.Size;
		//adjust camera for os window aspect ration
		Offset = -aperture.Position;
	}
}
