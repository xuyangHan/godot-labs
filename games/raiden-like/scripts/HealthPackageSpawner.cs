using Godot;

/// <summary>Spawns health pickups above the camera on a separate, slower cadence than enemies.</summary>
public partial class HealthPackageSpawner : Node
{
	private static readonly PackedScene DefaultPackageScene = GD.Load<PackedScene>("res://scenes/health_package.tscn");

	[ExportGroup("Timing")]
	[Export] public float BaseSpawnInterval { get; set; } = 14f;

	[Export] public float MinSpawnInterval { get; set; } = 7f;

	[Export] public float RampPerSecond { get; set; } = 0.02f;

	[ExportGroup("Placement")]
	[Export] public float SpawnMarginPixels { get; set; } = 40f;

	[ExportGroup("Pickup")]
	[Export] public PackedScene HealthPackageScene { get; set; }

	private float _elapsed;
	private float _timeToNext;

	public override void _Ready()
	{
		if (HealthPackageScene == null)
			HealthPackageScene = DefaultPackageScene;

		_timeToNext = BaseSpawnInterval * 0.5f;
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		_elapsed += dt;
		_timeToNext -= dt;

		if (_timeToNext > 0f)
			return;

		TrySpawn();
		_timeToNext = ComputeInterval();
	}

	private float ComputeInterval()
	{
		float t = BaseSpawnInterval * Mathf.Exp(-RampPerSecond * _elapsed);
		return Mathf.Clamp(t, MinSpawnInterval, BaseSpawnInterval);
	}

	private void TrySpawn()
	{
		Viewport viewport = GetViewport();
		Camera2D cam = viewport.GetCamera2D();
		Node world = GetParent();
		if (cam == null || world == null || HealthPackageScene == null)
			return;

		var package = HealthPackageScene.Instantiate<Node2D>();
		world.AddChild(package);
		package.GlobalPosition = ComputeSpawnPosition(cam, viewport);
	}

	private Vector2 ComputeSpawnPosition(Camera2D cam, Viewport viewport)
	{
		Vector2 vsize = viewport.GetVisibleRect().Size;
		Vector2 half = vsize / (2f * cam.Zoom);
		Vector2 center = cam.GetScreenCenterPosition();

		float top = center.Y - half.Y;
		float spawnY = top - SpawnMarginPixels;

		float left = center.X - half.X;
		float right = center.X + half.X;
		left = Mathf.Max(left, cam.LimitLeft + SpawnMarginPixels);
		right = Mathf.Min(right, cam.LimitRight - SpawnMarginPixels);

		float spawnX = (float)GD.RandRange((double)left, (double)right);
		spawnX = Mathf.Clamp(spawnX, left, right);
		return new Vector2(spawnX, spawnY);
	}
}
