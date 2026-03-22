using Godot;

public partial class WeaponGun : Weapon
{
	[Export] public PackedScene BulletScene { get; set; }
	[Export] public float FireCooldownSeconds { get; set; } = 0.35f;
	[Export] public float MuzzleLeadPixels { get; set; } = 10f;

	private Vector2 _aimWorld = Vector2.Right;
	private double _cooldownLeft;
	private AnimationPlayer _animationPlayer;

	public override void _Ready()
	{
		SetProcess(true);
		_animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (BulletScene == null)
			BulletScene = GD.Load<PackedScene>("res://scenes/weapons/bullet.tscn");
	}

	public override void _Process(double delta)
	{
		if (_cooldownLeft > 0)
			_cooldownLeft -= delta;
	}

	public override void SetAimDirection(Vector2 worldDirection)
	{
		if (worldDirection.LengthSquared() > 1e-6f)
			_aimWorld = worldDirection.Normalized();
	}

	public override void TryPrimaryAttack()
	{
		if (_cooldownLeft > 0 || BulletScene == null)
			return;

		var world = GetParent()?.GetParent()?.GetParent() ?? GetTree().CurrentScene;
		if (world == null)
			return;

		_animationPlayer?.Play("shoot");

		var bulletNode = BulletScene.Instantiate<Node2D>();
		world.AddChild(bulletNode);

		Vector2 spawn = GetNode<Node2D>("Sprite2D").GlobalPosition;
		spawn += _aimWorld * MuzzleLeadPixels;
		bulletNode.GlobalPosition = spawn;

		if (bulletNode is Bullet bullet)
			bullet.Velocity = _aimWorld * bullet.Speed;

		_cooldownLeft = FireCooldownSeconds;
	}
}
