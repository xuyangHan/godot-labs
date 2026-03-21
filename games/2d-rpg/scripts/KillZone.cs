using Godot;
using System;

public partial class KillZone : Area2D
{
	[Export] private ColorRect _deathOverlay; 
	private Timer _timer;


	public override void _Ready()
	{
		_timer = GetNode<Timer>("Timer");
		ColorRect sceneOverlay = GetNodeOrNull<ColorRect>("CanvasLayer/ColorRect");
		if (_deathOverlay == null && sceneOverlay != null)
		{
			_deathOverlay = sceneOverlay;
		}
		if (_deathOverlay != null) _deathOverlay.Visible = false;
	}

	private void _on_body_entered(Node2D body)
	{
		GD.Print("You Died!");
		
		var collisionShape = body.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (collisionShape != null)
		{
			collisionShape.QueueFree();
		}

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

	private void _on_timer_timeout() 
	{
		Engine.TimeScale = 1.0f;
		GetTree().ReloadCurrentScene();
	}
}
