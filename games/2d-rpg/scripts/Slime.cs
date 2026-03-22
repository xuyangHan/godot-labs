using Godot;
using System;

public partial class Slime : CharacterBody2D
{
	private const float Speed = 60.0f;
	private const float DeathFlashInDuration = 0.05f;
	private const float DeathFlashOutDuration = 0.1f;
	private const float DeathFadeDuration = 0.5f;
	private int _direction = 1;
	private bool _dying;

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
		if (_dying)
			return;
		_dying = true;

		RemoveFromGroup("enemies");
		SetPhysicsProcess(false);
		CollisionLayer = 0;
		CollisionMask = 0;

		var killZone = GetNodeOrNull<Area2D>("KillZone");
		if (killZone != null)
		{
			killZone.SetDeferred(Area2D.PropertyName.Monitoring, false);
			killZone.SetDeferred(Area2D.PropertyName.Monitorable, false);
		}

		Color baseModulate = _animatedSprite.Modulate;
		var tween = CreateTween();
		tween.TweenProperty(_animatedSprite, "modulate", new Color(3f, 3f, 3f, baseModulate.A), DeathFlashInDuration)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_animatedSprite, "modulate", baseModulate, DeathFlashOutDuration)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(_animatedSprite, "modulate", new Color(baseModulate.R, baseModulate.G, baseModulate.B, 0f), DeathFadeDuration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(QueueFree));
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
