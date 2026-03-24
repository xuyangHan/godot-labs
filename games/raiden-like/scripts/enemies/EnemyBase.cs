using Godot;

/// <summary>Shared collision bits (match *layer_names* in project.godot).</summary>
public static class GameCollisionLayers
{
	public const uint Player = 1;
	public const uint Enemy = 2;
	public const uint PlayerBullet = 4;
	public const uint EnemyBullet = 8;
}

public partial class EnemyBase : CharacterBody2D
{
	public enum MovementKind { Plane, Tank }

	[Export] public MovementKind Movement { get; set; } = MovementKind.Plane;

	/// <summary>Along +X for tanks; drift for planes while <see cref="MoveSpeed"/> drives +Y.</summary>
	[Export] public float MoveSpeed { get; set; } = 72f;

	[Export] public float PlaneDriftX { get; set; } = 0f;

	[Export] public float TankGroundY { get; set; } = -20f;

	[Export] public int MaxHealth { get; set; } = 2;

	private int _health;

	public override void _Ready()
	{
		AddToGroup("enemies");
		_health = MaxHealth;
		CollisionLayer = GameCollisionLayers.Enemy;
	}

	public void TakeDamage(int amount)
	{
		_health -= amount;
		if (_health <= 0)
			QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		switch (Movement)
		{
			case MovementKind.Plane:
				Velocity = new Vector2(PlaneDriftX, MoveSpeed);
				MoveAndSlide();
				break;
			case MovementKind.Tank:
				Velocity = new Vector2(MoveSpeed, 0f);
				MoveAndSlide();
				GlobalPosition = new Vector2(GlobalPosition.X, TankGroundY);
				break;
		}
	}
}
