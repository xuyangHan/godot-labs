# From RPG to Scrolling Shooter – Reuse, Game State, and Infinite Background

*Part 7 of the Godot game development series. In this post we start our 4th game: a Raiden-like scrolling shooter. We’ll recap the reusable building blocks from earlier games (tiles, `GameManager`, HUD, weapons), then zoom in on the new core problem: making the world move by scrolling the background.*

In the previous posts we built:

- a turn-based chess game (clean structure + signals)
- a beginner platformer (physics loop + collisions)
- a top-down RPG (tiles + shared state + scalable combat)

Now we switch genres again: a **vertical scrolling shooter**.

At first glance it looks very different from a platformer or an RPG, but the *architecture* can stay very similar:

- we still reuse scenes like building blocks
- we still keep shared run state in one place
- we still rely on collision + signals to turn overlaps into gameplay

The big new ingredient is this:

> The player stays roughly centered… and the world moves past them.

The full project used in this tutorial is available in the [project repository](/games/raiden-like/), so you can follow along with the same scenes and scripts.

---

## 1. Similarities: the reusable “backbone” shows up again

Even though this is a new genre, you can reuse the same mental model from the platformer and RPG posts.

### 1.1 Tiles are still the fastest way to build a world

Just like the platformer and RPG, the shooter uses tiles to create a consistent look quickly.

The difference is *how the tiles are used*:

- platformer tiles define jump paths and solid platforms
- RPG tiles define walkable vs blocked space and guide navigation
- shooter tiles are mostly **visual pattern + readability** (what the player can see while moving fast)

So the tool is the same (`TileMapLayer`), but the design goal changes.

---

### 1.2 `GameManager` + signals is still the cleanest run-state pattern

In the RPG we introduced a shared `GameManager` that owns run state (coins, health), and the UI simply listens.

That same structure works perfectly for a shooter:

```text
Enemy bullet hits player / Health pickup collected / Coin collected
                    ↓
            GameManager updates state
                    ↓
               signals emitted
                    ↓
                HUD updates
```

In `raiden-like`, `GameManager` is intentionally small: it owns `Coins`, `Health`, and a configurable `MaxHealth`, and it emits signals whenever values change.

```3:65:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\GameManager.cs
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
```

This is a beginner-friendly pattern because the project has **one source of truth** for important values.

---

### 1.3 HUD stays decoupled by subscribing to signals

The HUD does not chase game objects to “pull” values. It simply listens to `GameManager` and updates text.

```3:52:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\HUD.cs
public partial class HUD : CanvasLayer
{
	private Label _coinLabel;
	private Label _healthLabel;

	public override void _Ready()
	{
		_coinLabel = GetNodeOrNull<Label>("MarginContainer/Stats/CoinLabel");
		_healthLabel = GetNode<Label>("MarginContainer/Stats/HealthLabel");

		GameManager gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			if (_coinLabel != null)
				gameManager.CoinsChanged += OnCoinsChanged;
			gameManager.HealthChanged += OnHealthChanged;
			if (_coinLabel != null)
				OnCoinsChanged(gameManager.Coins);
			OnHealthChanged(gameManager.Health);
		}
		else
		{
			if (_coinLabel != null)
				_coinLabel.Text = "Coins: 0";
			_healthLabel.Text = "Health: 3";
		}
	}

	public override void _ExitTree()
	{
		GameManager gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			if (_coinLabel != null)
				gameManager.CoinsChanged -= OnCoinsChanged;
			gameManager.HealthChanged -= OnHealthChanged;
		}
	}

	private void OnCoinsChanged(int newValue)
	{
		if (_coinLabel != null)
			_coinLabel.Text = $"Coins: {newValue}";
	}

	private void OnHealthChanged(int newValue)
	{
		_healthLabel.Text = $"Health: {newValue}";
	}
}
```

Same idea as the RPG posts, just applied in a new genre.

---

## 2. Difference: platformer vs RPG vs scrolling shooter

Let’s summarize the key “feel” difference, because it affects how you build systems.

### 2.1 Platformer: gravity dominates the loop

Platformer loop (simplified):

```text
apply gravity → read jump input → MoveAndSlide → collisions
```

The world is mostly static, and the player traverses it.

---

### 2.2 RPG: navigation + interactions dominate

RPG loop (simplified):

```text
read input → MoveAndSlide → check interactions (coins, enemies, weapons)
```

No gravity, but the player explores a space with many interaction points.

---

### 2.3 Scrolling shooter: forward motion dominates the readability problem

Shooter loop (simplified):

```text
player moves inside bounds + shoots
enemies spawn above → move down → shoot down
background scrolls continuously to sell motion
```

Even if the player’s movement script is small, the **perception of motion** is a major part of the game.

Which leads to our main system for this post: the scrolling background.

---

## 3. Scrolling the world: looping a tile pattern cleanly

The simplest way to create “infinite scrolling” is:

1. create a tile pattern that fills the screen
2. scroll it downward every frame
3. when it moves out of view, wrap it back above

But there’s one problem:

