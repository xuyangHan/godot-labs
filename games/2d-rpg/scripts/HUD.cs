using Godot;

public partial class HUD : CanvasLayer
{
	private Label _coinLabel;
	private Label _healthLabel;

	public override void _Ready()
	{
		_coinLabel = GetNode<Label>("MarginContainer/Stats/CoinLabel");
		_healthLabel = GetNode<Label>("MarginContainer/Stats/HealthLabel");

		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			gameManager.CoinsChanged += OnCoinsChanged;
			gameManager.HealthChanged += OnHealthChanged;
			OnCoinsChanged(gameManager.Coins);
			OnHealthChanged(gameManager.Health);
		}
		else
		{
			_coinLabel.Text = "Coins: 0";
			_healthLabel.Text = $"Health: {GameManager.MaxHealth}";
		}
	}

	public override void _ExitTree()
	{
		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			gameManager.CoinsChanged -= OnCoinsChanged;
			gameManager.HealthChanged -= OnHealthChanged;
		}
	}

	private void OnCoinsChanged(int newValue)
	{
		_coinLabel.Text = $"Coins: {newValue}";
	}

	private void OnHealthChanged(int newValue)
	{
		_healthLabel.Text = $"Health: {newValue}";
	}
}
