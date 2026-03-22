using Godot;

public partial class Bullet : Area2D
{
	[Export] public uint WorldCollisionLayerMask { get; set; } = 1;

	[Export] public float Speed { get; set; } = 420f;
	[Export] public float LifetimeSeconds { get; set; } = 4f;

	public Vector2 Velocity { get; set; }

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		GetTree().CreateTimer(LifetimeSeconds).Timeout += OnLifetimeEnded;
	}

	public override void _Process(double delta)
	{
		GlobalPosition += Velocity * (float)delta;
	}

	private void OnLifetimeEnded()
	{
		if (IsInstanceValid(this))
			QueueFree();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body == null || !IsInstanceValid(this))
			return;
		if (body.IsInGroup("player"))
			return;

		if (body is Slime slime)
		{
			slime.TakeDamage(1, this);
			QueueFree();
			return;
		}

		if (body.IsInGroup("enemies") && body.HasMethod("TakeDamage"))
		{
			body.Call("TakeDamage", 1, this);
			QueueFree();
			return;
		}

		if (body is TileMapLayer)
		{
			QueueFree();
			return;
		}

		if (body is CollisionObject2D co && (co.CollisionLayer & WorldCollisionLayerMask) != 0u)
			QueueFree();
	}
}
