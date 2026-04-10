using Godot;

public partial class UiMenu : Control
{
	private AudioStreamPlayer2D _clickSound;
	private VBoxContainer _buttonContainer;

	public override void _Ready()
	{
		_clickSound = GetNode<AudioStreamPlayer2D>("ClickSound");
		_buttonContainer = GetNode<VBoxContainer>("VBoxContainer");

		var continueButton = GetNode<Button>("VBoxContainer/ContinueButton");
		continueButton.Pressed += OnContinuePressed;

		var exitButton = GetNode<Button>("VBoxContainer/ExitButton");
		exitButton.Pressed += OnExitPressed;

		foreach (var node in _buttonContainer.GetChildren())
		{
			if (node is Button button && button != continueButton && button != exitButton)
				button.Pressed += OnMenuButtonPressed;
		}
	}

	private void OnContinuePressed()
	{
		_clickSound.Play();
		GetTree().ChangeSceneToFile("res://scenes/game.tscn");
	}

	private void OnExitPressed()
	{
		_clickSound.Play();
		GetTree().Quit();
	}

	private void OnMenuButtonPressed()
	{
		_clickSound.Play();
	}
}
