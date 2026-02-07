using Godot;
using GodotStateCharts;

public partial class AirborneBehavior : Node
{
	// exposed godot inspector parameter "constants"
	[Export] public float StandardAccel = 159.0f; //2.65f;
	[Export] public float TurningAccel = 30.0f; //0.5f;
	[Export] public float MaxSpeed = 130.0f;
	[Export] public float BaseJumpVelocity = 140.0f;
	[Export] public float SpeedJumpVelBonus = 0.15f;
	[Export] public float NormalGravity = 705.0f;
	[Export] public float JumpFloatGravity = 200.0f;
	[Export] public float FallFloatGravity = 235.0f; 
	[Export] public float JumpCancelFactor = 0.75f;
	// godot nodes
	
	private PlayerMachineClerk _clerk;
	private StateChart _chart; // get rid of chart knowledge?

	// RESOURCES

	public void Setup(PlayerMachineClerk clerk, StateChart chart)
	{
		GD.Print($"airborne setup in");
		this._clerk = clerk;
		this._chart = chart;
		GD.Print($"airborne setup out");
	}

	// VERTICAL
	
	public void OnJumpRiseEnter()
	{
		// jump setup
		float jump_velocity = BaseJumpVelocity;
		jump_velocity += Mathf.Abs(_clerk.GetVelX()) * SpeedJumpVelBonus;

		_clerk.JumpInit(jump_velocity);
	}

	private void JumpRiseMovement(float y_vel, float delta, float gravity)
	{
		// apply gravity
		if (_clerk.IsOnCeiling()) {
			// TODO: is this a hack/too simple/too reliant on implementation?
			y_vel = -_chart.GetExpressionProperty<Vector2>("Velocity").Y;
			// to be clear, it's getting the velocity from the previous frame
		} else {
			y_vel = _clerk.CalcAirborneGravity(y_vel, delta, gravity);
		}
		// potential timestep independence error! move to body.BuildAndTryMove()?
		_clerk.SendMovement(Vector2.Axis.Y, y_vel);
	}

	public void OnJumpRiseTick(double delta)
	{
		JumpRiseMovement(_clerk.GetVelY(), (float)delta, JumpFloatGravity);
	}


	public void OnJumpBrakeTick(double delta)
	{
		// potential timestep independence error! change to ExpDecay()?
		JumpRiseMovement(_clerk.GetVelY() * JumpCancelFactor, (float)delta, JumpFloatGravity);
	}

	public void FallMovement(float delta, float gravity)
	{
		if (_clerk.IsOnFloor()) {
			_chart.SendEvent("Landing");
		} 

		float deltaf = (float)delta;
		float y_vel = _clerk.CalcAirborneGravity(_clerk.GetVelY(), deltaf, gravity);
		_clerk.SendMovement(Vector2.Axis.Y, y_vel);
	}

	public void OnSlowFallTick(double delta)
	{
		FallMovement((float)delta, FallFloatGravity);
	}

	public void OnQuickFallTick(double delta)
	{
		FallMovement((float)delta, NormalGravity);
	}

	// HORIZONTAL

	private float CalcDirAccel(float jump_direction) {
		// if moving the opposite direction to the start of the jump
		if (_clerk.GetVelX() * jump_direction < 0) {
			// backwards jump case--nerf acceleration
			return TurningAccel;
		} else {
			// standard jump case
			return StandardAccel;
		}
	}

	private void HorizontalMovement(float delta, float accel)
	{
		// calculate the movement
		float direction = InfoManager.GetInputDirection();
		float oriented_max_speed = direction * MaxSpeed;
		float x_vel = direction switch {
			0.0f => _clerk.GetVelX(),
			_ => Mathf.MoveToward(_clerk.GetVelX(), oriented_max_speed, accel*delta)
		};
			
		_clerk.SendMovement(Vector2.Axis.X, x_vel);
	}

	public void OnHorizontalUnrestrictedTick(double delta)
	{
		HorizontalMovement((float)delta, StandardAccel);
	}

	public void OnHorizontalRestrictedLeftTick(double delta)
	{
		HorizontalMovement((float)delta, CalcDirAccel(-1.0f));
	}
	public void OnHorizontalRestrictedRightTick(double delta)
	{
		HorizontalMovement((float)delta, CalcDirAccel(1.0f));
	}
}
