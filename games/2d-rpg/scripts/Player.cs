using Godot;

public partial class Player : CharacterBody2D
{
	public const float Speed = 130.0f;

	public override void _Ready()
	{
		AddToGroup("player");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		Vector2 velocity = direction * Speed;

		Velocity = velocity;
		MoveAndSlide();

		var sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		if (direction != Vector2.Zero)
			sprite.Play("walk");
		else
			sprite.Play("idle");

		if (direction.X > 0)
			sprite.FlipH = false;
		else if (direction.X < 0)
			sprite.FlipH = true;
	}
}
