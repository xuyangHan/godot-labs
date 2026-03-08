using Godot;
using System;

public partial class Square : Button
{
	public int X;
	public int Y;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var style = new StyleBoxFlat();
		
		var hoverStyle = new StyleBoxFlat();
		hoverStyle.BgColor = style.BgColor.Lightened(0.1f);
		
		if ((X + Y) % 2 == 0)
			Modulate = new Color("#ccdae0");
		else
			Modulate = new Color("#7498ad");
			
		AddThemeStyleboxOverride("normal", style);
		AddThemeStyleboxOverride("hover", hoverStyle);
		AddThemeStyleboxOverride("pressed", style);
		AddThemeStyleboxOverride("focus", style);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	

	public override void _Pressed()
	{
		GD.Print($"Clicked square {X},{Y}");
	}
}
