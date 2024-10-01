using Godot;

public partial class MainCamera : Camera2D {
	private CockpitHUD _hud;

	public override async void _Ready() {
		_hud = GetNode<CockpitHUD>("/root/Game/GUILayer/CockpitHUD");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void PickActiveScreen(Vector2 pos) {
		Vector2 aperature_size = _hud.GetAperatureSize();
		Position = (pos/aperature_size).Floor() * aperature_size; 
		
		GD.Print(pos/aperature_size);
	}
}
