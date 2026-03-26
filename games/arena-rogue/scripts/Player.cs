using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 150.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		Vector2 velocity = direction * Speed;

		Velocity = velocity;
		MoveAndSlide();
	}
}
