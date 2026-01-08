using Godot;

public partial class MapManager : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("map setup in");
		MainCamera camera = GetNode<MainCamera>("%MainCamera");
		RichTextLabel debug = GetNode<RichTextLabel>("%DebugText");
		GetNode<Player>("Player").Setup(camera, debug);
		GD.Print("map setup out");
	}

}
