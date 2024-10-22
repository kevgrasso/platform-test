using Godot;

public partial class ApertureRect : ReferenceRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		InfoManager.RegisterAperture(this);
	}
}
