using Godot;

public partial class Combat : Control
{
	public override void _Ready()
	{
		// Combat UI is built in the scene tree.
		// Next: wire button signals and update bars from BattleState.
		GD.Print("Combat scene ready.");
	}
}
