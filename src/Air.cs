using Godot;
using System;
using System.Reflection;

public partial class Air : LimboState {
	// exposed godot inspector parameter "constants"
	[Export] public float StandardAccel = 2.65f;
	[Export] public float TurningAccel = 0.5f;
	[Export] public float DirThreshold = 49.893f;
	[Export] public float GravityThreshold = 30.0f;
	[Export] public float MaxSpeed = 130.0f;
	[Export] public float BaseJumpVelocity = 140.0f;
	[Export] public float SpeedJumpVelBonus = 0.15f;
	[Export] public float NormalGravity = 705.0f;
	[Export] public float JumpFloatGravity = 200.0f;
	[Export] public float FallFloatGravity = 235.0f; 
	[Export] public float JumpCancelFactor = 0.75f;
	[Export] public float JumpBufferTime = 0.09f;
	
	// godot nodes
	[Export] private Player _body;
	[Export] private Timer _buffer;

	// jump modulation vars
	private float _jump_direction = 0.0f;
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
			_jump_direction = 0.0f; //reset
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

	private float GetAccel() {
		// if moving the opposite direction to the start of the jump and aren't falling too fast
		if ((_body.Velocity.X * _jump_direction) < 0 && _body.Velocity.Y < GravityThreshold) {
			// backwards jump case--nerf acceleration
			return TurningAccel;
		} else {
			// standard jump case
			return StandardAccel;
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
		float direction = _body.GetInputDirection();
		if (_jump_direction == 0 && Mathf.Abs(_body.Velocity.X) > DirThreshold) {
			_jump_direction = Mathf.Sign(_body.Velocity.X);
			GD.Print($"x vel: {_body.Velocity.X}; jump dir: {_jump_direction}; {_body.Velocity.X * _jump_direction}");
		}

		float oriented_max_speed = direction * MaxSpeed;
		frame_vel.X = Mathf.MoveToward(_body.Velocity.X, oriented_max_speed, GetAccel());
		
		_body.SetAndMove(frame_vel);
	}
}
