using Godot;

public partial class Player : CharacterBody2D {
	private LimboHsm _hsm;
	private MainCamera _camera;
	private RichTextLabel _debug;
	
	
	public override void _Ready() {
		_camera = GetNode<MainCamera>("/root/Game/MainCamera");
		_debug = GetNode<RichTextLabel>("%DebugText");
		InfoManager.RegisterPlayer(this);

		// aquire necessary limboai nodes
		_hsm = GetNode<LimboHsm>("Assets/PlatformMachine");
		LimboState air = GetNode<LimboState>("Assets/PlatformMachine/Air");
		LimboState ground = GetNode<LimboState>("Assets/PlatformMachine/Ground");

		// setup limboai states
		_hsm.AddTransition(air, ground, "grounded");
		_hsm.AddTransition(ground, air, "airborne");
		_hsm.AddTransition(air, air, "buffered jump");
		_hsm.InitialState = air;

		// start limboai
		_hsm.Initialize(this);
		_hsm.SetActive(true);
	}

	public bool SetAndMove(Vector2 velocity) {
		// if wall normal  and velocity x axis are opposing directions, cancel x axis movement
		if (IsOnWall() && Mathf.Sign(GetWallNormal().X * velocity.X) == -1.0f) {
			velocity.X = 0;
		}

		Velocity = velocity;
		bool result = MoveAndSlide();
		_camera.UpdateActiveBoard(GlobalPosition);
		return result;
	}
	
	public override void _PhysicsProcess(double delta) {
		_hsm.Update(delta);

		// _debug.Text = 
		// 	$"pos: {Position.Round()}\tlast motion: {GetLastMotion().Round()}\n" + 
		// 	$"cur vel: {GetRealVelocity().Round()}\tprev vel: {GetPositionDelta().Round()}\n" + 
		// 	$"collision count; {GetSlideCollisionCount()}\n" +
		// 	$"ceil: {IsOnCeiling()}\tfloor: {IsOnFloor()}\twall: {IsOnWall()}";
	}
}
