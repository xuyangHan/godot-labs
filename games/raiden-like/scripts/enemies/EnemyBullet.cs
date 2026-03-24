using Godot;

public partial class EnemyBullet : Area2D
{
	[Export] public float Speed { get; set; } = 240f;

	[Export] public int Damage { get; set; } = 1;

	/// <summary>World-space travel direction (normalized automatically).</summary>
	public Vector2 Direction { get; set; } = Vector2.Down;

	public override void _Ready()
	{
		var notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		notifier.ScreenExited += QueueFree;
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Players player)
		{
			player.TakeDamage(Damage);
			QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 dir = Direction.LengthSquared() > 0.0001f ? Direction.Normalized() : Vector2.Down;
		GlobalPosition += dir * Speed * (float)delta;
	}
}
