# Weapon Systems in a 2D RPG – Collision, Damage, and Scalable Design

*Part 6 of the Godot game development series. In this post, we build a scalable weapon system for a top-down RPG and connect collision events to real combat outcomes (damage and enemy death).*

In the previous post, we set up the RPG foundation:

* tile-based world design
* top-down movement
* shared run state with `GameManager`

Now we move into combat.

The goal is not just “make attacks work.”
The real goal is:

> Make attacks work in a structure that is easy to extend later.

That means we want reusable weapon contracts, clean collision flow, and clear enemy damage handling.

The full project used in this tutorial is available in the [project repository](/games/2d-rpg/), so you can follow along with the same scenes and scripts.

---

## 1. From Collision Basics to Combat Interactions

In earlier posts, collision was mostly about:

* stopping the player from walking through walls  
* detecting simple triggers like coins  

In this RPG, collision becomes something more important:

> it decides when combat happens.

---

### From collision to gameplay

Instead of just blocking movement, collision now drives a full gameplay flow:

```text
attack starts
    ↓
hit area / projectile overlaps body
    ↓
target validates hit
    ↓
damage method called
    ↓
death effect / cleanup
```

---

### A simple way to think about it

> Combat is: **collision + rules + feedback**

* **collision** → detects that something was hit
* **rules** → decide what happens (damage, ignore, etc.)
* **feedback** → shows the result (animation, sound, enemy death)

Once you see combat this way, it becomes much easier to design and extend.

---

## 2. Base `Weapon` Contract (Why It Scales)

The base class in `scripts/weapons/Weapon.cs` is intentionally minimal:

```csharp
public virtual void SetEquipped(bool equipped) { ... }
public virtual void SetAimDirection(Vector2 worldDirection) { }
public virtual void TryPrimaryAttack() { }
```

This gives every weapon a shared interface:

* equip/unequip behavior
* aim update hook
* primary attack trigger

When all weapons share this contract, player code does not need to know whether the active weapon is melee or ranged.

That is the key scalability win.

---

## 3. Weapon Pivot: One Controller, Many Weapon Types

`PlayerWeaponPivot.cs` coordinates loadout, aim, and attack input:

* builds a list of child `Weapon` nodes
* cycles loadout slots
* computes mouse-based aim direction
* calls `active.TryPrimaryAttack()` when attack input is pressed

Here is a real excerpt from `PlayerWeaponPivot.cs`:

```csharp
public override void _Process(double delta)
{
    // 1) Handle one-press weapon cycling (Tab or mapped action).
    bool tabPhysical = Input.IsPhysicalKeyPressed(Key.Tab);
    bool tabPhysicalEdge = tabPhysical && !_tabPhysicalPrev;
    _tabPhysicalPrev = tabPhysical;

    if (tabPhysicalEdge || Input.IsActionJustPressed("weapon_cycle"))
        CycleWeapon();

    var active = ActiveWeapon;
    if (active != null)
    {
        // 2) Aim from pivot -> mouse world position.
        Vector2 mouse = GetGlobalMousePosition();
        Vector2 raw = mouse - GlobalPosition;
        if (raw.LengthSquared() > 0.0001f)
        {
            Vector2 dir = raw.Normalized();

            // 3) Clamp vertical aim so weapons do not over-rotate.
            float maxVert = Mathf.Sin(Mathf.DegToRad(MaxAimElevationFromHorizontalDegrees));
            dir.Y = Mathf.Clamp(dir.Y, -maxVert, maxVert);
            dir = dir.LengthSquared() > 1e-6f ? dir.Normalized() : new Vector2(1f, 0f);

            float angle = dir.Angle() + active.AimOffsetRadians;
            Rotation = angle;

            // 4) Flip pivot when aiming left to keep sprites oriented correctly.
            bool aimLeft = mouse.X < GlobalPosition.X;
            Scale = aimLeft ? new Vector2(1f, -1f) : new Vector2(1f, 1f);

            // 5) Pass the final world direction to the active weapon.
            active.SetAimDirection(dir);
        }

        // 6) Fire through the weapon interface (melee/ranged handled internally).
        if (Input.IsActionJustPressed("attack"))
            active.TryPrimaryAttack();
    }
}
```

