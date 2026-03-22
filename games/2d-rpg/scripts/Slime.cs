using Godot;
using System;

public partial class Slime : CharacterBody2D
{
	private const float Speed = 60.0f;
	private int _direction = 1;
	
	private RayCast2D _rayCastRight;
	private RayCast2D _rayCastLeft;
	private AnimatedSprite2D _animatedSprite;

	public override void _Ready()
	{
		AddToGroup("enemies");
		_rayCastRight = GetNode<RayCast2D>("RayCastRight");
		_rayCastLeft = GetNode<RayCast2D>("RayCastLeft");
		_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public void TakeDamage(int amount, Node2D source = null)
	{
		QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		// 1. Check for collisions
		if (_rayCastRight.IsColliding())
		{
			_direction = -1;
			_animatedSprite.FlipH = true;
		}
		else if (_rayCastLeft.IsColliding())
		{
			_direction = 1;
			_animatedSprite.FlipH = false;
		}

		// 2. Apply Velocity
		// Note: We keep existing Y velocity (for gravity) and only set X
		Vector2 velocity = Velocity;
		velocity.X = _direction * Speed;
		Velocity = velocity;

		// 3. Move
		MoveAndSlide();
	}
}
