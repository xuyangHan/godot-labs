# Introduction to Godot – Your First Game Project Part 1

*Part 1 of a series on game development with Godot. In this post, we’ll go from zero to understanding how a small game project is structured: scenes, nodes, and the scripts that tie them together.*

Godot is a powerful and approachable game engine, but when you first open a project the concepts can feel a little abstract. Terms like **scene**, **node**, and **scene tree** appear everywhere, and it’s not always obvious how they fit together in a real project.

In this series, we’ll start building a simple chess game while learning the core ideas behind Godot’s architecture.

In this post, we’ll focus on the **structure of a Godot project**:

* how scenes are organized
* how nodes form a hierarchy
* how scripts interact with the scene tree
* how the chess board UI is built dynamically in code

By the end, you’ll understand how a small Godot project is wired together and how code and scenes work as a system.

The full project used in this tutorial is available in the [project repository](/games/chess-game/), so you can follow along or explore the complete implementation.

---

## 1. Why Godot

Godot is an open-source game engine that supports both 2D and 3D games. It has become very popular among indie developers because it is lightweight, flexible, and easy to get started with.

Here are a few reasons it works well for learning game development:

* **Scenes and Nodes** – Everything in Godot is built using nodes arranged in a tree structure. UI elements, sprites, sound players, and game logic all follow the same pattern.
* **No lock-in** – A Godot project is made of regular files and scripts. There are no large proprietary binary formats, which makes version control and collaboration much easier.
* **C# support** – Godot 4 has first-class support for C# through .NET, so you can build games using a language many developers already know.

In this series we’ll use **Godot 4.x with C#**, but the concepts are almost identical if you prefer **GDScript**, Godot’s built-in scripting language.

---

## 2. Why Chess Is a Great First Game Project

When learning a new game engine, it’s tempting to start with something visually impressive. But for a first project, it’s often better to focus on something small and well-defined.

A chess game is a great example.

Chess has a clear set of rules and a limited game board. The gameplay is turn-based, so you don’t need to worry about complex timing systems or physics. This lets you focus on understanding how the engine works and how different parts of the program interact.

Some reasons chess works well as a first project:

* **Clear structure** – The board is always an 8×8 grid, which makes it easy to represent in code.
* **Turn-based gameplay** – Only one player moves at a time, which simplifies the game loop.
* **Logic-focused** – Most of the work is about implementing rules and movement logic rather than graphics or animations.
* **Expandable** – You can start simple (basic piece movement) and gradually add more features like castling, promotion, check detection, and AI.

Because of this, a chess project is a great way to learn how to structure a game, manage game state, and connect gameplay logic with UI elements.

In this series, we’ll build a small chess game step by step while learning the core ideas behind Godot: **scenes, nodes, and scripts**.

---

## 3. Project Setup

When you create a new Godot project, the engine generates a file called `project.godot` in the root folder. This file acts as the main configuration for the project: it stores the project name, the main scene, and various engine settings.

Here is a simplified example:

```ini
config/name="ChessGame"
run/main_scene="uid://l41k4m2kd4oq"
config/features=PackedStringArray("4.6", "C#", "Mobile")
```

The important entries are:

* **config/name** – The name of the project. This is shown in the editor and is usually used as the app name when exporting the game.
* **run/main_scene** – The scene that runs when you press **Play**. This is typically the main game screen.
* **config/features** – Tracks which engine version and features the project uses, such as the renderer and the fact that this project uses C#.

