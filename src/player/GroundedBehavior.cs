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

	private Player body;
	private StateChart chart;
	private CollisionShape2D feet;

	// RESOURCES

	public void Setup(Player body, StateChart chart, CollisionShape2D feet)
	{
		GD.Print($"grounded setup in");
		this.body = body;
		this.chart = chart;
		this.feet = feet;
		GD.Print($"grounded setup out");
	}
	
	public void OnGroundedEnter()
	{
		GD.Print("feet enabled");
		feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
	}

	public void OnGroundedExit()
	{
		GD.Print("feet disabled");
		feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	// UTILITY

	private float GetForwardsness(float direction) => Mathf.Sign(body.Velocity.X * direction);

	private float CalcHorizontalMovement(float delta, float direction, float acceleration)
	{
		// handle the movement/deceleration
		float oriented_max_speed = direction * MaxSpeed;
		return Mathf.MoveToward(body.Velocity.X, oriented_max_speed, acceleration*delta);
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

		body.BuildAndTryMove(Vector2.Axis.X, x_vel);
	}

	// VERTICAL

	public void OnGroundedStableTick(double delta)
	{
		if (!body.IsOnFloor()) 
		{
			chart.SendEvent("Fall");
		}
		body.BuildAndTryMove(Vector2.Axis.Y, 0);
	}

	public void OnGroundedCoyoteTick(double delta)
	{
		float y_vel;
		if (body.IsOnFloor()) 
		{
			chart.SendEvent("Landing");
			y_vel = 0;
		}
		else
		{
			// in air apply gravity
			y_vel = body.CalcAirborneGravity(body.Velocity.Y, (float)delta, CoyoteGravity);
		}
		body.BuildAndTryMove(Vector2.Axis.Y, y_vel);
	}
}
