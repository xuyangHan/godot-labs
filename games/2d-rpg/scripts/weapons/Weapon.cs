using Godot;

[GlobalClass]
public partial class Weapon : Node2D
{
	public virtual void SetEquipped(bool equipped)
	{
		WeaponEquipment.ApplyEquipped(this, equipped);
		if (!equipped)
			TryPlayResetAnimation();
	}

	protected void TryPlayResetAnimation()
	{
		var ap = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (ap != null && ap.HasAnimation("RESET"))
			ap.Play("RESET");
	}

	public virtual void SetAimDirection(Vector2 worldDirection) { }

	public virtual void TryPrimaryAttack() { }
}
