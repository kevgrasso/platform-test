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
	
	private Player _body;
	private StateChart _chart;

	// RESOURCES

	public void Setup(Player body, StateChart chart)
	{
		GD.Print($"airborne setup in");
		this._body = body;
		this._chart = chart;
		GD.Print($"airborne setup out");
	}

	// VERTICAL

	public void OnJumpRiseEnter()
	{
		// jump setup
		float up = _body.UpDirection.Y;
		float jump_velocity = up * BaseJumpVelocity;
		jump_velocity += up * Mathf.Abs(_body.Velocity.X) * SpeedJumpVelBonus;

		Vector2 new_vel = new(_body.Velocity.X, jump_velocity);
		_body.Velocity = new_vel;

		//do not remove--necessary for buffered jumps!
		_chart.SetExpressionProperty("Velocity", new_vel);
		GD.Print($"jump body vel: {_body.Velocity.Y}");
	}

	private void JumpRiseMovement(float y_vel, float delta, float gravity)
	{
		// apply gravity
		if (_body.IsOnCeiling()) {
			// TODO: is this a hack/too simple/too reliant on implementation?
			y_vel = -_chart.GetExpressionProperty<Vector2>("Velocity").Y;
		} else {
			y_vel = _body.CalcAirborneGravity(y_vel, delta, gravity);
		}
		// potential timestep independence error! move to body.BuildAndTryMove()?
		_body.BuildAndTryMove(Vector2.Axis.Y, y_vel);
	}

	public void OnJumpRiseTick(double delta)
	{
		JumpRiseMovement(_body.Velocity.Y, (float)delta, JumpFloatGravity);
	}


	public void OnJumpBrakeTick(double delta)
	{
		// potential timestep independence error! change to ExpDecay()?
		JumpRiseMovement(_body.Velocity.Y * JumpCancelFactor, (float)delta, JumpFloatGravity);
	}

	public void FallMovement(float delta, float gravity)
	{
		if (_body.IsOnFloor()) {
			_chart.SendEvent("Landing");
		} 

		float deltaf = (float)delta;
		float y_vel = _body.CalcAirborneGravity(_body.Velocity.Y, deltaf, gravity);
		_body.BuildAndTryMove(Vector2.Axis.Y, y_vel);
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
		if (_body.Velocity.X * jump_direction < 0) {
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
			0.0f => _body.Velocity.X,
			_ => Mathf.MoveToward(_body.Velocity.X, oriented_max_speed, accel*delta)
		};
			
		_body.BuildAndTryMove(Vector2.Axis.X, x_vel);
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
