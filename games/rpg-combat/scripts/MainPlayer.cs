using Godot;

/// <summary>
/// WASD + arrow movement for Main playground scene (CharacterBody2D).
/// </summary>
public partial class MainPlayer : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 280f;

	/// <summary>
	/// World-space rectangle (same parent as this body) the character may not leave.
	/// Default matches Main.tscn background TextureRect size.
	/// </summary>
	[Export]
	public Rect2 PlayArea { get; set; } = new(0f, 0f, 1303f, 866f);

	[Export]
	public bool ClampInsidePlayArea { get; set; } = true;

	private Vector2 _halfExtents = new(36f, 54f);

	public override void _Ready()
	{
		var cs = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (cs?.Shape is RectangleShape2D rect)
			_halfExtents = rect.Size * 0.5f * cs.Scale;
	}

	public override void _PhysicsProcess(double delta)
	{
		float x = (IsMoveRight() ? 1f : 0f) - (IsMoveLeft() ? 1f : 0f);
		float y = (IsMoveDown() ? 1f : 0f) - (IsMoveUp() ? 1f : 0f);
		var dir = new Vector2(x, y);
		if (dir.LengthSquared() > 0f)
			dir = dir.Normalized();

		Velocity = dir * Speed;
		MoveAndSlide();

		if (ClampInsidePlayArea)
			ClampToPlayArea();
	}

	private void ClampToPlayArea()
	{
		float minX = PlayArea.Position.X + _halfExtents.X;
		float maxX = PlayArea.Position.X + PlayArea.Size.X - _halfExtents.X;
		float minY = PlayArea.Position.Y + _halfExtents.Y;
		float maxY = PlayArea.Position.Y + PlayArea.Size.Y - _halfExtents.Y;

		if (minX > maxX || minY > maxY)
			return;

		Position = new Vector2(
			Mathf.Clamp(Position.X, minX, maxX),
			Mathf.Clamp(Position.Y, minY, maxY));
	}

	private static bool IsMoveLeft() =>
		Input.IsPhysicalKeyPressed(Key.A) || Input.IsActionPressed("ui_left");

	private static bool IsMoveRight() =>
		Input.IsPhysicalKeyPressed(Key.D) || Input.IsActionPressed("ui_right");

	private static bool IsMoveUp() =>
		Input.IsPhysicalKeyPressed(Key.W) || Input.IsActionPressed("ui_up");

	private static bool IsMoveDown() =>
		Input.IsPhysicalKeyPressed(Key.S) || Input.IsActionPressed("ui_down");
}
