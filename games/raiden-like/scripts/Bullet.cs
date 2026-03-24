using Godot;

public partial class Bullet : Area2D
{
	[Export] public float Speed { get; set; } = 480f;

	[Export] public int Damage { get; set; } = 1;

	public override void _Ready()
	{
		var notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		notifier.ScreenExited += QueueFree;
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is EnemyBase enemy)
		{
			enemy.TakeDamage(Damage);
			QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += new Vector2(0f, -Speed) * (float)delta;
	}
}
