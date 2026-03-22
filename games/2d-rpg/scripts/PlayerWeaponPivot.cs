using Godot;
using System.Collections.Generic;

public partial class PlayerWeaponPivot : Node2D
{
	[Export(PropertyHint.Range, "20,80,1")]
	public float MaxAimElevationFromHorizontalDegrees { get; set; } = 52f;

	private readonly List<Weapon> _weapons = new();
	private int _loadoutSlot;
	private bool _tabPhysicalPrev;

	private Weapon ActiveWeapon =>
		_loadoutSlot >= 1 && _loadoutSlot <= _weapons.Count ? _weapons[_loadoutSlot - 1] : null;

	public override void _Ready()
	{
		SetProcess(true);
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
		foreach (var child in GetChildren())
		{
			if (child is Weapon weapon)
				_weapons.Add(weapon);
		}
		if (_weapons.Count > 0)
			return;
		TryAddWeaponFallback("weapon_sword");
		TryAddWeaponFallback("weapon_gun");
	}

	private void TryAddWeaponFallback(string nodeName)
	{
		var n = GetNodeOrNull(nodeName);
		if (n is Weapon w)
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
			Vector2 raw = mouse - GlobalPosition;
			if (raw.LengthSquared() > 0.0001f)
			{
				Vector2 dir = raw.Normalized();
				float maxVert = Mathf.Sin(Mathf.DegToRad(MaxAimElevationFromHorizontalDegrees));
				dir.Y = Mathf.Clamp(dir.Y, -maxVert, maxVert);
				dir = dir.LengthSquared() > 1e-6f ? dir.Normalized() : new Vector2(1f, 0f);

				float angle = dir.Angle() + active.AimOffsetRadians;
				Rotation = angle;

				bool aimLeft = mouse.X < GlobalPosition.X;
				Scale = aimLeft ? new Vector2(1f, -1f) : new Vector2(1f, 1f);

				active.SetAimDirection(dir);
			}

			if (Input.IsActionJustPressed("attack"))
				active.TryPrimaryAttack();
		}
		else
		{
			Scale = Vector2.One;
			Rotation = 0f;
		}
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
