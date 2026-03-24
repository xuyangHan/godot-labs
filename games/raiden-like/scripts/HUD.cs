using Godot;

public partial class HUD : CanvasLayer
{
	private Label _coinLabel;
	private Label _healthLabel;

	public override void _Ready()
	{
		_coinLabel = GetNodeOrNull<Label>("MarginContainer/Stats/CoinLabel");
		_healthLabel = GetNode<Label>("MarginContainer/Stats/HealthLabel");

		GameManager gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			if (_coinLabel != null)
				gameManager.CoinsChanged += OnCoinsChanged;
			gameManager.HealthChanged += OnHealthChanged;
			if (_coinLabel != null)
				OnCoinsChanged(gameManager.Coins);
			OnHealthChanged(gameManager.Health);
		}
		else
		{
			if (_coinLabel != null)
				_coinLabel.Text = "Coins: 0";
			_healthLabel.Text = "Health: 3";
		}
	}

	public override void _ExitTree()
	{
		GameManager gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			if (_coinLabel != null)
				gameManager.CoinsChanged -= OnCoinsChanged;
			gameManager.HealthChanged -= OnHealthChanged;
		}
	}

	private void OnCoinsChanged(int newValue)
	{
		if (_coinLabel != null)
			_coinLabel.Text = $"Coins: {newValue}";
	}

	private void OnHealthChanged(int newValue)
	{
		_healthLabel.Text = $"Health: {newValue}";
	}
}