This creates a clean separation:

* pivot handles player input and aim math
* each weapon handles its own attack behavior

So adding a new weapon usually means:

1. inherit from `Weapon`
2. implement attack logic
3. add it under the pivot

No rewrite of player movement logic is required.

---

## 4. Melee Example: `WeaponSword` Hit Window

`WeaponSword.cs` uses an `Area2D` `HitArea`, but only enables monitoring during an animation hit window:

```csharp
double t = _animationPlayer.CurrentAnimationPosition;
bool inHit = t >= HitWindowStart && t < HitWindowEnd;
if (inHit != _hitArea.Monitoring)
    _hitArea.Monitoring = inHit;
```

This is a strong beginner pattern because it keeps melee timing explicit:

* attack animation starts
* hitbox becomes active for a short window
* hitbox turns off again

To avoid multi-hit spam from one swing, the script also tracks hit targets with `_hitThisSwing`.

That one detail makes melee behavior feel much more reliable.

---

## 5. Ranged Example: `WeaponGun` + `Bullet`

`WeaponGun.cs` handles cooldown, spawn position, and initial bullet velocity.
`Bullet.cs` handles movement and `BodyEntered` reactions.

Combat flow for a shot:

1. gun validates cooldown
2. bullet instance is spawned into the world
3. bullet moves each frame
4. bullet collides with enemy/world and resolves hit

Real project excerpt (`WeaponGun.cs`):

```csharp
public override void TryPrimaryAttack()
{
    // 1) Block firing during cooldown or if no bullet scene is assigned.
    if (_cooldownLeft > 0 || BulletScene == null)
        return;

    // 2) Spawn bullet into the world (not as a child of the weapon node).
    var world = GetParent()?.GetParent()?.GetParent() ?? GetTree().CurrentScene;
    if (world == null)
        return;

    _animationPlayer?.Play("shoot");

    var bulletNode = BulletScene.Instantiate<Node2D>();
    world.AddChild(bulletNode);

    // 3) Spawn near muzzle and push forward along current aim direction.
    Vector2 spawn = GetNode<Node2D>("Sprite2D").GlobalPosition;
    spawn += _aimWorld * MuzzleLeadPixels;
    bulletNode.GlobalPosition = spawn;

    // 4) Pass starting velocity to bullet script.
    if (bulletNode is Bullet bullet)
        bullet.Velocity = _aimWorld * bullet.Speed;

    // 5) Start cooldown timer.
    _cooldownLeft = FireCooldownSeconds;
}
```

In `Bullet.cs`, the `OnBodyEntered` method applies the same damage path as melee:

* direct typed path for `Slime`
* generic group/method path for other enemy types
* world/tile collision frees the bullet

Real project excerpt (`Bullet.cs`):

```csharp
public override void _Process(double delta)
{
    // Move projectile every frame using assigned velocity.
    GlobalPosition += Velocity * (float)delta;
}

private void OnBodyEntered(Node2D body)
{
    if (body == null || !IsInstanceValid(this))
        return;
    if (body.IsInGroup("player"))
        return; // Ignore player to avoid self-hit.

    // Enemy hit: apply damage and remove projectile.
    if (body is Slime slime)
    {
        slime.TakeDamage(1, this);
        QueueFree();
        return;
    }

    // Generic support for any enemy implementing TakeDamage.
    if (body.IsInGroup("enemies") && body.HasMethod("TakeDamage"))
    {
        body.Call("TakeDamage", 1, this);
        QueueFree();
        return;
    }

    // World collision: bullet is consumed.
    if (body is TileMapLayer)
    {
        QueueFree();
        return;
    }
}
```

This gives immediate projectile gameplay while keeping future enemy support flexible.

---

## 6. Collision to Kill Effect

