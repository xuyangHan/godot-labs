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
	private const float DeathFlashInDuration = 0.05f;
	private const float DeathFlashOutDuration = 0.1f;
	private const float DeathFadeDuration = 0.5f;

	private static readonly PackedScene EnemyBulletScene = GD.Load<PackedScene>("res://scenes/enemy_bullet.tscn");

	public enum MovementKind { Plane, Tank }

	[Export] public MovementKind Movement { get; set; } = MovementKind.Plane;

	/// <summary>Along +X for tanks; drift for planes while <see cref="MoveSpeed"/> drives +Y.</summary>
	[Export] public float MoveSpeed { get; set; } = 72f;

	[Export] public float PlaneDriftX { get; set; } = 0f;

	[Export] public float TankGroundY { get; set; } = -20f;

	[Export] public int MaxHealth { get; set; } = 2;

	[ExportGroup("Weapon")]
	[Export] public float FireCooldownSeconds { get; set; } = 1.35f;

	[Export] public float FireCooldownJitter { get; set; } = 0.45f;

	/// <summary>Half-angle (radians) around <see cref="Vector2.Down"/> for random aim.</summary>
	[Export] public float FireSpreadHalfAngleRadians { get; set; } = 0.55f;

	[Export] public Vector2 BulletSpawnOffset { get; set; } = new Vector2(0f, 18f);

	[Export] public float BulletSpeed { get; set; } = 240f;

	private int _health;
	private float _fireCooldownRemaining;
	private bool _dying;
	private CanvasItem _deathVisual;

	public override void _Ready()
	{
		AddToGroup("enemies");
		_health = MaxHealth;
		CollisionLayer = GameCollisionLayers.Enemy;
		_deathVisual = FindDeathVisualSprite();
		ResetFireCooldown();
	}

	public void TakeDamage(int amount)
	{
		if (_dying || amount <= 0)
			return;

		_health -= amount;
		if (_health > 0)
			return;

		_health = 0;
		BeginDeathSequence();
	}

	private CanvasItem FindDeathVisualSprite()
	{
		foreach (Node n in GetChildren())
		{
			if (n.Name == "shadow")
				continue;
			if (n is Sprite2D s)
				return s;
			if (n is AnimatedSprite2D a)
				return a;
		}

		return null;
	}

	private void BeginDeathSequence()
	{
		if (_dying)
			return;
		_dying = true;

		RemoveFromGroup("enemies");
		SetPhysicsProcess(false);
		CollisionLayer = 0;
		CollisionMask = 0;

		CanvasItem visual = _deathVisual ?? FindDeathVisualSprite();
		if (visual == null)
		{
			QueueFree();
			return;
		}

		Color baseModulate = visual.Modulate;
		Tween tween = CreateTween();
		tween.TweenProperty(visual, "modulate", new Color(3f, 3f, 3f, baseModulate.A), DeathFlashInDuration)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(visual, "modulate", baseModulate, DeathFlashOutDuration)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(visual, "modulate", new Color(baseModulate.R, baseModulate.G, baseModulate.B, 0f), DeathFadeDuration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(QueueFree));
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

		float dt = (float)delta;
		_fireCooldownRemaining -= dt;
		if (_fireCooldownRemaining <= 0f)
		{
			TryFireEnemyBullet();
			ResetFireCooldown();
		}
	}

	private void ResetFireCooldown()
	{
		float jitter = FireCooldownJitter > 0f
			? (float)GD.RandRange(-FireCooldownJitter, FireCooldownJitter)
			: 0f;
		_fireCooldownRemaining = Mathf.Max(0.05f, FireCooldownSeconds + jitter);
	}

	private void TryFireEnemyBullet()
	{
		if (EnemyBulletScene == null)
			return;

		Node world = GetParent();
		if (world == null)
			return;

		float spread = FireSpreadHalfAngleRadians;
		float angle = spread > 0f ? (float)GD.RandRange(-spread, spread) : 0f;
		Vector2 dir = Vector2.Down.Rotated(angle);

		var bullet = EnemyBulletScene.Instantiate<EnemyBullet>();
		bullet.Direction = dir;
		bullet.Speed = BulletSpeed;
		world.AddChild(bullet);
		bullet.GlobalPosition = GlobalPosition + BulletSpawnOffset;
	}
}
