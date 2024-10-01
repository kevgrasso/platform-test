using Godot;
using System;

public partial class CockpitHUD : HBoxContainer {
	public Rect2 GetApertureRect() {
		ReferenceRect aperture = GetNode<ReferenceRect>("%ApertureRect");
		return new Rect2(aperture.GlobalPosition, aperture.Size);
	}
}
