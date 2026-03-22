using Godot;

// Physics: world/tiles layer 1, player 2, enemies 4, weapon HitArea 8, bullets 32 + mask (1|4).
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