To get started, create a new project (for example using **Godot 4.6 with .NET/C#**). After the project is created, open `project.godot` and confirm that the `run/main_scene` value points to your main scene file, such as:

```
res://scenes/Main.tscn
```

This scene will be the entry point of our chess game.

---

## 4. Scenes and the Node Tree

In Godot, a **scene** is simply a tree of **nodes**. Each node has a type (such as `Control`, `Sprite2D`, or `GridContainer`) and can have child nodes. The editor displays this structure as a hierarchy.

You can think of a scene as a small self-contained part of the game. A scene might represent a menu, a character, a level, or in our case, the main game screen.

For our chess game, the main scene looks roughly like this:

```
Main (Control)
└── Layout (MarginContainer)
    └── HBox (HBoxContainer)
        ├── Board (GridContainer)   ← 8×8 grid of squares
        └── RightPanel (VBoxContainer)
            ├── MoveListScroll
            └── ButtonsPanel (New Game, Prev, Next)
```

Each node in the tree has a specific job:

* **Control** – The base class for UI elements. Our root node is a `Control`, which allows us to use layout anchors and UI containers.
* **MarginContainer / HBoxContainer / VBoxContainer** – Layout nodes that automatically arrange their children with margins, horizontally, or vertically.
* **GridContainer** – A container that places its children in a grid layout. We use this for the chess board.

For the chess board, the `GridContainer` is configured with **8 columns**. At runtime, we create **64 square nodes** and add them as children. The container automatically arranges them into an 8×8 grid.

One important idea here is that **the scene defines structure, not data**.
The board layout exists in the scene, but the actual squares and pieces are created and managed by code when the game runs.

You don’t need to memorize every node type right away. The key idea is simple:

> A scene usually has **one root node**, with a small tree of containers that define layout and structure.

---

## 5. Scripts and the Scene Tree

Scenes define the structure of the game, but **scripts define the behavior**.

In this project, we attach a C# script to the root node (`Main`). This script acts as the controller of the game. It gets references to important child nodes and coordinates the game logic.

For example, it loads the board, initializes the UI, and plays sounds when pieces move.

---

### `_Ready()` runs once

When a node enters the scene tree, Godot automatically calls a special function called `_Ready()`.

This is where you typically perform **one-time initialization**, such as:

* loading resources
* creating objects
* connecting signals
* getting references to child nodes

Example:

```csharp
public override void _Ready()
{
    squareScene = GD.Load<PackedScene>("res://scenes/Square.tscn");
    board.SetupBoard();

    boardGrid = GetNode<GridContainer>("Layout/HBox/Board");
    boardManager.Init(boardGrid, squareScene);

    moveSound = GetNode<AudioStreamPlayer>("MoveSound");
    promotionPanel = GetNode<PromotionPanel>("PromotionPanel");
}
```

In this method we:

1. Load the square scene used for board tiles
2. Initialize the board data
3. Find important UI nodes in the scene tree
4. Store references so we can use them later

---

### Getting child nodes with `GetNode`

To interact with nodes in the scene from a script, we use **node paths** and the `GetNode` function.

For example:

```csharp
boardGrid = GetNode<GridContainer>("Layout/HBox/Board");
moveSound = GetNode<AudioStreamPlayer>("MoveSound");
gameOverLabel = GetNode<Label>("GameOverPanel/GameOverLabel");
```

A node path works similarly to a file path:

* Each node name is separated by `/`
* The path starts from the current node (`Main` in this case)

The generic type (`<GridContainer>`, `<Label>`, etc.) tells Godot what type of node we expect. This gives us a **typed reference**, which means the compiler can catch errors and we don’t need to cast the result.

If you rename or move nodes in the editor, you will need to update these paths in your script.

This is one of the core patterns in Godot development:

> **Scenes define the structure. Scripts find nodes in that structure and control how the game behaves.**

---

## 6. Building the Board UI (Conceptual)

In our project, the **Board** node in the scene is just an empty `GridContainer`. We don’t manually place 64 squares in the editor.

Instead, the squares are **created at runtime**.

The process looks like this:

1. We load a **PackedScene** that represents a single square.
2. A `BoardManager` loops over all `(x, y)` board positions.
3. For each position, it **instantiates a new square** from the scene.
4. The square is given its board coordinates and added as a child of the grid.

This means the scene file defines the **layout**, while the script fills it with **content**.

```
GridContainer (8 columns)
├── Square
├── Square
├── Square
...
└── Square (64 total)
```

The important concept here is **scene instancing**.

Instead of creating 64 separate square scenes, we define **one reusable scene** and create multiple copies of it at runtime.

Each square is its own small scene. For example:

```
Square (Button)
├── PieceSprite
├── MoveHighlight
└── AttackHighlight
```

This structure lets every square manage its own visuals while the board logic stays centralized in the main game script.

So the key takeaway is:

> **Reusable scene = one definition, many instances.**

One square scene, instantiated **64 times** to form the chessboard.

We’ll look at the exact code for this in the next post.

---

## 7. Run and Iterate

Now it’s time to run the project.

Press **Play (F5)** and Godot will launch the main scene.

When the game starts:

1. The scene tree is constructed.
2. Nodes enter the tree.
3. Godot calls `_Ready()` on every node.

An important detail is that **children run `_Ready()` before their parents**. This ensures that by the time a parent node runs its setup code, its children are already initialized.

In our case, the `Main` script’s `_Ready()` method will:

* Load the square scene
* Initialize the board logic
* Create the 64 board squares
* Connect UI elements like the promotion panel and move sounds
* Refresh the board display

If everything is wired correctly, the board should appear automatically when the game starts.

![Chess board screenshot](../assets/chess-board.png)

*Screenshot: The chess board is generated at runtime by instantiating a single Square scene 64 times.*

---

### A simple experiment

A good way to understand the relationship between **scripts and nodes** is to try a small change.

Inside `_Ready()`, modify something visible in the scene. For example:

```csharp
var label = GetNode<Label>("GameOverPanel/GameOverLabel");
label.Text = "Hello from code!";
```

Or hide a UI panel:

```csharp
GetNode<Control>("GameOverPanel").Visible = false;
```

Run the project again and see how the scene changes.

This reinforces one of the most important ideas in Godot development:

> **Scripts find nodes in the scene tree and control their behavior.**

Once you’re comfortable with this pattern, building larger game systems becomes much easier.

---

## 8. Concepts Introduced

In this post we walked through some of the core ideas behind how a Godot project is structured. Even with a simple chess UI, these concepts form the foundation of most Godot games.

| Concept                | What it is                                                                                            |
| ---------------------- | ----------------------------------------------------------------------------------------------------- |
| **Scene**              | A tree of nodes saved in a `.tscn` file. Scenes are the main building blocks of a Godot project.      |
| **Node**               | A single element in the scene tree (UI element, sprite, container, logic node, etc.).                 |
| **`_Ready()`**         | A lifecycle method called once when the node enters the scene tree. Commonly used for initialization. |
| **`GetNode<T>(path)`** | Retrieves a child node by its path in the scene tree and returns a typed reference.                   |
| **Main Scene**         | The scene that runs when you press **Play**. This is configured in `project.godot`.                   |

Along the way we also introduced a few practical patterns:

* Using **container nodes** (`HBoxContainer`, `VBoxContainer`, `GridContainer`) to define UI layout
* Attaching **scripts** to nodes to control behavior
* Creating UI elements **at runtime** instead of manually placing everything in the editor

These ideas are small individually, but together they form the basic workflow of building games in Godot.

If you'd like to explore the full implementation — including the board logic, square scene, and project setup — the complete source code for this tutorial is available in the [project repository](/games/chess-game/).

---

### Next Post

In the next post we’ll move one step deeper into the architecture of the chess project.

We’ll cover:

* **Separating game logic from the UI**
* Using **signals** so board squares don’t depend directly on game rules
* **Instancing scenes in code** to create the 64 board squares

By the end, we’ll have a clean structure where the **board logic, UI, and interaction layers stay nicely decoupled** — which makes the project much easier to extend later.
