using Godot;
using System.Collections.Generic;

public partial class HistoryManager : Node
{
	[Signal] public delegate void RequestBoardStateEventHandler(int moveIndex);

	private List<MoveEntry> _history = new List<MoveEntry>();
	private List<Label> _moveLabels = new List<Label>();
	private int _currentIndex = -1;

	[Export] public GridContainer moveListGrid;

	public int CurrentIndex => _currentIndex;

	public override void _Ready()
	{
		moveListGrid = GetParent().GetNode<GridContainer>("Layout/HBox/RightPanel/MoveListScroll/MoveListGrid");
	}

	public MoveEntry GetEntry(int index) => _history[index];

	public void SetCurrentIndex(int index) => _currentIndex = index;

	public bool CanUndo() => _currentIndex >= 0;
	public bool CanRedo() => _currentIndex < _history.Count - 1;

	public void RecordMove(MoveEntry move)
	{
		// Truncate any future moves when branching from a past position
		if (_currentIndex < _history.Count - 1)
		{
			int removeFrom = _currentIndex + 1;
			int removeCount = _history.Count - removeFrom;

			// Each white move added 2 grid children (turn label + notation),
			// each black move added 1. Count correctly before removing.
			int cellsToRemove = 0;
			for (int i = removeFrom; i < _history.Count; i++)
				cellsToRemove += _history[i].PieceMoved.Color == PieceColor.White ? 2 : 1;

			for (int i = 0; i < cellsToRemove; i++)
			{
				var lastChild = moveListGrid.GetChild(moveListGrid.GetChildCount() - 1);
				moveListGrid.RemoveChild(lastChild);
				lastChild.QueueFree();
			}

			_history.RemoveRange(removeFrom, removeCount);
			_moveLabels.RemoveRange(removeFrom, removeCount);
		}

		_history.Add(move);
		_currentIndex++;

		bool isWhite = move.PieceMoved.Color == PieceColor.White;
		int turnNumber = (_history.Count + 1) / 2;

		if (isWhite)
		{
			var numLabel = new Label { Text = $"{turnNumber}." };
			moveListGrid.AddChild(numLabel);
		}

		var moveLabel = new Label { Text = move.Notation };
		moveLabel.MouseFilter = Control.MouseFilterEnum.Stop;
		moveLabel.GuiInput += (ev) => {
			if (ev is InputEventMouseButton mb && mb.Pressed)
				EmitSignal(SignalName.RequestBoardState, _history.IndexOf(move));
		};

		moveListGrid.AddChild(moveLabel);
		_moveLabels.Add(moveLabel);

		HighlightActiveMove(_currentIndex);
	}

	public void HighlightActiveMove(int index)
	{
		for (int i = 0; i < _moveLabels.Count; i++)
		{
			if (!GodotObject.IsInstanceValid(_moveLabels[i])) continue;

			if (i == index)
				_moveLabels[i].AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.1f));
			else
				_moveLabels[i].RemoveThemeColorOverride("font_color");
		}
	}

	public void Clear()
	{
		_history.Clear();
		_moveLabels.Clear();
		_currentIndex = -1;

		// Remove all grid children except the 3 header labels (Turn / White / Black)
		int childCount = moveListGrid.GetChildCount();
		for (int i = childCount - 1; i >= 3; i--)
		{
			var child = moveListGrid.GetChild(i);
			moveListGrid.RemoveChild(child);
			child.QueueFree();
		}
	}
}
