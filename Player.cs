using Godot;
using System;
using System.Reflection;

public partial class Player : CharacterBody2D {
	private Godot.LimboHsm _hsm;
	private RichTextLabel _debug;
	private bool _is_floating_jump = false;
	private bool _is_landing_stop = false;
	private bool _is_turning_jump = false;
	// private float _p_speed = 0; //p-speed is cut
	
	//FIXME:
	// head can get stuck in ceiling by jumping into a corner
	// gets caught running off ledges sometimes -- it's in narrow gaps!
	// find way to remove extra air frame delay before disabling feet
	//TODO:
	// implement focus?
	// ladders
	// max air turn speed
	
	public override void _Ready() {
		_debug = GetNode<RichTextLabel>("%DebugText");

		// aquire necessary limboai nodes
		_hsm = GetNode<Godot.LimboHsm>("Assets/PlatformMachine");
		Godot.LimboState air = GetNode<Godot.LimboState>("Assets/PlatformMachine/Air");
		Godot.LimboState ground = GetNode<Godot.LimboState>("Assets/PlatformMachine/Ground");

		// setup
		_hsm.AddTransition(air, ground, "grounded");
		_hsm.AddTransition(ground, air, "airborne");
		_hsm.AddTransition(air, air, "buffered jump");
		_hsm.InitialState = air;

		_hsm.Initialize(this);
		_hsm.SetActive(true);
	}
	
	public override void _PhysicsProcess(double delta) {
		_hsm.Update(delta);

		_debug.Text = 
			$"pos: {Position}\tlast motion: {GetLastMotion()}\n" + 
			$"prev vel: {GetPositionDelta()}\tcur vel: {GetRealVelocity()}\n" + 
			$"collision count; {GetSlideCollisionCount()}\n" +
			$"ceil: {IsOnCeiling()}\tfloor: {IsOnFloor()}\twall: {IsOnWall()}";


		//for reference:
		/*
		float deltaf = (float)delta;
		
		if (Input.IsActionJustPressed("jump")) {
			_buffer.Start(JumpBufferTime); // mark that player asked for a jump
		} else if (!Input.IsActionPressed("jump")) {
			_is_floating_jump = false;
			_buffer.Stop();
		}
		
		// init movement vars
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Vector2 frame_vel = Velocity;
		bool is_turning = Mathf.Sign(Velocity.X) != direction.X;
		
		if (IsOnFloor()) {
			// indicate player can jump by readying coyote timer
			_coyote.Start(CoyoteTime);
			if (_feet.Disabled) {
				GD.Print("feet enabled");
				_feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
				//reduce velocity on landing if held direction isn't forward
				if (is_turning || direction.X == 0) {
					GD.Print("Landing Stop");
					_is_landing_stop = true;
				}
			}
		} else {
			if (!_feet.Disabled) {
				// TODO: find way to remove extra frame delay before disabling
				GD.Print("feet disabled");
				_feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
				_is_landing_stop = false;
			}
		}
		
		// if player can jump and asked for a jump within the allotted time,
		// start jumping!
		if (!_buffer.IsStopped() && !_coyote.IsStopped()) {
			GD.Print("jump start. coyote jump: " + !IsOnFloor());
			_coyote.Stop();
			_buffer.Stop();
			frame_vel.Y = UpDirection.Y * BaseJumpVelocity;
			frame_vel.Y += UpDirection.Y * Mathf.Abs(Velocity.X) * SpeedJumpVelBonus;
			_is_floating_jump = true;
		}
		
		// apply gravity
		if (IsOnCeiling()) {
			frame_vel.Y = -Velocity.Y;
		} else if (_is_floating_jump) {
			frame_vel.Y += -UpDirection.Y * FloatGravity * deltaf;
		} else {
			// TODO: likely will have to adjust this condition for edge cases later
			if (frame_vel.Y < 0) { 
				frame_vel.Y *= Mathf.Pow(JumpCancelFactor, deltaf);
			}
			frame_vel.Y += -UpDirection.Y * NormalGravity * deltaf;
		}

		// handle the movement/deceleration
		if (direction.X != 0) {
			// NaN means floats fail loudly if not assigned
			float accel = Mathf.NaN; 
			if (IsOnFloor()) {
				accel = GroundAccel;
			} else {
				accel = AirAccel;
			}
			
			frame_vel.X += direction.X * accel;
			if (Mathf.Abs(frame_vel.X) > MaxSpeed) {
				frame_vel.X = direction.X * MaxSpeed;
			} else if (IsOnFloor() && is_turning) {
				frame_vel.X *= Mathf.Pow(TurnSkidFactor, (float)delta);
			} else if (!is_turning){
				// must be holding forward so cancel landing stop
				if (_is_landing_stop) {
					GD.Print("landing stop cancelled 1");
				}
				_is_landing_stop = false;
			}
		} else if (IsOnFloor()) {
			float deaccel = Mathf.NaN;
			if (!_is_landing_stop) {
				deaccel = GroundDeaccel;
			} else {
				deaccel = LandingDeaccel;
			}
			frame_vel.X = Mathf.MoveToward(Velocity.X, 0, deaccel);
			if (frame_vel.X == 0) {
				if (_is_landing_stop) {
					GD.Print("landing stop cancelled 2");
				}
				_is_landing_stop = false;
			}
		}
		
		// debug
		//if (!coyote.IsStopped() || IsOnFloor()) {
			//GD.Print("active: " + !coyote.IsStopped() + "; on ground: " + IsOnFloor());
		//}

		Velocity = frame_vel;
		MoveAndSlide();
		*/
	}
}
