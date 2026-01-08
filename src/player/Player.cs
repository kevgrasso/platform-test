using Godot;
using GodotStateCharts;

public partial class Player : CharacterBody2D
{
	public StateChart chart;

	public override void _Ready()
	{
		chart = StateChart.Of(GetNode("StateChart"));
		InfoManager.RegisterPlayer(this);
	}

	//unhandled input
	public void OnRootLateInput(InputEvent input)
	{
		if (input.IsAction("jump"))
		{
			if (input.IsReleased())
			{
				chart.SendEvent("JumpCancel");
			}
			else if (input.IsPressed())
			{
				chart.SendEvent("Jump");
			} 
		}
	}

	public void OnGroundedEnter()
	{
		GD.Print("feet enabled");
		_feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		
		// dampen landing velocity if held direction isn't forward
		_is_landing_stop = true;
	}

	public void OnGroundedExit()
	{
		GD.Print("feet disabled");
		_feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	public void OnGroundedStableTick(double delta)
	{
		float direction = InfoManager.GetInputDirection();

		// handle the movement/deceleration
		float oriented_max_speed = direction * MaxSpeed;
		float forwardsness = Mathf.Sign(Velocity.X * direction); 
		x_vel = Mathf.MoveToward(Velocity.X, oriented_max_speed, GetAccel(direction));
		if (forwardsness < 0) {
			// turning case--apply skidding
			x_vel *= Mathf.Pow(TurnSkidFactor, (float)delta);
		} else if (forwardsness > 0 || x_vel == 0) {
			// if stationary or holding forwards cancel stop
			_is_landing_stop = false;
		}
	}

	public void OnGroundedCoyoteTick(double delta)
	{
		
	}
	private float CalcAirbourneGravity(float y_vel, float delta, float gravity)
	{
		return y_vel - (UpDirection.Y * gravity * delta);
	}

	private float JumpRiseMovement(float y_vel, float delta, float gravity)
	{
		// apply gravity
		if (IsOnCeiling()) {
			y_vel = -y_vel;
		} else {
			y_vel = CalcAirbourneGravity(y_vel, delta, gravity);
		}

		return y_vel;
	}

	private Vector2 frame_vel = new(float.NaN, float.NaN);

	private bool? BuildAndTryMove(Vector2.Axis axis, float length)
	{
		frame_vel[(int)axis] = length;

		if (frame_vel.IsFinite())
		{
			// if wall normal  and velocity x axis are opposing directions, cancel x axis movement
			if (IsOnWall() && Mathf.Sign(GetWallNormal().X * frame_vel.X) == -1.0f) {
				frame_vel.X = 0;
			}

			Velocity = frame_vel;
			bool result = MoveAndSlide();
			_camera.UpdateActiveBoard(GlobalPosition);

			frame_vel = new Vector2(float.NaN, float.NaN);
			return result;
		}
		else
		{
			return null;
		}
	}

	public void OnJumpRiseEnter(double delta)
	{
		// jump setup
		float up = UpDirection.Y;
		float jump_velocity = up * BaseJumpVelocity;
		jump_velocity += up * Mathf.Abs(Velocity.X) * SpeedJumpVelBonus;
		Velocity = new Vector2(Velocity.X, jump_velocity);
		_jump_direction = 0.0f; //reset
		GD.Print($"jump body vel: {Velocity.Y}");
	}

	public void OnJumpRiseTick(double delta)
	{
		BuildAndTryMove(Vector2.Axis.Y, JumpRiseMovement(y_vel, delta, gravity));
	}

	public void OnJumpBrakeTick(double delta)
	{
		BuildAndTryMove(Vector2.Axis.Y, JumpRiseMovement(y_vel, delta, gravity));
	}

	private void LandingCheck()
	{
		if (IsOnFloor()) {
			chart.SendEvent("Landing");
		}
	}

	public void OnSlowFallTick(double delta)
	{
		LandingCheck();
		BuildAndTryMove(Vector2.Axis.Y, JumpRiseMovement(y_vel, delta, gravity));
	}

	public void OnQuickFallTick(double delta)
	{
		LandingCheck();
		BuildAndTryMove(Vector2.Axis.Y, JumpRiseMovement(y_vel, delta, gravity));
	}

	public void AirMovement(double delta, float gravity)
	{
		float deltaf = (float)delta;
		

		// calculate the movement
		float direction = InfoManager.GetInputDirection();
		if (_jump_direction == 0 && Mathf.Abs(Velocity.X) > DirThreshold) {
			_jump_direction = Mathf.Sign(Velocity.X);
			GD.Print($"x vel: {Velocity.X}; jump dir: {_jump_direction}; {Velocity.X * _jump_direction}");
		}

		float oriented_max_speed = direction * MaxSpeed;
		x_vel = Mathf.MoveToward(Velocity.X, oriented_max_speed, GetAccel());
	}
}
