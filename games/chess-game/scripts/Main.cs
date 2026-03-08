using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Control
{
	private Board board = new Board();
	private BoardManager boardManager = new BoardManager();
	private SelectionManager selectionManager;

	public PackedScene squareScene;

	private AudioStreamPlayer moveSound;
	private AudioStreamPlayer captureSound;

	public override void _Ready()
	{
		// Load square scene
		squareScene = GD.Load<PackedScene>("res://scenes/Square.tscn");
		
		// Initialize board logic
		board.SetupBoard();

		// Initialize board UI
		var boardUI = GetNode<GridContainer>("Board");
		boardManager.Init(boardUI, squareScene);

		// Initialize selection manager
		selectionManager = new SelectionManager(boardManager);

		// Wire clicks
		foreach (Square sq in GetNode<GridContainer>("Board").GetChildren())
		{
			sq.SquareClicked += (s, button) => OnSquareClicked((Square)s, (string)button);
		}

		moveSound = GetNode<AudioStreamPlayer>("MoveSound");
		captureSound = GetNode<AudioStreamPlayer>("CaptureSound");

		// Refresh initial board
		RefreshBoard();
	}

	void OnSquareClicked(Square square, string button)
	{
		if (button == "right")
		{
			selectionManager.ResetSelection();
			return;
		}

		Piece clickedPiece = board.GetPiece(square.X, square.Y);

		// Nothing selected yet
		if (!selectionManager.HasSelection())
		{
			if (clickedPiece != null)
			{
				selectionManager.SelectSquare(square, board);
			}
			return;
		}

		// Move piece
		var selected = selectionManager.GetSelectedSquare();
		Piece movingPiece = board.GetPiece(selected.X, selected.Y);

		var legalMoves = movingPiece.GetLegalMoves(selected.X, selected.Y, board.board);
		bool isLegal = legalMoves.Contains((square.X, square.Y));

		if (!isLegal)
		{
			GD.Print("Illegal move");
			selectionManager.ResetSelection();
			return;
		}

		var moveResult = board.MovePiece(selected.X, selected.Y, square.X, square.Y);
		switch (moveResult)
		{
			case MoveResult.Normal:
				moveSound.Play();
				break;

			case MoveResult.Capture:
				captureSound.Play();
				break;

			// case MoveResult.Castle:
			// 	castleSound.Play();
			// 	break;
		}
		
		RefreshBoard();
		selectionManager.ResetSelection();
	}

	void RefreshBoard()
	{
		foreach (Square square in GetNode<GridContainer>("Board").GetChildren())
		{
			square.SetPiece(board.GetPiece(square.X, square.Y));
		}
	}
}
