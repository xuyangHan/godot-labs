# Enemies in a Scrolling Shooter – Spawning, Properties, Bullets, and Pickups

*Part 8 of the Godot game development series. In this post we focus on the enemy side of the Raiden-like shooter: multiple enemy types with different properties, automatic spawning with a difficulty ramp, enemies shooting downward, and collision logic that reduces health. We’ll also extend the same spawn pattern to health pickups and connect it back to `GameManager`.*

In the previous post we set up the shooter foundation and explained the infinite scrolling background.

Now we build the systems that create “pressure” in a shooter:

- enemies appear continuously (not hand-placed)
- different enemies behave differently, but share a base contract
- bullets move downwards and punish bad positioning
- health/pickups provide recovery and connect to the same run state as damage

The full project used in this tutorial is available in the [project repository](/games/raiden-like/), so you can follow along with the same scenes and scripts.

---

## 1. Enemy design goal: different behavior, shared structure

Beginner projects often start with one enemy scene.
Then you add a second enemy… and duplicate everything.

Instead, we aim for:

> Many enemy *variants* that share one readable base implementation.

In `raiden-like`, enemies are instances of different `.tscn` scenes (`plane_1`, `plane_2`, …), but they all use the same script base: `EnemyBase`.

That gives us a consistent place to put:

- movement rules
- health / damage handling
- bullet firing behavior
- death feedback and cleanup

---

## 2. `EnemyBase`: movement + shooting + death in one place

### 2.1 The properties that make “types”

If all enemy scenes share the same script, how do they feel different?

By using exported properties.

In `EnemyBase`, you can tune:

- movement mode (`Plane` vs `Tank`)
- move speed and horizontal drift
- max health
- fire rate + randomness
- bullet speed + spread

```12:170:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\enemies\EnemyBase.cs
public partial class EnemyBase : CharacterBody2D
{
	private static readonly PackedScene EnemyBulletScene = GD.Load<PackedScene>("res://scenes/enemy_bullet.tscn");

	public enum MovementKind { Plane, Tank }

	[Export] public MovementKind Movement { get; set; } = MovementKind.Plane;

	/// <summary>Along +X for tanks; drift for planes while <see cref="MoveSpeed"/> drives +Y.</summary>
	[Export] public float MoveSpeed { get; set; } = 72f;

	[Export] public float PlaneDriftX { get; set; } = 0f;

	[Export] public float TankGroundY { get; set; } = -20f;

	[Export] public int MaxHealth { get; set; } = 2;

	[ExportGroup("Weapon")]
	[Export] public float FireCooldownSeconds { get; set; } = 1.35f;
	[Export] public float FireCooldownJitter { get; set; } = 0.45f;

	/// <summary>Half-angle (radians) around <see cref="Vector2.Down"/> for random aim.</summary>
	[Export] public float FireSpreadHalfAngleRadians { get; set; } = 0.55f;

	[Export] public Vector2 BulletSpawnOffset { get; set; } = new Vector2(0f, 18f);
	[Export] public float BulletSpeed { get; set; } = 240f;
	// ...
}
```

That one design decision keeps things beginner-friendly:

- you can create new enemies mostly by tweaking inspector values
- you don’t need to fork a new script for every enemy variant

---

### 2.2 A simple movement model for a shooter

For early shooter prototypes, movement does not need pathfinding or steering.
It just needs to be consistent and readable.

In `_PhysicsProcess`, the enemy chooses movement based on `MovementKind`:

- planes move downward (with optional drift)
- tanks move horizontally but clamp their Y to a ground lane

```119:141:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\enemies\EnemyBase.cs
public override void _PhysicsProcess(double delta)
{
	switch (Movement)
	{
		case MovementKind.Plane:
			Velocity = new Vector2(PlaneDriftX, MoveSpeed);
			MoveAndSlide();
			break;
		case MovementKind.Tank:
			Velocity = new Vector2(MoveSpeed, 0f);
			MoveAndSlide();
			GlobalPosition = new Vector2(GlobalPosition.X, TankGroundY);
			break;
	}

	float dt = (float)delta;
	_fireCooldownRemaining -= dt;
	if (_fireCooldownRemaining <= 0f)
	{
		TryFireEnemyBullet();
		ResetFireCooldown();
	}
}
```

