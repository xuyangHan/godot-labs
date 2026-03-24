using Godot;

public partial class Bullet : Area2D
{
	[Export] public float Speed { get; set; } = 480f;

	public override void _Ready()
	{
		var notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		notifier.ScreenExited += QueueFree;
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += new Vector2(0f, -Speed) * (float)delta;
	}
}
