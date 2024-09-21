using Godot;
using System;

public partial class LimboHsm : Godot.LimboHsm
{
	//FIXME:
	// head can get stuck in ceiling by jumping into a corner
	// gets caught running off ledges sometimes -- it's in narrow gaps!
	// find way to remove extra air frame delay before disabling feet
	//TODO:
	// implement focus?
	// ladders
	// max air turn speed

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
