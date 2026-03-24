using Godot;

public partial class GameManager : Node
{
	[Signal]
	public delegate void CoinsChangedEventHandler(int newValue);

	[Signal]
	public delegate void HealthChangedEventHandler(int newValue);

	private int _maxHealth = 3;

	/// <summary>Current run max HP (set from player export before <see cref="ResetRun"/>).</summary>
	public int MaxHealth => _maxHealth;

	public int Coins { get; private set; }
	public int Health { get; private set; } = 3;

	public void SetMaxHealth(int max)
	{
		_maxHealth = Mathf.Max(1, max);
	}

	public void AddCoin(int amount = 1)
	{
		if (amount <= 0)
			return;

		Coins += amount;
		EmitSignal(SignalName.CoinsChanged, Coins);
	}

	public void ResetRun()
	{
		Coins = 0;
		Health = _maxHealth;
		EmitSignal(SignalName.CoinsChanged, Coins);
		EmitSignal(SignalName.HealthChanged, Health);
	}

	public bool TakeDamage(int amount = 1)
	{
		if (amount <= 0 || Health <= 0)
			return Health <= 0;

		Health = Mathf.Max(0, Health - amount);
		EmitSignal(SignalName.HealthChanged, Health);

		return Health <= 0;
	}

	/// <returns><see langword="true"/> if any HP was restored (pickup consumed).</returns>
	public bool AddHealth(int amount = 1)
	{
		if (amount <= 0 || Health >= _maxHealth)
			return false;

		int before = Health;
		Health = Mathf.Min(_maxHealth, Health + amount);
		if (Health != before)
			EmitSignal(SignalName.HealthChanged, Health);

		return Health != before;
	}
}
