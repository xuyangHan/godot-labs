using Godot;

public partial class Players : CharacterBody2D
{
	private static readonly PackedScene BulletScene = GD.Load<PackedScene>("res://scenes/bullet.tscn");

	public const float Speed = 300.0f;

	/// <summary>Minimum time between shots (seconds). Lower = higher fire rate.</summary>
	[Export] public float FireCooldownSeconds { get; set; } = 0.12f;

	[Export] public Vector2 BulletSpawnOffset { get; set; } = new Vector2(0f, -28f);

	private float _fireCooldownRemaining;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		Vector2 velocity = direction * Speed;

		Velocity = velocity;
		MoveAndSlide();

		_fireCooldownRemaining -= (float)delta;

		if (Input.IsPhysicalKeyPressed(Key.Space) && _fireCooldownRemaining <= 0f)
		{
			Shoot();
			_fireCooldownRemaining = FireCooldownSeconds;
		}
	}

	private void Shoot()
	{
		var world = GetParent();
		if (world == null || BulletScene == null)
			return;

		var bullet = BulletScene.Instantiate<Node2D>();
		world.AddChild(bullet);
		bullet.GlobalPosition = GlobalPosition + BulletSpawnOffset;
	}
}
