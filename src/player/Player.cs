using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Godot;
using GodotStateCharts;

// are OnLanding transitions in Root/Airborne/Horizontal needed?

public partial class PlayerBody : CharacterBody2D
{

	private StateChart _chart;
	private MainCamera _camera;
	private RichTextLabel _debug;

	public void Setup(MainCamera camera, RichTextLabel debug)
	{
		GD.Print($"player setup in");
		InfoManager.RegisterPlayerBody(this);
		_chart = StateChart.Of(GetNode("StateChart"));
		_camera = camera;
		_debug = debug;

		CollisionShape2D feet = GetNode<CollisionShape2D>("RetractableFeet");
		GetNode<GroundedBehavior>("GroundedBehavior").Setup(this, _chart, feet);
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
	
	// consider merging `y_vel +/-` part into BuildAndTryMove()
	public float CalcAirborneGravity(float y_vel, float delta, float gravity)
	{
		return y_vel - (UpDirection.Y * gravity * delta);
		// potential timestep independence error! 
	}

	private Vector2 frame_vel = new(float.NaN, float.NaN);

	public bool? BuildAndTryMove(Vector2.Axis axis, float amount)
	{
		int index = (int)axis;
		frame_vel[index] = amount;

		if (frame_vel.IsFinite())
		{
			// if wall normal  and velocity x axis are opposing directions, cancel x axis movement
			if (IsOnWall() && Mathf.Sign(GetWallNormal().X * frame_vel.X) == -1.0f) {
				frame_vel.X = 0;
			}
			_chart.SetExpressionProperty("Velocity", frame_vel);

			Velocity = frame_vel;
			bool result = MoveAndSlide();
			// potential timestep independence error!
			_camera.UpdateActiveBoard(GlobalPosition);

			// _debug.Text = 
			// 	$"pos: {Position.Round()}\tlast motion: {GetLastMotion().Round()}\n" + 
			// 	$"cur vel: {GetRealVelocity().Round()}\t " + 
			//  $"prev vel: {GetPositionDelta().Round()}\n" + 
			// 	$"collision count; {GetSlideCollisionCount()}\n" +
			// 	$"ceil: {IsOnCeiling()}\tfloor: {IsOnFloor()}\twall: {IsOnWall()}";

			frame_vel = new Vector2(float.NaN, float.NaN);
			return result;
		}
		else
		{
			return null;
		}
	}

	// DEBUG

	public void PeakDebug()
	{
		GD.Print($"peak velocity: {Velocity}");
	}
	
	public void BufferDebug()
	{
		GD.Print($"buffer velocity: {Velocity}");
	}

}
