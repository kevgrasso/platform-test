using Godot;
using System;

public partial class CockpitHUD : HBoxContainer {
	public Vector2 GetAperatureSize() {
		return GetNode<ReferenceRect>("%AperatureRect").Size;
	}
}
