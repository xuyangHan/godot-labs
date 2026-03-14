# From game logic to the screen – Signals, scenes, and input

*Part 2 of the Godot game development series. We look at how to keep game rules separate from the engine, how to build the board from a reusable scene, and how signals and input connect the two.*

---

## 1. Why separate logic from presentation

Game rules—what is a legal move, who wins, what state changes—should be testable and independent of the engine. If you ever port the game or rewrite the UI, you want the core logic to stay intact.

In this project that split is explicit:

- **`chess/`** – Plain C# classes: `Board`, `Piece`, `BoardManager`, `SelectionManager`, `BoardSnapshot`, `MoveEntry`, `ChessAI`. They have **no** `using Godot` (or at most `GD.Print` for debugging). They work with arrays, enums, and method calls.
- **`scripts/` and scenes** – `Main`, `Square`, `PromotionPanel`, `HistoryManager` (as a Node). These hold references to UI nodes, load resources, and **call into** the chess types. They are the “view.”

So: **logic lives in engine-agnostic code; the view talks to that logic and updates the screen.** Main is the coordinator: it owns the Board and the UI and connects clicks to moves.

---

## 2. Data model in plain C#

The game state is a 8×8 grid of pieces and a few extra fields (e.g. en passant target). No nodes, no textures.

**Board** – Holds the grid and game flow:

```csharp
public class Board
{
    public Piece[,] board = new Piece[8, 8];

    public void SetupBoard() { /* place pieces in starting position */ }
    public Piece GetPiece(int x, int y) => /* bounds check and return */;
    public (MoveResult Result, MoveEntry Entry) MovePiece(int fromX, int fromY, int toX, int toY) { /* ... */ }
}
```

**Piece** – Type, color, and move generation:

```csharp
public class Piece
{
    public PieceType Type { get; set; }
    public PieceColor Color { get; set; }

    public List<(int x, int y)> GetLegalMoves(int fromX, int fromY, Board board)
    {
        var pseudoMoves = GetPseudoMoves(fromX, fromY, board);
        var legalMoves = new List<(int, int)>();
        foreach (var move in pseudoMoves)
        {
            if (!board.MoveLeavesKingInCheck(fromX, fromY, move.x, move.y, Color))
                legalMoves.Add(move);
        }
        return legalMoves;
    }
}
```

Pseudo-legal moves are “all moves this piece type can make”; legal moves are “those that don’t leave our king in check.” That filtering happens in one place: the Board/Piece layer. The UI never decides what’s legal; it only asks and displays.

---

## 3. Creating the board UI from a scene

We don’t create 64 squares by hand in the editor. We use one **Square** scene and instantiate it 64 times.

**PackedScene** – Load once, instantiate many times:

```csharp
squareScene = GD.Load<PackedScene>("res://scenes/Square.tscn");
```

**BoardManager** – Fills the grid with square instances:

```csharp
public void Init(GridContainer boardUI, PackedScene squareScene)
{
    this.boardUI = boardUI;
    for (int y = 0; y < 8; y++)
    {
        for (int x = 0; x < 8; x++)
        {
            Square sq = squareScene.Instantiate<Square>();
            sq.X = x; sq.Y = y;
            boardUI.AddChild(sq);
            squares[x, y] = sq;
        }
    }
}
```

Each square gets coordinates `(x, y)` so that when it’s clicked, we know which cell it is. The **single source of truth** for “what piece is where” is `Board.board`. The UI only reflects it: e.g. `RefreshBoard()` loops over squares and calls `square.SetPiece(board.GetPiece(square.X, square.Y))`.

---

## 4. Signals: UI not calling game logic directly

Squares don’t know about the Board or the rules. They only know “I was clicked” and “which button.” They **emit a signal**; the parent (Main) subscribes and decides what to do.

**Declare the signal** (on Square):

```csharp
[Signal]
public delegate void SquareClickedEventHandler(Square square, string button);
```

**Emit it** – on left-click via the Button’s press:

```csharp
public override void _Pressed()
{
    EmitSignal(SignalName.SquareClicked, this, "left");
}
```

And for right-click we use `_GuiInput` (see next section) and emit with `"right"`.

**Subscribe** (in Main’s `_Ready()`):

```csharp
foreach (Square sq in boardGrid.GetChildren())
{
    sq.SquareClicked += (s, button) => OnSquareClicked((Square)s, (string)button);
}
```

So: **child emits, parent (or coordinator) subscribes and applies game logic.** The same pattern appears elsewhere: `PromotionPanel.PromotionSelected`, `HistoryManager.RequestBoardState`. The UI raises events; Main (or another coordinator) translates them into moves, history jumps, or panel visibility.

---

## 5. Input handling

We need two kinds of click: left (select/move) and right (deselect). The Button node gives us `_Pressed()` for the primary action (left-click). For right-click we override `_GuiInput` and interpret the event ourselves:

```csharp
public override void _GuiInput(InputEvent @event)
{
    if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
    {
        if (mouseButton.ButtonIndex == MouseButton.Left)
            EmitSignal(SignalName.SquareClicked, this, "left");
        else if (mouseButton.ButtonIndex == MouseButton.Right)
            EmitSignal(SignalName.SquareClicked, this, "right");
    }
}
```

So input stays at the **view** layer: Square turns raw input into “this square, this button.” Game logic only sees “square (x, y) and which button”; it never touches `InputEvent` or mouse indices.

---

## 6. Optional: threading and the main thread

The chess AI runs Stockfish in a separate process. Its output is read on a **background thread**. When Stockfish sends a “bestmove” line, our callback runs on that thread—but Godot’s APIs (nodes, scene tree, `GetNode`, etc.) are **not** thread-safe. We must run the board update on the main thread.

**ChessAI** invokes the callback from the reader thread:

```csharp
void ReadOutput()
{
    while (!engine.StandardOutput.EndOfStream)
    {
        string line = engine.StandardOutput.ReadLine();
        if (line != null && line.StartsWith("bestmove "))
            OnBestMoveReceived?.Invoke(line);
    }
}
```

**Main** subscribes with `CallDeferred`, so the handler runs on the main thread:

```csharp
ai.OnBestMoveReceived = (line) => CallDeferred(nameof(OnStockfishResponse), line);
```

Rule of thumb: **when you get a callback or event from another thread, use `CallDeferred` to run the code that touches the scene tree or any Godot object.**

---

## Concepts covered

| Concept | Role |
|--------|------|
| **Logic vs view** | Game rules in plain C#; UI and nodes only display and forward input. |
| **PackedScene / Instantiate** | One scene asset, many instances created in code. |
| **Signals** | Declare with `[Signal]` and a delegate; emit with `EmitSignal`; connect with `+=`. Child emits, parent subscribes. |
| **_GuiInput** | Custom input handling on a Control; we use it to distinguish left vs right click and emit one signal. |
| **CallDeferred** | Run a method on the main thread from another thread when integrating async or external processes. |

With this, you have a clear split between “what the game is” (Board, Piece, moves) and “how it’s shown and controlled” (Main, Square, signals, input). In later posts we’ll build on this for turn-based design, then action games, and beyond.