This is a good beginner baseline because it creates a clear gameplay loop:

- enemy enters from above
- enemy travels into the play space
- enemy occasionally fires
- enemy leaves or gets destroyed

---

### 2.3 Shooting downward: “always dangerous, sometimes unpredictable”

Most classic shooters are built around a simple idea:

> Enemy bullets mostly travel down, but not always straight down.

`EnemyBase` implements this by aiming around `Vector2.Down` with a randomized spread angle.

```143:169:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\enemies\EnemyBase.cs
private void TryFireEnemyBullet()
{
	if (EnemyBulletScene == null)
		return;

	Node world = GetParent();
	if (world == null)
		return;

	float spread = FireSpreadHalfAngleRadians;
	float angle = spread > 0f ? (float)GD.RandRange(-spread, spread) : 0f;
	Vector2 dir = Vector2.Down.Rotated(angle);

	var bullet = EnemyBulletScene.Instantiate<EnemyBullet>();
	bullet.Direction = dir;
	bullet.Speed = BulletSpeed;
	world.AddChild(bullet);
	bullet.GlobalPosition = GlobalPosition + BulletSpawnOffset;
}
```

Beginner takeaway:

- **`FireCooldownSeconds`** controls *how often* enemies shoot
- **`FireSpreadHalfAngleRadians`** controls *how avoidable* the bullets are
- **`BulletSpeed`** controls the player reaction time

Those three knobs alone can create many “difficulty feels”.

---

### 2.4 Collision: enemy bullets hit player health

The enemy bullet itself is a small script:

- it moves each physics frame in its assigned direction
- it damages the player if it overlaps the player node
- it despawns when it leaves the screen

```3:33:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\enemies\EnemyBullet.cs
public partial class EnemyBullet : Area2D
{
	[Export] public float Speed { get; set; } = 240f;
	[Export] public int Damage { get; set; } = 1;

	/// <summary>World-space travel direction (normalized automatically).</summary>
	public Vector2 Direction { get; set; } = Vector2.Down;

	public override void _Ready()
	{
		var notifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
		notifier.ScreenExited += QueueFree;
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Players player)
		{
			player.TakeDamage(Damage);
			QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 dir = Direction.LengthSquared() > 0.0001f ? Direction.Normalized() : Vector2.Down;
		GlobalPosition += dir * Speed * (float)delta;
	}
}
```

This is the clean “collision → gameplay” pattern from earlier posts, just adapted for shooter bullets.

---

## 3. Automatic spawning: enemies should not be hand-placed

If you place enemies manually in the editor:

- the stage never changes
- replayability is low
- “difficulty ramp” is hard to tune

So we spawn enemies automatically above the camera.

### 3.1 The spawn loop in one sentence

The spawner:

- counts down a timer
- instantiates a random enemy scene
- places it just above the visible top edge
- repeats faster over time

---

### 3.2 `EnemySpawner`: weighted random + exponential ramp

The enemy spawner uses three key ideas:

- **spawn interval shrinks over time** using an exponential curve
- **multiple enemy scenes** can be assigned in the inspector
- **weights** allow some enemies to appear more often than others

```4:144:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\enemies\EnemySpawner.cs
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

	private float ComputeInterval()
	{
		float t = BaseSpawnInterval * Mathf.Exp(-RampPerSecond * _elapsed);
		return Mathf.Clamp(t, MinSpawnInterval, BaseSpawnInterval);
	}

	// PickRandomScene() and ComputeSpawnPosition(...) omitted here for brevity.
}
```

Beginner-friendly tuning:

- if the game feels too slow early, reduce `BaseSpawnInterval`
- if it becomes unfair too quickly, reduce `RampPerSecond`
- if it becomes unfair eventually, increase `MinSpawnInterval`

---

### 3.3 Spawning above the camera (so enemies “enter” naturally)

One subtle detail: spawn position is computed from the active camera + viewport.

