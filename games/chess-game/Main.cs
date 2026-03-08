using Godot;
using System;

public partial class Main : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GridContainer board = GetNode<GridContainer>("Board");

		var squareScene = GD.Load<PackedScene>("res://Square.tscn");

		for (int y = 0; y < 8; y++)
		{
			for (int x = 0; x < 8; x++)
			{
				Square square = squareScene.Instantiate<Square>();

				square.X = x;
				square.Y = y;

				board.AddChild(square);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
