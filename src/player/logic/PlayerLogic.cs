using Godot;
using GodotStateCharts;

public partial class PlayerLogic : AnimationTree
{
	private PlayerBody _body;
	private StateChart _chart;
	private CollisionShape2D _feet;

	public void Setup(PlayerBody body, CollisionShape2D feet)
	{
		GD.Print($"player setup in");
		_body = body;
		_chart = StateChart.Of(GetNode("StateChart"));
		_feet = feet;

		GetNode<GroundedBehavior>("GroundedBehavior").Setup(this, _chart);
		GetNode<AirborneBehavior>("AirborneBehavior").Setup(this, _chart);
		GD.Print($"player setup out");
	}
	
	//unhandled input
	public void OnRootLateInput(InputEvent e)
	{
		if (!e.IsEcho() && e.IsAction("jump")) {
			GD.Print("jump");
			if (e.IsPressed())
			{
				GD.Print("pressed");
				_chart.SendEvent("Jump");
			} 
			else
			{
				GD.Print("released");
				_chart.SendEvent("JumpCancel");
			}
		}
	}

	public void OnGroundedEnter()
	{
		GD.Print("feet enabled");
		_feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
	}

	public void OnGroundedExit()
	{
		GD.Print("feet disabled");
		_feet.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	public void JumpInit(float jump_velocity)
	{
		Vector2 new_vel = new(_body.Velocity.X, _body.UpDirection.Y * jump_velocity);
		_body.Velocity = new_vel;

		//do not remove--necessary for buffered jumps!
		_chart.SetExpressionProperty("Velocity", new_vel);
		GD.Print($"jump body vel: {_body.Velocity.Y}");
	}

	public float GetVelX()
	{
		return _body.Velocity.X;
	}

	public float GetVelY()
	{
		return _body.Velocity.Y;
	}

	public bool IsOnCeiling()
	{
		return _body.IsOnCeiling();
	}

	public bool IsOnFloor()
	{
		return _body.IsOnFloor();
		
	}

	// consider merging `y_vel +/-` part into PlayerBody::BuildAndTryMove()
	public float CalcAirborneGravity(float y_vel, float delta, float gravity)
	{
		return y_vel - (_body.UpDirection.Y * gravity * delta);
		// potential timestep independence error! 
	}

	public bool? SendMovement(Vector2.Axis axis, float amount)
	{
		bool? result = _body.BuildAndTryMove(axis, amount);
		if (result != null)
		{
			_chart.SetExpressionProperty("Velocity", _body.Velocity);
		}
		return result;
	}
}
