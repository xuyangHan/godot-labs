using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Control
{
	Piece[,] board = new Piece[8,8];
	Square selectedSquare = null;
	private List<Square> highlightedSquares = new List<Square>();

	public override void _Ready()
	{
		GridContainer boardUI = GetNode<GridContainer>("Board");

		SetupBoard();

		var squareScene = GD.Load<PackedScene>("res://scenes/Square.tscn");

		for (int y = 0; y < 8; y++)
		{
			for (int x = 0; x < 8; x++)
			{
				Square square = squareScene.Instantiate<Square>();
				square.X = x;
				square.Y = y;

				square.SquareClicked += (s, button) => OnSquareClicked((Square)s, (string)button);

				boardUI.AddChild(square);
				square.SetPiece(board[x,y]);
			}
		}
	}

	void SetupBoard()
	{
		// Black pieces
		board[0,0] = new Piece(PieceType.Rook, PieceColor.Black);
		board[1,0] = new Piece(PieceType.Knight, PieceColor.Black);
		board[2,0] = new Piece(PieceType.Bishop, PieceColor.Black);
		board[3,0] = new Piece(PieceType.Queen, PieceColor.Black);
		board[4,0] = new Piece(PieceType.King, PieceColor.Black);
		board[5,0] = new Piece(PieceType.Bishop, PieceColor.Black);
		board[6,0] = new Piece(PieceType.Knight, PieceColor.Black);
		board[7,0] = new Piece(PieceType.Rook, PieceColor.Black);

		// Black pawns
		for (int x = 0; x < 8; x++)
			board[x,1] = new Piece(PieceType.Pawn, PieceColor.Black);

		// White pawns
		for (int x = 0; x < 8; x++)
			board[x,6] = new Piece(PieceType.Pawn, PieceColor.White);

		// White pieces
		board[0,7] = new Piece(PieceType.Rook, PieceColor.White);
		board[1,7] = new Piece(PieceType.Knight, PieceColor.White);
		board[2,7] = new Piece(PieceType.Bishop, PieceColor.White);
		board[3,7] = new Piece(PieceType.Queen, PieceColor.White);
		board[4,7] = new Piece(PieceType.King, PieceColor.White);
		board[5,7] = new Piece(PieceType.Bishop, PieceColor.White);
		board[6,7] = new Piece(PieceType.Knight, PieceColor.White);
		board[7,7] = new Piece(PieceType.Rook, PieceColor.White);
	}

	void OnSquareClicked(Square square, string button)
	{
		if (button == "right")
		{
			ResetSelection();
			return;
		}

		Piece clickedPiece = board[square.X, square.Y];
		
		// Nothing selected yet
		if (selectedSquare == null)
		{
			if (clickedPiece != null)
			{
				selectedSquare = square;
				square.SetSelected(true);

				// highlight legal moves
				var legalMovesofSelectedPiece = clickedPiece.GetLegalMoves(square.X, square.Y, board);
				highlightedSquares.Clear();

				foreach (var (lx, ly) in legalMovesofSelectedPiece)
				{
					Square moveSquare = GetSquare(lx, ly);
					moveSquare.SetHighlight(true);
					highlightedSquares.Add(moveSquare);
				}
			}
			return;
		}

		// Move piece
		Piece movingPiece = board[selectedSquare.X, selectedSquare.Y];
		var legalMoves = movingPiece.GetLegalMoves(selectedSquare.X, selectedSquare.Y, board);

		bool isLegal = legalMoves.Contains((square.X, square.Y));
		if (!isLegal)
		{
			GD.Print("Illegal move");
			ResetSelection();
			return;
		}

		board[square.X, square.Y] = movingPiece;
		board[selectedSquare.X, selectedSquare.Y] = null;

		selectedSquare.SetSelected(false);
		
		RefreshBoard();
		ResetSelection();
	}

	private Square GetSquare(int x, int y)
	{
		foreach (Square sq in GetNode<GridContainer>("Board").GetChildren())
		{
			if (sq.X == x && sq.Y == y)
				return sq;
		}
		return null;
	}

	void ResetSelection()
	{
		if (selectedSquare != null)
			selectedSquare.SetSelected(false);

		selectedSquare = null;
		ClearHighlights();
	}
	void ClearHighlights()
	{
		foreach (var sq in highlightedSquares)
		{
			sq.SetHighlight(false);
		}

		highlightedSquares.Clear();
	}

	void RefreshBoard()
	{
		foreach (Square square in GetNode<GridContainer>("Board").GetChildren())
		{
			square.SetPiece(board[square.X, square.Y]);
		}
	}
}
