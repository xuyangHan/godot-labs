using Godot;

public partial class GameManager : Node
{
	[Signal]
	public delegate void CoinsChangedEventHandler(int newValue);

	public int Coins { get; private set; }

	public void AddCoin(int amount = 1)
	{
		if (amount <= 0)
		{
			return;
		}

		Coins += amount;
		EmitSignal(SignalName.CoinsChanged, Coins);
	}

	public void ResetRun()
	{
		Coins = 0;
		EmitSignal(SignalName.CoinsChanged, Coins);
	}
}
