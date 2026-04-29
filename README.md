# Game Development Journey with Godot

This repository documents my journey learning **game development with Godot** from a software engineering perspective.

As a backend developer, I wanted to explore how interactive systems, game loops, and simulation architectures differ from traditional application development.

The goal of this project is to:

* Learn the fundamentals of **game development architecture**
* Build **small playable prototypes**
* Share the learning process through **technical blog posts**
* Eventually develop a **real game**

---

## Blog Series

Alongside development, I write a series of technical articles about **game system design**. 

### Posts

1. **[Introduction to Godot](blogs/01-introduction-to-godot.md)** – Project setup, scenes, nodes, `_Ready()`, `GetNode`, and how the main script ties everything together.

2. **[From game logic to the screen](blogs/02-logic-to-screen-signals-scenes-input.md)** – Separating Board/Piece from UI, PackedScene/Instantiate, signals, input (`_GuiInput`), and optional threading (`CallDeferred`).

3. **[From Input to Playable Loop](blogs/03-2d-platform-core-loop-godot-csharp.md)** – Building a beginner 2D platformer loop in Godot C#: movement, jumping, animation states, coin pickup, death flow, and restart.

4. **[Collision, Reusability, and Polish](blogs/04-2d-platform-collisions-reusability-polish.md)** – Collision layers/masks, reusable scenes, patrol AI with `RayCast2D`, animation-driven gameplay events, and beginner-safe polish.

5. **[From Platformer to RPG](blogs/05-2d-rpg-tiles-and-game-manager.md)** – Transitioning to top-down RPG design, tile workflow recap (without gravity), and `GameManager` architecture for health/coins with signal-based HUD updates.

6. **[Weapon Systems in a 2D RPG](blogs/06-2d-rpg-weapons-collision-and-scalability.md)** – Scalable weapon architecture (`Weapon` base + melee/ranged implementations), weapon pivot aiming/cycling, and collision-to-damage-to-kill flow.

7. **[From RPG to Scrolling Shooter](blogs/07-raiden-like-scrolling-shooter-reuse-and-scrolling.md)** – Reusing tiles + `GameManager` + HUD patterns in a new genre, and implementing an infinite scrolling background with a looping `TileMapLayer`.

8. **[Enemies in a Scrolling Shooter](blogs/08-raiden-like-enemies-spawning-bullets-and-pickups.md)** – Enemy variants via exported properties, auto-spawning with a difficulty ramp, downward bullets, player damage flow, and pickup spawning connected back to `GameManager`.

---

Topics for future posts may include:

* Designing a turn-based combat system
* Architecture of farming simulations
* Managing game state and persistence
* Real-time game loops vs request-response systems
* Integrating real-world data into gameplay

These posts aim to bridge **traditional software engineering and game development thinking**.

---

## Why This Project Exists

Many programming resources explain **how to use a game engine**, but fewer focus on **how games are designed as systems**.

This repository explores questions such as:

* How does a **turn-based combat system** work?
* How do games manage **world state and progression**?
* What architecture supports **real-time gameplay**?
* How are **simulation-heavy games** like farming or city builders structured?

The goal is to approach game development the same way engineers approach **system design**.

---

## Planned Tech Stack

* **Godot Engine 4 (.NET)**
* **C#**
* Tile-based 2D gameplay
* Mobile deployment (iOS)

Possible integrations later:

* Apple HealthKit
* Native iOS plugins
* Cloud save or backend services

