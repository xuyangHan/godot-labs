using System.Threading.Tasks;
using Godot;

/// <summary>Falls straight down; heals the player on contact via <see cref="GameManager"/>.</summary>
public partial class HealthPackage : Area2D
{
	[Export] public int HealAmount { get; set; } = 1;

	[Export] public float FallSpeedPixels { get; set; } = 72f;

	private AudioStreamPlayer2D _sfx;
	private GameManager _gameManager;
	private VisibleOnScreenNotifier2D _notifier;
	private bool _consumed;

	public override void _Ready()
	{
		_sfx = GetNodeOrNull<AudioStreamPlayer2D>("AddHealthSound");
		_gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		BodyEntered += OnBodyEntered;

		_notifier = GetNodeOrNull<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		if (_notifier != null)
			_notifier.ScreenExited += OnScreenExited;
	}

	public override void _ExitTree()
	{
		BodyEntered -= OnBodyEntered;
		if (_notifier != null)
			_notifier.ScreenExited -= OnScreenExited;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_consumed)
			return;

		GlobalPosition += Vector2.Down * FallSpeedPixels * (float)delta;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_consumed || body is not Players)
			return;
		if (_gameManager == null || HealAmount <= 0)
		{
			QueueFree();
			return;
		}

		// GameManager owns current HP + max; HUD listens to HealthChanged.
		if (!_gameManager.AddHealth(HealAmount))
			return;

		_consumed = true;
		SetDeferred(Area2D.PropertyName.Monitoring, false);
		// Play AddHealthSound (child node) until it finishes, then remove pickup.
		_ = ConsumeAndFreeAsync();
	}

	private async Task ConsumeAndFreeAsync()
	{
		_sfx?.Play();

		if (_sfx?.Stream != null)
			await ToSignal(_sfx, AudioStreamPlayer2D.SignalName.Finished);
		else
			await ToSignal(GetTree().CreateTimer(0.05f), SceneTreeTimer.SignalName.Timeout);

		QueueFree();
	}

	/// <summary>Despawn if it leaves the view (e.g. player missed it).</summary>
	private void OnScreenExited()
	{
		if (!_consumed)
			QueueFree();
	}
}
