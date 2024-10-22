using Godot;

public partial class MainCamera : Camera2D {

	public override void _Ready() {
		int min_w = (int)ProjectSettings.GetSetting("display/window/size/viewport_width");
		int min_h = (int)ProjectSettings.GetSetting("display/window/size/viewport_height");
		GetWindow().MinSize = new Vector2I(min_w, min_h);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void UpdateActiveBoard(Vector2 pos) {
		Rect2 aperture = InfoManager.GetApertureRect(); // the "window" of the gui cockpit
		// position camera to correct cell
		Position = InfoManager.GetPlayerBoardPos();
		//adjust camera for os window aspect ration
		Offset = -aperture.Position;
	}
}