That keeps spawning robust if you change resolution or camera zoom, because the spawner is using *what the player can currently see*.

```125:142:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\enemies\EnemySpawner.cs
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
```

So enemies appear from just above the top edge, which looks natural in a vertical scroller.

---

## 4. Player damage: collision + cooldown + run state

There are two ways the player can take damage in this project:

- **enemy bullets** (handled by `EnemyBullet`)
- **touching enemies** (handled by the player’s hurtbox)

### 4.1 Why a “contact damage cooldown” matters

Without a cooldown, physics overlap can trigger many damage events in a fraction of a second.

That feels unfair and it’s confusing to debug.

So `Players` includes `EnemyContactCooldownSeconds` and a simple timer check:

```66:75:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\Players.cs
private void OnHurtboxBodyEntered(Node2D body)
{
	if (_isDying || body is not EnemyBase)
		return;
	if (_enemyContactCooldown > 0f)
		return;

	TakeDamage(EnemyContactDamage);
	_enemyContactCooldown = EnemyContactCooldownSeconds;
}
```

That one detail makes “touching an enemy” readable and consistent.

---

### 4.2 Damage ultimately routes through `GameManager`

The player does not own the “real” health value.

The player asks `GameManager` to reduce health, and the HUD reacts via the `HealthChanged` signal (from Part 7).

```77:94:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\Players.cs
public void TakeDamage(int amount)
{
	if (amount <= 0 || _isDying)
		return;
	if (_gameManager == null || _gameManager.Health <= 0)
		return;

	bool dead = _gameManager.TakeDamage(amount);

	if (!dead)
	{
		_hitSound?.Play();
		_ = PlayHitFeedbackAsync();
		return;
	}

	StartDeathSequence();
}
```

This keeps the same clean responsibility split:

- player handles movement + feedback
- `GameManager` owns health values and emits signals
- HUD displays values

---

## 5. Extending the spawn pattern: health pickups

Once you have an enemy spawner, it’s natural to ask:

> Can we spawn other entities the same way?

Yes — and this is a great beginner scalability milestone.

In this project, health packages spawn above the camera on a **separate, slower cadence** than enemies.

### 5.1 `HealthPackageSpawner`: same idea, different pacing

`HealthPackageSpawner` mirrors the enemy spawner:

- exponential ramp
- spawn above the top edge
- instantiate a scene and add it to the world

```3:82:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\HealthPackageSpawner.cs
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
}
```

Beginner design tip:

> Keep pickup spawns on a slower, independent timer so they feel like “events”, not noise.

---

### 5.2 `HealthPackage`: collision → `GameManager.AddHealth()` → HUD updates

The pickup falls downward and heals the player on contact.

The important part is the integration point:

> Pickups don’t update UI directly. They call `GameManager`, and the HUD reacts.

```4:80:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\HealthPackage.cs
public partial class HealthPackage : Area2D
{
	[Export] public int HealAmount { get; set; } = 1;
	[Export] public float FallSpeedPixels { get; set; } = 72f;

	// ...

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
}
```

This is the same scalable pattern from earlier posts, now applied to shooter pickups.

---

## 6. Concepts Covered

| Concept | Why it matters |
| --- | --- |
| Exported enemy properties | create many enemy types without copying scripts |
| Simple movement kinds | readable baseline behavior for beginners |
| Downward bullets with spread | adds pressure while staying fair and learnable |
| Auto spawners + ramp | creates replayability and controllable difficulty |
| Contact damage cooldown | prevents “jitter damage spam” |
| Spawner reuse for pickups | shows how systems can generalize beyond enemies |
| `GameManager` integration | keeps health/coins + HUD consistent across systems |

The full project used in this tutorial is available in the [project repository](/games/raiden-like/), so you can follow along with the same scenes and scripts.

---

### Next Steps (small, beginner-friendly extensions)

If you want easy follow-ups that fit this structure:

- add a second pickup type (shield or temporary fire-rate boost) using the same spawner pattern
- add enemy “wave” presets by temporarily increasing weights for specific enemy scenes
- add simple scoring and emit a `ScoreChanged` signal from `GameManager`