> If you scroll only one tilemap, you get empty space when it leaves the screen.

So we use two copies of the same tilemap pattern.

### 3.1 The “two layers” mental model

Think of it like this:

```text
[Pattern B]  ← starts above the screen
[Pattern A]  ← starts on the screen

Both scroll down.
When A goes fully below, it jumps back above B (and vice versa).
```

This gives the illusion of an endless world.

---

### 3.2 `BackgroundScroller` in code

In this project, the scrolling logic is implemented in a `TileMapLayer` script so it stays self-contained.

Core ideas in the script:

- compute the pattern height in pixels using `GetUsedRect()` and `TileSet.TileSize`
- create a second `TileMapLayer` at runtime
- copy tile data into it
- move both layers downward
- wrap positions when they pass the pattern height

Here is the full implementation:

```3:82:C:\Users\62707\OneDrive\文档\Repos\godot-labs\games\raiden-like\scripts\BackgroundScroller.cs
public partial class BackgroundScroller : TileMapLayer
{
	[Export]
	public float ScrollSpeed = 80.0f;

	private float _patternHeightPixels;
	private TileMapLayer _secondaryLayer;
	private float _primaryY;
	private float _secondaryY;
	private bool _isInitialized;

	public override void _Ready()
	{
		Rect2I usedRect = GetUsedRect();
		Vector2I usedRectSize = usedRect.Size;
		Vector2I tileSize = TileSet.TileSize;
		_patternHeightPixels = usedRectSize.Y * tileSize.Y;

		if (_patternHeightPixels <= 0.0f)
		{
			GD.PushWarning($"{Name}: Background pattern height is zero. Scrolling disabled.");
			return;
		}

		CallDeferred(nameof(SetupSecondaryLayerDeferred));
	}

	private void SetupSecondaryLayerDeferred()
	{
		_secondaryLayer = new TileMapLayer();
		_secondaryLayer.Name = $"{Name}_loop_copy";
		_secondaryLayer.Set("tile_set", Get("tile_set"));
		_secondaryLayer.Set("tile_map_data", Get("tile_map_data"));
		_secondaryLayer.Set("modulate", Get("modulate"));

		Node parent = GetParent();
		if (parent == null)
		{
			return;
		}

		parent.AddChild(_secondaryLayer);
		_secondaryLayer.Owner = Owner;
		_secondaryLayer.ZIndex = ZIndex - 1;

		_primaryY = 0.0f;
		_secondaryY = -_patternHeightPixels;
		Position = new Vector2(Position.X, _primaryY);
		_secondaryLayer.Position = new Vector2(_secondaryLayer.Position.X, _secondaryY);
		_isInitialized = true;
	}

	public override void _Process(double delta)
	{
		if (_patternHeightPixels <= 0.0f)
		{
			return;
		}
		if (!_isInitialized || _secondaryLayer == null)
		{
			return;
		}

		float deltaY = ScrollSpeed * (float)delta;
		_primaryY += deltaY;
		_secondaryY += deltaY;

		if (_primaryY >= _patternHeightPixels)
		{
			_primaryY -= _patternHeightPixels * 2.0f;
		}
		if (_secondaryY >= _patternHeightPixels)
		{
			_secondaryY -= _patternHeightPixels * 2.0f;
		}

		Position = new Vector2(Position.X, _primaryY);
		_secondaryLayer.Position = new Vector2(_secondaryLayer.Position.X, _secondaryY);
	}
}
```

---

### 3.3 Why `CallDeferred` is used here

We create and add `_secondaryLayer` using `CallDeferred`.

This avoids subtle timing issues where the node tree isn’t ready for re-parenting or ownership changes at the exact moment `_Ready()` runs.

Beginner rule of thumb:

> If you create nodes and immediately add them to the scene tree during `_Ready()`, and you hit strange ordering issues, try `CallDeferred()`.

---

### 3.4 Why this is a good beginner solution

There are many ways to do scrolling backgrounds (parallax, textures, shaders). This tilemap approach is a solid early step because:

- it’s easy to inspect and debug (just watch two layers move)
- it uses the same tile workflow you already learned
- it scales to adding details (variation tiles, decorations, multiple layers)

Once you understand *this* loop, you can later replace it with more advanced approaches without changing the rest of the architecture.

---

## 4. Concepts Covered

| Concept | Why it matters in a scrolling shooter |
| --- | --- |
| Tiles reused across genres | same tool, different design goals |
| Shared `GameManager` state | keeps coins/health consistent across gameplay systems |
| HUD via signals | avoids “gameplay scripts update labels” coupling |
| Two-layer background loop | simplest infinite scroll that doesn’t show gaps |
| `CallDeferred` for setup | safer scene-tree modifications during initialization |

The full project used in this tutorial is available in the [project repository](/games/raiden-like/), so you can follow along with the same scenes and scripts.

---

### Next Post

In the next post we’ll focus on the “active” part of the shooter loop:

- enemy types (different properties, different movement)
- auto spawning and difficulty ramp
- bullets that shoot downward (and spread)
- collision and health logic (including pickups that plug into `GameManager`)

