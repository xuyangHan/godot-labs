using Godot;

public partial class Game : Node2D
{
	private Control _uiMenu;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		_uiMenu = GetNode<Control>("UILayer/UIMenu");

		foreach (var child in GetChildren())
		{
			if (child is CanvasLayer)
				continue;
			child.ProcessMode = ProcessModeEnum.Pausable;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!@event.IsActionPressed("ui_cancel"))
			return;

		SetPauseMenuVisible(!_uiMenu.Visible);
		GetViewport().SetInputAsHandled();
	}

	public void SetPauseMenuVisible(bool visible)
	{
		_uiMenu.Visible = visible;
		GetTree().Paused = visible;
	}
}
