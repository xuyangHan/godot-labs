using Godot;
using System;

public partial class KillZone : Area2D
{
	private Timer _timer;

	public override void _Ready()
	{
		_timer = GetNode<Timer>("Timer");
	}

	private void _on_body_entered(Node2D body)
	{
		GD.Print("You Died!");
		_timer.Start(); 
	}

	private void _on_timer_timeout() 
	{
		GetTree().ReloadCurrentScene();
	}
}
