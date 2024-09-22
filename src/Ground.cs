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
	[Export] public float Gravity = 300.0f;
	[Export] public float CoyoteTime = 0.1f;
	
	[Export] private CharacterBody2D _body;
	[Export] private Timer _coyote;
	[Export] private CollisionShape2D _feet;
	
	private bool _is_landing_stop = false;

	private static float GetInputDirection() {
		return Mathf.Sign(
			Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")
		);
	}
	
	// Called when the node enters the scene tree for the first time.
	// public override void _Setup() {
	// 	_body = (CharacterBody2D)Blackboard.GetVar("PlayerBody");
	// }
	
	public override void _Enter() {
		GD.Print("feet enabled");
		_feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		_coyote.Start(CoyoteTime);
		
		// dampen landing velocity if held direction isn't forward
		float direction = GetInputDirection();
		if (Mathf.Sign(_body.Velocity.X) != direction || direction == 0) {
			GD.Print("Landing Stop");
			_is_landing_stop = true;
		} else {
			_is_landing_stop = false;
		}
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
		bool is_turning = Mathf.Sign(_body.Velocity.X) != direction;
		
		if (_body.IsOnFloor()) {
			// indicate player can jump by readying coyote timer
			_coyote.Start(CoyoteTime);
		} 
		
		// if player can jump and asked for a jump within the allotted time,
		// start jumping!
		if (_coyote.IsStopped()) { 
			// coyote timer ran out--start falling
			Dispatch("airborne");
		} else if (Input.IsActionJustPressed("jump")) {
			GD.Print($"jump start. coyote jump: {!_body.IsOnFloor()}");
			_coyote.Stop();
			Dispatch("jumping");
		}

		// apply gravity
		frame_vel.Y -= _body.UpDirection.Y * Gravity * deltaf;

		// handle the movement/deceleration
		if (direction != 0) {
			frame_vel.X += direction * GroundAccel;
			if (Mathf.Abs(frame_vel.X) > MaxSpeed) {
				frame_vel.X = direction * MaxSpeed;
			} else if (is_turning) {
				frame_vel.X *= Mathf.Pow(TurnSkidFactor, (float)delta);
			} else {
				// must be holding forward so cancel landing stop
				if (_is_landing_stop) {
					GD.Print("landing stop cancelled 1");
				}
				_is_landing_stop = false;
			}
		} else {
			float deaccel = Mathf.NaN;
			if (!_is_landing_stop) {
				deaccel = GroundDeaccel;
			} else {
				deaccel = LandingDeaccel;
			}
			frame_vel.X = Mathf.MoveToward(_body.Velocity.X, 0, deaccel);
			if (frame_vel.X == 0) {
				if (_is_landing_stop) {
					GD.Print("landing stop cancelled 2");
				}
				_is_landing_stop = false;
			}
		}

		// move.
		_body.Velocity = frame_vel;
		_body.MoveAndSlide();
	}
}
