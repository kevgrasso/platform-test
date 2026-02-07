using Godot;
using GodotStateCharts;

public partial class GroundedBehavior : Node
{
	// exposed godot inspector parameter "constants"
	[Export] public float GroundAccel = 300.0f;
	[Export] public float MaxSpeed = 130.0f; 
	[Export] public float GroundDeaccel = 105.0f;
	[Export] public float LandingDeaccel = 261.0f; 
	[Export] public float TurnSkidFactor = 0.8f;
	[Export] public float CoyoteGravity = 115.0f;

	private PlayerMachineClerk _clerk;
	private StateChart _chart;

	// RESOURCES

	public void Setup(PlayerMachineClerk clerk, StateChart chart)
	{
		GD.Print($"grounded setup in");
		this._clerk = clerk;
		this._chart = chart;
		GD.Print($"grounded setup out");
	}
	
	// UTILITY

	private float GetForwardsness(float direction) => Mathf.Sign(_clerk.GetVelX() * direction);

	private float CalcHorizontalMovement(float delta, float direction, float acceleration)
	{
		// handle the movement/deceleration
		float oriented_max_speed = direction * MaxSpeed;
		return Mathf.MoveToward(_clerk.GetVelX(), oriented_max_speed, acceleration*delta);
	}
	
	private float CalcHorizontalBraking(float delta, float x_vel)
	{
		return x_vel * Mathf.Pow(TurnSkidFactor, (float)delta);
	} 

	// HORIZONTAL

	/*
	public void OnGroundedLandingStopEnter()
	{
		float direction = InfoManager.GetInputDirection();
		if (GetForwardsness(direction) > 0)
		{
			// if holding forwards cancel stop
			chart.SendEvent("Accelerate");
		}
	}

	public void OnGroundedLandingStopTick(double delta)
	{
		float deltaf = (float)delta;

		float direction = InfoManager.GetInputDirection();
		float x_vel = CalcHorizontalMovement(deltaf, direction, LandingDeaccel);
		x_vel = CalcHorizontalBraking(deltaf, x_vel);

		body.BuildAndTryMove(Vector2.Axis.X, x_vel);
	}
	*/

	public void OnGroundedLocomotionTick(double delta)
	{
		float deltaf = (float)delta;

		float direction = InfoManager.GetInputDirection();
		float x_vel = CalcHorizontalMovement(deltaf, direction, LandingDeaccel);
		if (GetForwardsness(direction) < 0)
		{
			x_vel = CalcHorizontalBraking(deltaf, x_vel);
		}

		_clerk.SendMovement(Vector2.Axis.X, x_vel);
	}

	// VERTICAL

	public void OnGroundedStableTick(double delta)
	{
		if (!_clerk.IsOnFloor()) 
		{
			_chart.SendEvent("Fall");
		}
		_clerk.SendMovement(Vector2.Axis.Y, 0);
	}

	public void OnGroundedCoyoteTick(double delta)
	{
		float y_vel;
		if (_clerk.IsOnFloor()) 
		{
			_chart.SendEvent("Landing");
			y_vel = 0;
		}
		else
		{
			// in air apply gravity
			y_vel = _clerk.CalcAirborneGravity(_clerk.GetVelY(), (float)delta, CoyoteGravity);
		}
		_clerk.SendMovement(Vector2.Axis.Y, y_vel);
	}
}
