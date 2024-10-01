using Godot;

public partial class Player : CharacterBody2D {
	private Godot.LimboHsm _hsm;
	private RichTextLabel _debug;
	
	
	public override void _Ready() {
		_debug = GetNode<RichTextLabel>("%DebugText");

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

	public float GetInputDirection() {
		return Mathf.Sign(
			Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")
		);
	}

	public bool SetAndMove(Vector2 velocity) {
		Velocity = velocity;
		return MoveAndSlide();
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