Both sword and bullets converge into enemy `TakeDamage`.

In `Slime.cs`, when damage is received:

* enemy is marked as dying
* enemy group membership and collisions are disabled
* a small flash + fade tween plays
* `QueueFree()` runs at the end

Real project excerpt (`Slime.cs`):

```csharp
public void TakeDamage(int amount, Node2D source = null)
{
    // 1) Prevent duplicate kill logic from repeated hits.
    if (_dying)
        return;
    _dying = true;

    // 2) Remove enemy from combat systems immediately.
    RemoveFromGroup("enemies");
    SetPhysicsProcess(false);
    CollisionLayer = 0;
    CollisionMask = 0;

    // 3) Disable kill trigger so nothing else can interact during death.
    var killZone = GetNodeOrNull<Area2D>("KillZone");
    if (killZone != null)
    {
        killZone.SetDeferred(Area2D.PropertyName.Monitoring, false);
        killZone.SetDeferred(Area2D.PropertyName.Monitorable, false);
    }

    // 4) Play death feedback (flash -> return -> fade out), then free node.
    Color baseModulate = _animatedSprite.Modulate;
    var tween = CreateTween();
    tween.TweenProperty(_animatedSprite, "modulate", new Color(3f, 3f, 3f, baseModulate.A), DeathFlashInDuration);
    tween.TweenProperty(_animatedSprite, "modulate", baseModulate, DeathFlashOutDuration);
    tween.TweenProperty(_animatedSprite, "modulate", new Color(baseModulate.R, baseModulate.G, baseModulate.B, 0f), DeathFadeDuration);
    tween.TweenCallback(Callable.From(QueueFree));
}
```

So “kill effect” is not a single line. It is a controlled sequence:

```text
valid hit
  ↓
TakeDamage
  ↓
disable further interactions
  ↓
play death feedback
  ↓
remove node safely
```

This avoids common issues like double-kill calls or repeated hit events while dying.

---

## 7. How This Connects Back to Game State

From Post 5, we already have a shared state layer in `GameManager`.
Combat systems can now plug into that same architecture:

* enemy kill rewards can call manager methods
* player damage can route through `TakeDamage`
* HUD updates continue via signals, without tight coupling

So your project naturally evolves into:

```text
Input + Weapons + Collision
            ↓
Damage / Gameplay events
            ↓
GameManager state updates
            ↓
HUD and feedback systems react
```

This is exactly how small game prototypes stay manageable while gaining features.

---

## 8. Scaling Ideas (Next Steps)

With this structure, it is straightforward to extend:

* **Weapon rotation tuning**: adjust aim constraints and offsets per weapon
* **New weapon families**: add `WeaponSpear`, `WeaponBow`, `WeaponMagic` from the same base class
* **Damage model**: add per-weapon damage, crit chance, status effects
* **Enemy reactions**: knockback, stagger, armor, resistances
* **Shared interfaces**: move toward a consistent “damageable” contract for all enemies

You can add these incrementally without replacing the architecture.

---

## 9. Concepts Covered

| Concept | Why it matters |
| --- | --- |
| Base `Weapon` contract | Keeps weapon switching and input integration consistent |
| Melee hit windows | Makes close-range attacks predictable and fair |
| Projectile pipeline | Separates gun fire logic from bullet collision logic |
| Collision-to-damage flow | Turns overlap events into gameplay outcomes cleanly |
| Controlled death sequence | Prevents duplicate hits and improves visual feedback |
| Extensible structure | Supports adding many weapon/enemy types over time |

The full project used in this tutorial is available in the [project repository](/games/2d-rpg/), so you can inspect the same scenes and scripts.

---

### Series Direction

At this point, the series has moved from:

* turn-based board logic (chess)
* real-time movement loop (platformer)
* combat-oriented systems (top-down RPG)

The same core principles keep showing up:

* clear responsibilities
* reusable scenes/classes
* event-driven communication

That consistency is what helps beginners move from one genre to another without starting from zero each time.
