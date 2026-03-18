using Godot;
using System;

public partial class KillZone : Area2D
{
	// In C#, we usually get a reference to the node
	private Timer _timer;

	public override void _Ready()
	{
		// Equivalent to GDScript's $Timer
		_timer = GetNode<Timer>("Timer");
	}

	private void _on_body_entered(Node2D body)
	{
		GD.Print("You Died!");
		_timer.Start(); // Methods start with Capital letters in C#
	}

	private void _on_timer_timeout() // No colon here in C#
	{
		// GetTree() is a method, and ReloadCurrentScene() is PascalCase
		GetTree().ReloadCurrentScene();
	}
}
