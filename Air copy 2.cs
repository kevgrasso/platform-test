using Godot;
using System;
using System.Reflection;

public partial class AirOld : LimboState {
	[Export] public float Accel = 2.65f;
	[Export] public float MaxSpeed = 130.0f;
	[Export] public float MaxTurnSpeed = 40.0f;
	[Export] public float BaseJumpVelocity = 140.0f;
	[Export] public float SpeedJumpVelBonus = 0.15f;
	[Export] public float NormalGravity = 300.0f;
	[Export] public float FloatGravity = 200.0f;
	[Export] public float JumpCancelFactor = 0.3f;
	[Export] public float JumpBufferTime = 0.1f;
	
	private CharacterBody2D _body;
	private Timer _buffer;
	private CollisionShape2D _feet;
	
	private bool _is_floating_jump = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Setup() {
		_body = GetNode<CharacterBody2D>("%Assets/..");
		_buffer = GetNode<Timer>("%Assets/JumpBuffer");
	}
	
	public override void _Enter() {
		if (Input.IsActionPressed("jump")) {

			float y_vel = _body.UpDirection.Y * BaseJumpVelocity;
			y_vel += _body.UpDirection.Y * Mathf.Abs(_body.Velocity.X) * SpeedJumpVelBonus;
			_body.Velocity = new Vector2(_body.Velocity.X, y_vel);
			GD.Print($"body vel: {_body.Velocity.Y}");
			
			_buffer.Stop();
			_is_floating_jump = true;
		} else {
			_is_floating_jump = false;
		}
	}
	public override void _Exit() {
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Update(double delta) {
		float deltaf = (float)delta;
		
		if (Input.IsActionJustPressed("jump")) {
			// mark that player asked for a jump
			_buffer.Start(JumpBufferTime); 
		} else if (!Input.IsActionPressed("jump")) {
			// cancel jump actions
			_is_floating_jump = false; 
			_buffer.Stop();
		}
		
		// init movement vars
		float direction = Mathf.Sign(
			Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left")
		);
		Vector2 frame_vel = _body.Velocity;
		bool is_turning = Mathf.Sign(_body.Velocity.X) != direction;
		
		if (_body.IsOnFloor() && _body.Velocity.Y == 0) {
			if (!_buffer.IsStopped()) {
				Dispatch("buffered jump");
				GD.Print("buffered");
			} else {
				Dispatch("grounded");
				GD.Print($"deltaf: {deltaf}");
			}
		}
		
		// apply gravity
		if (_body.IsOnCeiling()) {
			frame_vel.Y = -_body.Velocity.Y;
		} else if (_is_floating_jump) {
			frame_vel.Y -= _body.UpDirection.Y * FloatGravity * deltaf;
		} else {
			// if ascending but the player released the jump button at some
			// point, cancel the ascent
			if (frame_vel.Y < 0) { 
				frame_vel.Y *= Mathf.Pow(JumpCancelFactor, deltaf);
			}
			frame_vel.Y -= _body.UpDirection.Y * NormalGravity * deltaf;
		}
		
		// handle the movement
		frame_vel.X += direction * Accel;
		if (Mathf.Abs(frame_vel.X) > MaxSpeed) {
			frame_vel.X = direction * MaxSpeed;
		} 
		
		_body.Velocity = frame_vel;
		_body.MoveAndSlide();
	}
}
