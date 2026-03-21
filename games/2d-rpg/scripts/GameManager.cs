using Godot;

public partial class GameManager : Node
{
	[Signal]
	public delegate void CoinsChangedEventHandler(int newValue);
	[Signal]
	public delegate void HealthChangedEventHandler(int newValue);

	public const int MaxHealth = 3;
	public int Coins { get; private set; }
	public int Health { get; private set; } = MaxHealth;

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
		Health = MaxHealth;
		EmitSignal(SignalName.CoinsChanged, Coins);
		EmitSignal(SignalName.HealthChanged, Health);
	}

	public bool TakeDamage(int amount = 1)
	{
		if (amount <= 0 || Health <= 0)
		{
			return Health <= 0;
		}

		Health = Mathf.Max(0, Health - amount);
		EmitSignal(SignalName.HealthChanged, Health);

		return Health <= 0;
	}
}
