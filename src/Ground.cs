using Godot;
using System;

public partial class Ground : LimboState
{
	// exposed godot inspector parameter "constants"
	[Export] public float GroundAccel = 5.0f;
	[Export] public float MaxSpeed = 130.0f;
	[Export] public float GroundDeaccel = 1.75f;
	[Export] public float LandingDeaccel = 4.35f;
	[Export] public float TurnSkidFactor = 0.8f;
	[Export] public float CoyoteTime = 0.1f;
	[Export] public float CoyoteGravity = 300.0f;
	
	[Export] private CharacterBody2D _body;
	[Export] private Timer _coyote;
	[Export] private CollisionShape2D _feet;
	
	private bool _is_landing_stop = false;

	[Signal] public delegate void JumpedEventHandler();

	private static float GetInputDirection() {
		return Mathf.Sign(
			Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")
		);
	}
	private float GetAccel(float direction) {
		if (direction != 0) {
			// there is active left/right directional input
			return GroundAccel;
		} else if (!_is_landing_stop) {
			// no left/right directional input and isn't landing stop--slow drift to a stop
			return GroundDeaccel;
		} else {
			// no left/right directional input and is landing stop--quick drift to a stop
			return LandingDeaccel;
		}
	}
	
	public override void _Enter() {
		GD.Print("feet enabled");
		_feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		_coyote.Start(CoyoteTime);
		
		// dampen landing velocity if held direction isn't forward
		_is_landing_stop = true;
	}
	public override void _Exit() {
		GD.Print("feet disabled");
		_feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		_coyote.Stop();
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Update(double delta) {
		float deltaf = (float)delta;
		
		// init movement vars
		float direction = GetInputDirection();
		Vector2 frame_vel = _body.Velocity;
		if (_body.IsOnFloor()) {
			// indicate player can jump by readying coyote timer
			_coyote.Start(CoyoteTime);
		} else {
			// in air apply gravity
			frame_vel.Y -= _body.UpDirection.Y * CoyoteGravity * deltaf;
		}
		
		// if player can jump and asked for a jump within the allotted time,
		// start jumping!
		if (_coyote.IsStopped()) { 
			// coyote timer ran out--start falling
			Dispatch("airborne");
		} else if (Input.IsActionJustPressed("jump")) {
			GD.Print($"jump start. coyote jump: {!_body.IsOnFloor()}");
			_coyote.Stop();
			EmitSignal("Jumped");
			Dispatch("airborne");
		}

		// handle the movement/deceleration
		// TODO: refactor
		float max_oriented_speed = direction * MaxSpeed;
		float forwardsness = Mathf.Sign(_body.Velocity.X) * direction; 
		frame_vel.X = Mathf.MoveToward(_body.Velocity.X, max_oriented_speed, GetAccel(direction));
		if (forwardsness < 0) {
			// turning case--apply skidding
			frame_vel.X *= Mathf.Pow(TurnSkidFactor, (float)delta);
		} else if (forwardsness > 0 || frame_vel.X == 0) {
			// if stationary or holding forwards cancel stop
			_is_landing_stop = false;
		}

		// move.
		_body.Velocity = frame_vel;
		_body.MoveAndSlide();
	}
}
