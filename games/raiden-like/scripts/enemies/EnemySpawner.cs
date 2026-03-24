using Godot;
using Godot.Collections;

/// <summary>Spawns weighted random enemies just above the visible camera area; intervals shorten over time.</summary>
public partial class EnemySpawner : Node
{
	[ExportGroup("Difficulty")]
	[Export] public float BaseSpawnInterval { get; set; } = 2.1f;

	[Export] public float MinSpawnInterval { get; set; } = 0.32f;

	/// <summary>Larger values make waves tighten faster (per second of play).</summary>
	[Export] public float RampPerSecond { get; set; } = 0.085f;

	[ExportGroup("Placement")]
	[Export] public float SpawnMarginPixels { get; set; } = 40f;

	[ExportGroup("Entries")]
	[Export] public Array<PackedScene> EnemyScenes { get; set; } = new();

	/// <summary>Parallel to <see cref="EnemyScenes"/>; missing entries default to weight 1.</summary>
	[Export] public Array<float> SpawnWeights { get; set; } = new();

	private float _elapsed;
	private float _timeToNext;

	public override void _Ready()
	{
		FillDefaultEnemiesIfEmpty();
		_timeToNext = BaseSpawnInterval * 0.35f;
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

	private void FillDefaultEnemiesIfEmpty()
	{
		if (EnemyScenes.Count > 0)
			return;

		string[] paths =
		[
			"res://scenes/enemies/plane_1.tscn",
			"res://scenes/enemies/plane_2.tscn",
			"res://scenes/enemies/plane_3.tscn",
			"res://scenes/enemies/plane_4.tscn",
			// Tank spawn paused — re-add `tank.tscn` here or assign scenes in the inspector.
		];

		foreach (string path in paths)
		{
			PackedScene scene = GD.Load<PackedScene>(path);
			if (scene != null)
				EnemyScenes.Add(scene);
		}
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
		if (cam == null || world == null)
			return;

		PackedScene scene = PickRandomScene();
		if (scene == null)
			return;

		Node2D enemy = scene.Instantiate<Node2D>();
		world.AddChild(enemy);
		enemy.GlobalPosition = ComputeSpawnPosition(cam, viewport);
	}

	private PackedScene PickRandomScene()
	{
		if (EnemyScenes.Count == 0)
			return null;

		if (EnemyScenes.Count == 1)
			return EnemyScenes[0];

		float totalWeight = 0f;
		for (int i = 0; i < EnemyScenes.Count; i++)
		{
			float w = i < SpawnWeights.Count ? SpawnWeights[i] : 1f;
			totalWeight += w < 0f ? 0f : w;
		}

		if (totalWeight <= 0f)
			return EnemyScenes[(int)(GD.Randi() % EnemyScenes.Count)];

		float r = GD.Randf() * totalWeight;
		float acc = 0f;

		for (int i = 0; i < EnemyScenes.Count; i++)
		{
			float w = i < SpawnWeights.Count ? SpawnWeights[i] : 1f;
			if (w < 0f)
				w = 0f;
			acc += w;
			if (r <= acc)
				return EnemyScenes[i];
		}

		return EnemyScenes[^1];
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
