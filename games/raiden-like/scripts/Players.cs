using System.Threading.Tasks;
using Godot;

public partial class Players : CharacterBody2D
{
	private static readonly PackedScene BulletScene = GD.Load<PackedScene>("res://scenes/bullet.tscn");

	public const float Speed = 300.0f;

	[Export] public int MaxHealth { get; set; } = 3;

	/// <summary>Minimum time between shots (seconds). Lower = higher fire rate.</summary>
	[Export] public float FireCooldownSeconds { get; set; } = 0.12f;

	[Export] public Vector2 BulletSpawnOffset { get; set; } = new Vector2(0f, -28f);

	[ExportGroup("Enemy contact")]
	[Export] public int EnemyContactDamage { get; set; } = 1;

	/// <summary>Min time between damage ticks from touching enemies (avoids physics jitter spam).</summary>
	[Export] public float EnemyContactCooldownSeconds { get; set; } = 0.35f;

	private float _fireCooldownRemaining;
	private float _enemyContactCooldown;
	private GameManager _gameManager;
	private Area2D _hurtbox;
	private AudioStreamPlayer2D _shootSfx;
	private AudioStreamPlayer2D _hitSound;
	private AudioStreamPlayer2D _killSound;
	private ColorRect _damageOverlay;
	private Timer _deathTimer;

	private bool _isDying;
	private bool _isHitFeedbackPlaying;

	public override void _Ready()
	{
		AddToGroup("player");
		_gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		_gameManager?.SetMaxHealth(MaxHealth);
		_gameManager?.ResetRun();
		_shootSfx = GetNodeOrNull<AudioStreamPlayer2D>("ShootSfx");
		_hitSound = GetNodeOrNull<AudioStreamPlayer2D>("HitSound");
		_killSound = GetNodeOrNull<AudioStreamPlayer2D>("KillSound");
		_damageOverlay = GetNodeOrNull<ColorRect>("FxLayer/DamageOverlay");
		_deathTimer = GetNodeOrNull<Timer>("DeathTimer");
		if (_deathTimer != null)
			_deathTimer.Timeout += OnDeathTimerTimeout;

		if (_damageOverlay != null)
			_damageOverlay.Visible = false;

		_hurtbox = GetNodeOrNull<Area2D>("Hurtbox");
		if (_hurtbox != null)
			_hurtbox.BodyEntered += OnHurtboxBodyEntered;
	}

	public override void _ExitTree()
	{
		if (_deathTimer != null)
			_deathTimer.Timeout -= OnDeathTimerTimeout;
		if (_hurtbox != null)
			_hurtbox.BodyEntered -= OnHurtboxBodyEntered;
	}

	private void OnHurtboxBodyEntered(Node2D body)
	{
		if (_isDying || body is not EnemyBase)
			return;
		if (_enemyContactCooldown > 0f)
			return;

		TakeDamage(EnemyContactDamage);
		_enemyContactCooldown = EnemyContactCooldownSeconds;
	}

	public void TakeDamage(int amount)
	{
		if (amount <= 0 || _isDying)
			return;
		if (_gameManager == null || _gameManager.Health <= 0)
			return;

		bool dead = _gameManager.TakeDamage(amount);

		if (!dead)
		{
			_hitSound?.Play();
			_ = PlayHitFeedbackAsync();
			return;
		}

		StartDeathSequence();
	}

	private void OnDeathTimerTimeout()
	{
		Engine.TimeScale = 1.0f;
		GetNodeOrNull<GameManager>("/root/GameManager")?.ResetRun();
		GetTree().ReloadCurrentScene();
	}

	private async Task PlayHitFeedbackAsync()
	{
		if (_isHitFeedbackPlaying || _isDying)
			return;

		_isHitFeedbackPlaying = true;
		Engine.TimeScale = 0.5f;

		if (_damageOverlay != null)
		{
			_damageOverlay.Visible = true;
			_damageOverlay.Color = new Color(1, 0, 0, 0.55f);
			Tween tween = CreateTween();
			tween.TweenProperty(_damageOverlay, "color", new Color(1, 0, 0, 0), 0.12f)
				.SetTrans(Tween.TransitionType.Quint);
		}

		await ToSignal(GetTree().CreateTimer(0.08f), SceneTreeTimer.SignalName.Timeout);

		if (!_isDying)
		{
			Engine.TimeScale = 1.0f;
			if (_damageOverlay != null)
				_damageOverlay.Visible = false;
		}

		_isHitFeedbackPlaying = false;
	}

	private void StartDeathSequence()
	{
		_isDying = true;
		CollisionLayer = 0;
		CollisionMask = 0;
		_hurtbox?.SetDeferred(Area2D.PropertyName.Monitoring, false);
		SetPhysicsProcess(false);
		SetProcess(false);

		_killSound?.Play();

		if (_damageOverlay != null)
		{
			_damageOverlay.Visible = true;
			Tween tween = CreateTween();
			_damageOverlay.Color = new Color(0, 0, 0, 0);
			tween.TweenProperty(_damageOverlay, "color", new Color(0, 0, 0, 1), 0.5f)
				.SetTrans(Tween.TransitionType.Quint);
		}

		Engine.TimeScale = 0.5f;
		_deathTimer?.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		Vector2 velocity = direction * Speed;

		Velocity = velocity;
		MoveAndSlide();

		_enemyContactCooldown = Mathf.Max(0f, _enemyContactCooldown - (float)delta);
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
