using Godot;
using System.Collections.Generic;

public partial class WeaponSword : Weapon
{
	private const double HitWindowStart = 0.2;
	private const double HitWindowEnd = 0.37;

	private Area2D _hitArea;
	private AnimationPlayer _animationPlayer;
	private readonly HashSet<ulong> _hitThisSwing = new();
	private bool _wasInHitWindow;

	public override void _Ready()
	{
		SetProcess(true);
		_hitArea = GetNodeOrNull<Area2D>("HitArea");
		_animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (_hitArea != null)
			_hitArea.BodyEntered += OnHitBodyEntered;
	}

	public override void _ExitTree()
	{
		if (_hitArea != null)
			_hitArea.BodyEntered -= OnHitBodyEntered;
	}

	public override void TryPrimaryAttack()
	{
		if (_animationPlayer == null || _animationPlayer.IsPlaying())
			return;
		_hitThisSwing.Clear();
		_wasInHitWindow = false;
		if (_hitArea != null)
			_hitArea.Monitoring = false;
		_animationPlayer.Play("attack");
	}

	public override void _Process(double delta)
	{
		if (!Visible || _hitArea == null || _animationPlayer == null)
			return;

		bool attacking = _animationPlayer.IsPlaying() && _animationPlayer.CurrentAnimation == "attack";
		if (!attacking)
		{
			if (_hitArea.Monitoring)
				_hitArea.Monitoring = false;
			_wasInHitWindow = false;
			return;
		}

		double t = _animationPlayer.CurrentAnimationPosition;
		bool inHit = t >= HitWindowStart && t < HitWindowEnd;
		if (inHit && !_wasInHitWindow)
			_hitThisSwing.Clear();
		if (inHit != _hitArea.Monitoring)
			_hitArea.Monitoring = inHit;
		_wasInHitWindow = inHit;
	}

	private void OnHitBodyEntered(Node2D body)
	{
		if (body == null || !_hitArea.Monitoring)
			return;
		if (body.IsInGroup("player"))
			return;
		ulong id = body.GetInstanceId();
		if (!_hitThisSwing.Add(id))
			return;

		if (body is Slime slime)
		{
			slime.TakeDamage(1, this);
			return;
		}

		if (body.IsInGroup("enemies") && body.HasMethod("TakeDamage"))
			body.Call("TakeDamage", 1, this);
	}
}
