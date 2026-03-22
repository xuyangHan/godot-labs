using Godot;

// Physics: player body layer 2, enemy body layer 4, weapon HitArea layer 8 + mask 4 (see weapon scenes).
public static class WeaponEquipment
{
	public static void ApplyEquipped(Node2D weapon, bool equipped)
	{
		weapon.Visible = equipped;
		weapon.ProcessMode = equipped ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;

		var hitArea = weapon.GetNodeOrNull<Area2D>("HitArea");
		if (hitArea != null)
			hitArea.SetDeferred(Area2D.PropertyName.Monitoring, false);
	}
}
