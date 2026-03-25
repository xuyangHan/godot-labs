using Godot;

public partial class MainMenu : Control
{
	private AudioStreamPlayer2D _clickSound;
	private VBoxContainer _buttonContainer;

	public override void _Ready()
	{
		_clickSound = GetNode<AudioStreamPlayer2D>("ClickSound");
		_buttonContainer = GetNode<VBoxContainer>("VBoxContainer");

		foreach (var node in _buttonContainer.GetChildren())
		{
			if (node is Button button)
				button.Pressed += OnMenuButtonPressed;
		}
	}

	private void OnMenuButtonPressed()
	{
		_clickSound.Play();
	}
}
