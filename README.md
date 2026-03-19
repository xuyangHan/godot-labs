# Game Development Journey with Godot

This repository documents my journey learning **game development with Godot** from a software engineering perspective.

As a backend developer, I wanted to explore how interactive systems, game loops, and simulation architectures differ from traditional application development.

The goal of this project is to:

* Learn the fundamentals of **game development architecture**
* Build **small playable prototypes**
* Share the learning process through **technical blog posts**
* Eventually develop a **mobile farming game that connects gameplay with real-world activity**

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

## Learning Roadmap

The project will progress through several stages.

### Stage 1 — Godot Fundamentals

Learn the core concepts of the engine:

* Nodes and Scenes
* Scene Tree architecture
* Signals (event system)
* Input handling
* Game loops
* Basic UI systems

Small experiments will be added to the repository as prototypes.

---

### Stage 2 — Game System Prototypes

To understand how different genres work internally, I plan to build small prototypes for various game types.

Examples include:

* Turn-based battle system (inspired by Pokémon)
* Puzzle mechanics
* Idle / incremental systems
* Farming simulation mechanics
* Simple real-time combat

Each prototype focuses on **a specific gameplay architecture**.

---

### Stage 3 — Mobile Farming Game

The long-term goal is to build a small **mobile farming simulation game**.

Inspirations include:

* Stardew Valley
* Animal Crossing

Core idea:

Players grow crops and expand their farm, but **energy or currency is generated from real-world activity** such as walking or exercising.

This data may be connected through **Apple Health** integration.

The design aims to:

* Encourage healthy habits
* Prevent excessive playtime
* Create a relaxing daily gameplay loop

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

---

## Blog Series

Alongside development, I write a series of technical articles about **game system design**. Drafts for the fundamentals phase use the **chess game** as the teaching project.

### Posts

1. **[Introduction to Godot](blogs/01-introduction-to-godot.md)** – Project setup, scenes, nodes, `_Ready()`, `GetNode`, and how the main script ties everything together.

2. **[From game logic to the screen](blogs/02-logic-to-screen-signals-scenes-input.md)** – Separating Board/Piece from UI, PackedScene/Instantiate, signals, input (`_GuiInput`), and optional threading (`CallDeferred`).

3. **[From Input to Playable Loop](blogs/03-2d-platform-core-loop-godot-csharp.md)** – Building a beginner 2D platformer loop in Godot C#: movement, jumping, animation states, coin pickup, death flow, and restart.

4. **[Collision, Reusability, and Polish](blogs/04-2d-platform-collisions-reusability-polish.md)** – Collision layers/masks, reusable scenes, patrol AI with `RayCast2D`, animation-driven gameplay events, and beginner-safe polish.

### Series arc

- **Phase 1 (Fundamentals):** Intro to Godot + logic vs presentation (chess-based posts).
- **Phase 2 (Applied gameplay):** Platformer essentials (core loop + systems thinking), then other game types.
- **Phase 3 (In practice):** By game type – turn-based (e.g. Pokémon-style), action/real-time, then multiplayer (e.g. MOBA).

The chess project illustrates turn-based state, move history, and clear separation of rules vs view, which will be referenced in the turn-based in-practice post.

---

Topics for future posts may include:

* Designing a turn-based combat system
* Architecture of farming simulations
* Managing game state and persistence
* Real-time game loops vs request-response systems
* Integrating real-world data into gameplay

These posts aim to bridge **traditional software engineering and game development thinking**.

