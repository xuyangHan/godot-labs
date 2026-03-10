using Godot;
using System.Collections.Generic;

public partial class HistoryManager : Node
{
	// Define a signal for when a player clicks a move to explore
	[Signal] public delegate void RequestBoardStateEventHandler(int moveIndex);

	private List<MoveEntry> _history = new List<MoveEntry>();
	private int _currentIndex = -1;

	[Export] public GridContainer moveListGrid;

	public override void _Ready()
	{
		moveListGrid = GetNode<GridContainer>("MoveListGrid");
	}

	public void RecordMove(MoveEntry move)
	{
		// 1. Handle Branching (Delete future moves if we were in the past)
		if (_currentIndex < _history.Count - 1)
		{
			int removeCount = _history.Count - (_currentIndex + 1);
			_history.RemoveRange(_currentIndex + 1, removeCount);
			
			// Remove the corresponding children from the GridContainer
			for (int i = 0; i < removeCount; i++)
			{
				var lastChild = moveListGrid.GetChild(moveListGrid.GetChildCount() - 1);
				lastChild.QueueFree();
			}
		}

		// 2. Add to Memory
		_history.Add(move);
		_currentIndex++;

		// 3. UI Update: Add to GridContainer
		// Determine turn and color
		bool isWhite = (move.PieceMoved.Color == PieceColor.White);
		int turnNumber = (_history.Count + 1) / 2;

		if (isWhite)
		{
			var numLabel = new Label { Text = $"{turnNumber}." };
			moveListGrid.AddChild(numLabel);
		}

		var moveLabel = new Label { Text = move.Notation };
		moveLabel.MouseFilter = Control.MouseFilterEnum.Stop;
		
		// Add click behavior
		moveLabel.GuiInput += (ev) => {
			if (ev is InputEventMouseButton mb && mb.Pressed)
				EmitSignal(SignalName.RequestBoardState, _history.IndexOf(move));
		};

		moveListGrid.AddChild(moveLabel);
	}

	public MoveEntry Undo()
	{
		if (!CanUndo()) return null;
		return _history[_currentIndex--];
	}

	public bool CanUndo() => _currentIndex >= 0;
	public bool CanRedo() => _currentIndex < _history.Count - 1;
}
