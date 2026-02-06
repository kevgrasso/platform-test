using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Godot;

// are OnLanding transitions in Root/Airborne/Horizontal needed?

public partial class PlayerBody : CharacterBody2D
{

	private MainCamera _camera;
	private RichTextLabel _debug;

	public void Setup(MainCamera camera, RichTextLabel debug)
	{
		GD.Print($"player body setup in");
		InfoManager.RegisterPlayerBody(this);
		_camera = camera;
		_debug = debug;
		GD.Print($"player body setup out");
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
