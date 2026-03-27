# Enemies in a Scrolling Shooter – Spawning, Properties, Bullets, and Pickups

*Part 8 of the Godot game development series. In this post we focus on the enemy side of the Raiden-like shooter: multiple enemy types with different properties, automatic spawning with a difficulty ramp, enemies shooting downward, and collision logic that reduces health. We’ll also extend the same spawn pattern to health pickups and connect it back to `GameManager`.*

In the previous post we set up the shooter foundation and explained the infinite scrolling background.

Now we build the systems that create “pressure” in a shooter:

- enemies appear continuously (not hand-placed)
- different enemies behave differently, but share a base contract
- bullets move downwards and punish bad positioning
- health/pickups provide recovery and connect to the same run state as damage

The full project used in this tutorial is available in the [project repository](/games/raiden-like/), so you can follow along with the same scenes and scripts.

![game screenshot](assets/raiden-game.png)

---

## 1. Enemy Design Goal: Different Behavior, Same Structure

In beginner projects, it’s very common to start like this:

* create one enemy
* duplicate it to make a second one
* copy and tweak code

This works at first… but quickly becomes hard to manage.

Instead, we aim for a cleaner idea:

> Different enemies should *feel different*, but be built from the same structure.

In this project:

* each enemy is its own scene (`plane_1`, `plane_2`, etc.)
* but they all share the same script: `EnemyBase`

This gives us one place to handle:

* movement
* health and damage
* shooting
* death and cleanup

So instead of copying logic, we reuse it and just tweak values.

---

## 2. `EnemyBase`: One Script, Many Enemy Types

### 2.1 Use Properties to Create Variety

If all enemies share one script, how do they behave differently?

The answer is simple:

> We change values, not code.

In `EnemyBase`, we expose properties in the inspector:

* movement type (`Plane` or `Tank`)
* movement speed and drift
* health
* fire rate
* bullet speed and spread

```csharp
[Export] public MovementKind Movement { get; set; } = MovementKind.Plane;
[Export] public float MoveSpeed { get; set; } = 72f;
[Export] public float PlaneDriftX { get; set; } = 0f;
[Export] public int MaxHealth { get; set; } = 2;

[Export] public float FireCooldownSeconds { get; set; } = 1.35f;
[Export] public float FireSpreadHalfAngleRadians { get; set; } = 0.55f;
[Export] public float BulletSpeed { get; set; } = 240f;
```

This gives you a very beginner-friendly workflow:

* duplicate a scene
* tweak a few numbers
* instantly get a new enemy type

No new script needed.

---

### 2.2 Simple Movement That Feels Right

For a shooter, movement does not need to be complex.

It just needs to be:

* predictable
* readable
* consistent

In `_PhysicsProcess`, enemies move based on their type:

* **planes** move downward (with optional sideways drift)
* **tanks** move sideways but stay on a fixed “ground” line

```csharp
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
```

This creates a clear and simple loop:

* enemies enter the screen
* move in a predictable way
* create pressure
* leave or get destroyed

That’s all you need for a solid beginner shooter.

---

### 2.3 Shooting Downward (With a Bit of Chaos)

A core idea in many shooters is:

> Bullets mostly go down… but not perfectly straight.

This keeps gameplay fair but not boring.

In `EnemyBase`, bullets are aimed downward with a small random angle:

```csharp
float angle = (float)GD.RandRange(-spread, spread);
Vector2 dir = Vector2.Down.Rotated(angle);
```

Then we spawn and launch the bullet:

```csharp
bullet.Direction = dir;
bullet.Speed = BulletSpeed;
```

You can control difficulty using just a few values:

* **FireCooldownSeconds** → how often enemies shoot
* **FireSpread** → how accurate (or messy) shots are
* **BulletSpeed** → how much time the player has to react

These small tweaks make a big difference in how the game feels.

---

### 2.4 Collision: Turning Bullets into Damage

The enemy bullet is a small, focused script:

* it moves every frame
* it checks collision with the player
* it applies damage
* it removes itself

```csharp
private void OnBodyEntered(Node2D body)
{
    if (body is Players player)
    {
        player.TakeDamage(Damage);
        QueueFree();
    }
}
```

So the flow is very clear:

```text
bullet moves
    ↓
hits player
    ↓
player takes damage
    ↓
bullet disappears
```

This is the same idea you’ve used before:

> collision → gameplay effect

Just applied to a faster, more action-heavy game.

---

## 3. Automatic Spawning: Let the Game Create Pressure

If you place enemies manually in the editor:

* the level always plays the same
* replay value is low
* it’s hard to control difficulty over time

So instead, we let the game **spawn enemies continuously**.

> Enemies should *enter the screen*, not sit there waiting.

---

### 3.1 The Spawn Loop (Simple Mental Model)

The spawner follows a very simple loop:

* wait for a short delay
* pick a random enemy
* spawn it just above the screen
* repeat… a bit faster each time

That’s enough to create a constantly changing challenge.

---

### 3.2 `EnemySpawner`: Making Difficulty Increase Over Time

The spawner uses three simple ideas:

* **spawn faster over time**
* **choose from multiple enemy types**
* **control how often each type appears**

```csharp
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

What these mean in plain terms:

* **BaseSpawnInterval** → how slow the game starts
* **MinSpawnInterval** → the fastest it can ever get
* **RampPerSecond** → how quickly it speeds up

So the flow becomes:

```text
start slow
   ↓
spawn faster over time
   ↓
eventually stabilize at a fast pace
```

You don’t need to fully understand the math here—the important idea is:

> The game gets harder automatically as time passes.

---

### 3.3 Controlling Enemy Variety (Weighted Random)

Instead of always spawning the same enemy, we pick from a list:

```csharp
[Export] public Array<PackedScene> EnemyScenes { get; set; }
[Export] public Array<float> SpawnWeights { get; set; }
```


Weights let you control how often each enemy appears:

* higher weight → appears more often
* lower weight → appears less often

Example:

* basic enemy → weight 5
* stronger enemy → weight 2
* rare enemy → weight 1

This keeps the game interesting without adding complex logic.

---

### 3.4 Tuning Tips (What to Adjust First)

If something feels off, start here:

* game too slow at the start → lower **BaseSpawnInterval**
* difficulty ramps too quickly → lower **RampPerSecond**
* late game feels unfair → increase **MinSpawnInterval**

These three values give you a lot of control without touching code.

---

### 3.5 Spawning Just Above the Screen

One small but important detail:

> Enemies should appear *outside* the screen, then move in.

So we spawn them slightly above the visible area.

The position is calculated using:

* the camera position
* the current screen size

```csharp
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

This makes sure:

* enemies always enter from the top
* spawning still works if you change resolution or zoom

So visually, it feels like:

```text
enemy appears above
    ↓
moves into view
    ↓
interacts with player
```

That small detail makes the game feel much more natural.

---

## 4. Player damage: collision + cooldown + run state

There are two ways the player can take damage in this project:

- **enemy bullets** (handled by `EnemyBullet`)
- **touching enemies** (handled by the player’s hurtbox)

### 4.1 Why a “contact damage cooldown” matters

Without a cooldown, physics overlap can trigger many damage events in a fraction of a second.

That feels unfair and it’s confusing to debug.

So `Players` includes `EnemyContactCooldownSeconds` and a simple timer check:

``` csharp
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

```csharp
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

```csharp
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

```csharp
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

