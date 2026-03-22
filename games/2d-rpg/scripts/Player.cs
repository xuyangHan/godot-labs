using Godot;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	public const float Speed = 130.0f;

	/// <summary>Max angle between aim direction and horizontal (0° = straight right/left). Smaller = less aim over head / under feet.</summary>
	[Export(PropertyHint.Range, "20,80,1")]
	public float MaxAimElevationFromHorizontalDegrees { get; set; } = 52f;

	private Node2D _weaponPivot;
	private readonly List<Weapon> _weapons = new();
	private int _loadoutSlot;
	private bool _tabPhysicalPrev;

	private Weapon ActiveWeapon =>
		_loadoutSlot >= 1 && _loadoutSlot <= _weapons.Count ? _weapons[_loadoutSlot - 1] : null;

	public override void _Ready()
	{
		AddToGroup("player");
		SetProcess(true);
		_weaponPivot = GetNode<Node2D>("WeaponPivot");
		RefreshWeaponList();
		if (_weapons.Count == 0)
			CallDeferred(nameof(DeferredRefreshWeapons));
		else
			ApplyLoadout();
	}

	private void DeferredRefreshWeapons()
	{
		RefreshWeaponList();
		ApplyLoadout();
	}

	private void RefreshWeaponList()
	{
		_weapons.Clear();
		foreach (var child in _weaponPivot.GetChildren())
		{
			if (child is Weapon weapon)
				_weapons.Add(weapon);
		}
		if (_weapons.Count > 0)
			return;
		var sword = _weaponPivot.GetNodeOrNull("weapon_sword");
		if (sword is Weapon w)
			_weapons.Add(w);
	}

	public override void _Process(double delta)
	{
		bool tabPhysical = Input.IsPhysicalKeyPressed(Key.Tab);
		bool tabPhysicalEdge = tabPhysical && !_tabPhysicalPrev;
		_tabPhysicalPrev = tabPhysical;

		if (tabPhysicalEdge || Input.IsActionJustPressed("weapon_cycle"))
			CycleWeapon();

		var active = ActiveWeapon;
		if (active != null)
		{
			Vector2 mouse = GetGlobalMousePosition();
			Vector2 raw = mouse - _weaponPivot.GlobalPosition;
			if (raw.LengthSquared() > 0.0001f)
			{
				Vector2 dir = raw.Normalized();
				float maxVert = Mathf.Sin(Mathf.DegToRad(MaxAimElevationFromHorizontalDegrees));
				dir.Y = Mathf.Clamp(dir.Y, -maxVert, maxVert);
				dir = dir.LengthSquared() > 1e-6f ? dir.Normalized() : new Vector2(1f, 0f);

				float angle = dir.Angle();
				if (active is WeaponSword sword)
					angle += sword.AimOffsetRadians;
				_weaponPivot.Rotation = angle;

				bool aimLeft = mouse.X < _weaponPivot.GlobalPosition.X;
				_weaponPivot.Scale = aimLeft ? new Vector2(1f, -1f) : new Vector2(1f, 1f);

				active.SetAimDirection(dir);
			}

			if (Input.IsActionJustPressed("attack"))
				active.TryPrimaryAttack();
		}
		else
		{
			_weaponPivot.Scale = Vector2.One;
			_weaponPivot.Rotation = 0f;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		Vector2 velocity = direction * Speed;

		Velocity = velocity;
		MoveAndSlide();

		var sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		if (direction != Vector2.Zero)
		{
			sprite.Play("walk");
		}
		else
		{
			sprite.Play("idle");
		}

		if (direction.X > 0)
			sprite.FlipH = false;
		else if (direction.X < 0)
			sprite.FlipH = true;
	}

	private void CycleWeapon()
	{
		if (_weapons.Count == 0)
			RefreshWeaponList();
		int slotCount = _weapons.Count + 1;
		if (slotCount <= 1)
			return;
		_loadoutSlot = (_loadoutSlot + 1) % slotCount;
		ApplyLoadout();
	}

	private void ApplyLoadout()
	{
		for (int i = 0; i < _weapons.Count; i++)
			_weapons[i].SetEquipped(_loadoutSlot == i + 1);
	}
}
