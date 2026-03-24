using Godot;

public partial class Players : CharacterBody2D
{
	private static readonly PackedScene BulletScene = GD.Load<PackedScene>("res://scenes/bullet.tscn");

	public const float Speed = 300.0f;

	[Export] public int MaxHealth { get; set; } = 3;

	/// <summary>Minimum time between shots (seconds). Lower = higher fire rate.</summary>
	[Export] public float FireCooldownSeconds { get; set; } = 0.12f;

	[Export] public Vector2 BulletSpawnOffset { get; set; } = new Vector2(0f, -28f);

	private int _health;
	private float _fireCooldownRemaining;
	private AudioStreamPlayer2D _shootSfx;

	public override void _Ready()
	{
		AddToGroup("player");
		_health = MaxHealth;
		_shootSfx = GetNodeOrNull<AudioStreamPlayer2D>("ShootSfx");
	}

	public void TakeDamage(int amount)
	{
		if (amount <= 0 || _health <= 0)
			return;

		_health -= amount;
		if (_health <= 0)
			CallDeferred(nameof(ReloadCurrentSceneDeferred));
	}

	private void ReloadCurrentSceneDeferred()
	{
		GetTree().ReloadCurrentScene();
	}

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

		_shootSfx?.Play();

		var bullet = BulletScene.Instantiate<Node2D>();
		world.AddChild(bullet);
		bullet.GlobalPosition = GlobalPosition + BulletSpawnOffset;
	}
}
