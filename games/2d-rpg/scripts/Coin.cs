using Godot;
using System;

public partial class Coin : Area2D
{
	private AnimationPlayer _AnimationPlayer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void _on_body_entered(Node2D body)
	{
		GD.Print("Coin collected by: " + body.Name);
		_AnimationPlayer.Play("pickup");
	}
}
