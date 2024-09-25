using Godot;
using System;
using System.Reflection;

public partial class Air : LimboState {
	// exposed godot inspector parameter "constants"
	[Export] public float Accel = 2.65f;
	[Export] public float MaxSpeed = 130.0f;
	[Export] public float BaseJumpVelocity = 140.0f;
	[Export] public float SpeedJumpVelBonus = 0.15f;
	[Export] public float NormalGravity = 705.0f;
	[Export] public float JumpFloatGravity = 200.0f;
	[Export] public float FallFloatGravity = 235.0f; 
	[Export] public float JumpCancelFactor = 0.75f;
	[Export] public float JumpBufferTime = 0.1f;
	
	// godot nodes
	[Export] private Player _body;
	[Export] private Timer _buffer;

	// jump modulation vars
	private bool _is_floating_jump = false;

	public bool OnJumped() {
			GD.Print($"OnJumped event");
			_is_floating_jump = true;
			return false;
	}

	// Called when the fsm is being initialized
	public override void _Setup() {
		AddEventHandler("buffered jump", Callable.From(OnJumped));
		AddEventHandler("grounded", Callable.From(() => _is_floating_jump = false));
	}

	public override void _Enter() {
		if (_is_floating_jump) {
			// jump setup
			float up = _body.UpDirection.Y;
			float jump_velocity = up * BaseJumpVelocity;
			jump_velocity += up * Mathf.Abs(_body.Velocity.X) * SpeedJumpVelBonus;
			_body.Velocity = new Vector2(_body.Velocity.X, jump_velocity);
			GD.Print($"jump body vel: {_body.Velocity.Y}");
		} else {
			_buffer.Stop();
			// no jump
			GD.Print($"no jump body vel: {_body.Velocity.Y}");
		}
	}

	private float GetGravity() {
		if (_is_floating_jump) {
			if (_body.Velocity.Y < 0) {
				return JumpFloatGravity;
			} else {
				return FallFloatGravity;
			} 
		} else {
			// jump input released
			return NormalGravity;
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Update(double delta) {
		float deltaf = (float)delta;
		
		Vector2 frame_vel = _body.Velocity;
		if (Input.IsActionJustPressed("jump")) {
			// mark that player asked for a jump
			_buffer.Start(JumpBufferTime); 
		} else if (Input.IsActionJustReleased("jump")) {
			if (_is_floating_jump && _body.Velocity.Y < 0) {
				// truncate jump
				frame_vel.Y *= JumpCancelFactor;
			}
			
			// cancel jump actions
			_is_floating_jump = false; 
			_buffer.Stop();
		}
		
		//check if a state transition is necessary
		if (_body.IsOnFloor() && _body.Velocity.Y >= 0) {
			if (!_buffer.IsStopped()) {
				Dispatch("buffered jump");
			} else {
				Dispatch("grounded");
			}
		}
		
		// apply gravity
		if (_body.IsOnCeiling() && _body.Velocity.Y < 0) {
			frame_vel = frame_vel.Reflect(_body.UpDirection.Orthogonal());
		} else {
			frame_vel.Y -= _body.UpDirection.Y * GetGravity() * deltaf;
		}

		// calculate the movement
		float oriented_max_speed = _body.GetInputDirection() * MaxSpeed;
		frame_vel.X = Mathf.MoveToward(_body.Velocity.X, oriented_max_speed, Accel);
		
		_body.SetAndMove(frame_vel);
	}
}
