using Godot;
using System;
using System.Reflection;

public partial class Air : LimboState {
	// exposed godot inspector parameter "constants"
	[Export] public float InitialAccel = 2f;
	[Export] public float FinalAccel = 2.65f;
	[Export] public float AccelStartupTime = 35.0f;
	[Export] public float MaxSpeed = 130.0f;
	[Export] public float BaseJumpVelocity = 140.0f;
	[Export] public float SpeedJumpVelBonus = 0.15f;
	[Export] public float NormalGravity = 705.0f;
	[Export] public float JumpFloatGravity = 200.0f;
	[Export] public float FallFloatGravity = 235.0f; 
	[Export] public float JumpCancelFactor = 0.75f;
	[Export] public float JumpBufferTime = 0.1f;
	
	// godot nodes
	[Export] private CharacterBody2D _body;
	[Export] private Timer _buffer;

	// jump modulation vars
	private float _initial_jump_velocity = 0.0f;
	private bool _is_floating_jump = false;

	public void OnJumped() {
			_is_floating_jump = true;
	}

	// Called when the fsm is being initialized
	public override void _Enter() {
		if (_is_floating_jump) {
			// jump setup
			float up = _body.UpDirection.Y;
			_initial_jump_velocity = up * BaseJumpVelocity;
			_initial_jump_velocity += up * Mathf.Abs(_body.Velocity.X) * SpeedJumpVelBonus;
			_body.Velocity = new Vector2(_body.Velocity.X, _initial_jump_velocity);
			GD.Print($"jump body vel: {_body.Velocity.Y}");
		} else {
			_buffer.Stop();
			// no jump
			GD.Print($"no jump body vel: {_body.Velocity.Y}");
		}
	}

	public override void _Exit() {
			_is_floating_jump = false;
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

	private float GetHorizAccel() {
		float amount = (_body.Velocity.Y + _initial_jump_velocity) / AccelStartupTime;
		return Mathf.Lerp(InitialAccel, FinalAccel, Mathf.Clamp(amount, 0.0f, 1.0f));
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
				Dispatch("buffered jump", true);
				CallDeferred(MethodName.OnJumped);
				OnJumped();
				GD.Print("buffered");
			} else {
				Dispatch("grounded");
				GD.Print($"deltaf: {deltaf}");
			}
		}
		
		// apply gravity
		if (_body.IsOnCeiling() && _body.Velocity.Y < 0) {
			frame_vel.Y = -_body.Velocity.Y;
		} else {
			frame_vel.Y -= _body.UpDirection.Y * GetGravity() * deltaf;
		}

		// calculate the movement
		float direction = Mathf.Sign(
			Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")
		);
		frame_vel.X += direction * GetHorizAccel();
		if (Mathf.Abs(frame_vel.X) > MaxSpeed) { // speed limit
			frame_vel.X = direction * MaxSpeed;
		} 

		// finally, perform the movement
		_body.Velocity = frame_vel;
		_body.MoveAndSlide();
	}
}
