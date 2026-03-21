using Godot;
using System;
using System.Threading.Tasks;

public partial class KillZone : Area2D
{
	[Export] private ColorRect _deathOverlay; 
	private Timer _timer;
	private AudioStreamPlayer2D _killSound;
	private AudioStreamPlayer2D _hitSound;
	private bool _isDying;
	private bool _isHitFeedbackPlaying;


	public override void _Ready()
	{
		_timer = GetNode<Timer>("Timer");
		_killSound = GetNodeOrNull<AudioStreamPlayer2D>("KillSound");
		_hitSound = GetNodeOrNull<AudioStreamPlayer2D>("HitSound");
		ColorRect sceneOverlay = GetNodeOrNull<ColorRect>("CanvasLayer/ColorRect");
		if (_deathOverlay == null && sceneOverlay != null)
		{
			_deathOverlay = sceneOverlay;
		}
		if (_deathOverlay != null) _deathOverlay.Visible = false;
	}

	private void _on_body_entered(Node2D body)
	{
		if (_isDying || body is not Player) return;

		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		bool isDead = gameManager?.TakeDamage(1) ?? true;
		if (!isDead)
		{
			_hitSound?.Play();
			_ = PlayHitFeedback();
			return;
		}

		StartDeathSequence();
	}

	private void _on_timer_timeout() 
	{
		Engine.TimeScale = 1.0f;
		GetNodeOrNull<GameManager>("/root/GameManager")?.ResetRun();
		GetTree().ReloadCurrentScene();
	}

	private async Task PlayHitFeedback()
	{
		if (_isHitFeedbackPlaying || _isDying)
		{
			return;
		}

		_isHitFeedbackPlaying = true;
		Engine.TimeScale = 0.5f;

		if (_deathOverlay != null)
		{
			_deathOverlay.Visible = true;
			_deathOverlay.Color = new Color(1, 0, 0, 0.55f);
			var tween = CreateTween();
			tween.TweenProperty(_deathOverlay, "color", new Color(1, 0, 0, 0), 0.12f)
				.SetTrans(Tween.TransitionType.Quint);
		}

		await ToSignal(GetTree().CreateTimer(0.08f), SceneTreeTimer.SignalName.Timeout);

		if (!_isDying)
		{
			Engine.TimeScale = 1.0f;
			if (_deathOverlay != null)
			{
				_deathOverlay.Visible = false;
			}
		}

		_isHitFeedbackPlaying = false;
	}

	private void StartDeathSequence()
	{
		_isDying = true;

		GD.Print("You Died!");
		_killSound?.Play();

		if (_deathOverlay != null)
		{
			_deathOverlay.Visible = true;
			var tween = CreateTween();
			_deathOverlay.Color = new Color(0, 0, 0, 0); // Start transparent
			tween.TweenProperty(_deathOverlay, "color", new Color(0, 0, 0, 1), 0.5f)
				.SetTrans(Tween.TransitionType.Quint);
		}

		Engine.TimeScale = 0.5f;
		_timer.Start();
	}
}
