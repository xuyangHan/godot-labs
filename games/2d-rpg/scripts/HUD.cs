using Godot;

public partial class HUD : CanvasLayer
{
	private Label _coinLabel;

	public override void _Ready()
	{
		_coinLabel = GetNode<Label>("MarginContainer/CoinLabel");

		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			gameManager.CoinsChanged += OnCoinsChanged;
			OnCoinsChanged(gameManager.Coins);
		}
		else
		{
			_coinLabel.Text = "Coins: 0";
		}
	}

	public override void _ExitTree()
	{
		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			gameManager.CoinsChanged -= OnCoinsChanged;
		}
	}

	private void OnCoinsChanged(int newValue)
	{
		_coinLabel.Text = $"Coins: {newValue}";
	}
}
