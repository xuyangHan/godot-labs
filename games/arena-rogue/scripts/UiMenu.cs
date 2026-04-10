using Godot;

public partial class UiMenu : Control
{
	private const string MainMenuScene = "res://scenes/main_menu.tscn";
	private const string GameScene = "res://scenes/game.tscn";

	private AudioStreamPlayer2D _clickSound;

	private Button _continueButton;
	private Button _newGameButton;
	private Button _loadSaveButton;
	private Button _optionsButton;
	private Button _exitButton;
	private Button _resumeButton;
	private Button _saveButton;
	private Button _backToMainButton;

	public override void _Ready()
	{
		if (GetParent() is CanvasLayer)
		{
			SetAnchorsPreset(LayoutPreset.FullRect);
			OffsetLeft = OffsetTop = 0;
			OffsetRight = OffsetBottom = 0;
		}

		MouseFilter = MouseFilterEnum.Stop;

		_clickSound = GetNode<AudioStreamPlayer2D>("ClickSound");

		_continueButton = GetNode<Button>("VBoxContainer/ContinueButton");
		_newGameButton = GetNode<Button>("VBoxContainer/NewGameButton");
		_loadSaveButton = GetNode<Button>("VBoxContainer/LoadSaveButton");
		_optionsButton = GetNode<Button>("VBoxContainer/OptionsButton");
		_exitButton = GetNode<Button>("VBoxContainer/ExitButton");
		_resumeButton = GetNode<Button>("VBoxContainer/ResumeButton");
		_saveButton = GetNode<Button>("VBoxContainer/SaveButton");
		_backToMainButton = GetNode<Button>("VBoxContainer/BackToMainButton");

		var isPauseMenu = FindOwningGame() != null;
		ApplyMenuContext(isPauseMenu);

		if (isPauseMenu)
		{
			_resumeButton.Pressed += OnResumePressed;
			_saveButton.Pressed += OnSavePressed;
			_backToMainButton.Pressed += OnBackToMainPressed;
		}
		else
		{
			_continueButton.Pressed += OnContinuePressed;
			_newGameButton.Pressed += OnNewGamePressed;
			_loadSaveButton.Pressed += OnMenuButtonPressed;
			_optionsButton.Pressed += OnMenuButtonPressed;
			_exitButton.Pressed += OnExitPressed;
		}
	}

	private void ApplyMenuContext(bool pauseMenu)
	{
		_resumeButton.Visible = pauseMenu;
		_saveButton.Visible = pauseMenu;
		_backToMainButton.Visible = pauseMenu;

		_continueButton.Visible = !pauseMenu && (SaveService.Instance?.CanContinue() ?? false);
		_newGameButton.Visible = !pauseMenu;
		_loadSaveButton.Visible = !pauseMenu;
		_optionsButton.Visible = !pauseMenu;
		_exitButton.Visible = !pauseMenu;
	}

	private void OnNewGamePressed()
	{
		_clickSound.Play();
		GetTree().ChangeSceneToFile(GameScene);
	}

	private void OnContinuePressed()
	{
		_clickSound.Play();
		GetTree().ChangeSceneToFile(GameScene);
	}

	private void OnResumePressed()
	{
		_clickSound.Play();
		FindOwningGame()?.SetPauseMenuVisible(false);
	}

	private void OnSavePressed()
	{
		_clickSound.Play();
	}

	private void OnBackToMainPressed()
	{
		_clickSound.Play();
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(MainMenuScene);
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

	private Game FindOwningGame()
	{
		for (var n = GetParent(); n != null; n = n.GetParent())
		{
			if (n is Game g)
				return g;
		}

		return null;
	}
}
