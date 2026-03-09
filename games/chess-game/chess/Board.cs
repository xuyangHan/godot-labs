using System;
using System.Collections.Generic;
using Godot;

public class Board
{
    public Piece[,] board = new Piece[8,8];

    public void SetupBoard() {
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

    public List<(int x, int y)> GetLegalMoves(Piece piece, int x, int y)
    {
        return piece.GetLegalMoves(x, y, board);
    }

    public MoveResult MovePiece(int fromX, int fromY, int toX, int toY)
    {
		Piece movingPiece = board[fromX, fromY];
		Piece targetPiece = board[toX, toY];

		if (movingPiece == null)
			return MoveResult.Illegal;

		// Pawn promotion
		if (movingPiece.Type == PieceType.Pawn)
		{
			if ((movingPiece.Color == PieceColor.White && toY == 0) ||
				(movingPiece.Color == PieceColor.Black && toY == 7))
			{
				board[toX, toY] = movingPiece;
				movingPiece.HasMoved = true;
				board[fromX, fromY] = null;
				return MoveResult.Promotion;
			}
		}

		// capture
		if (targetPiece != null)
		{
			board[toX, toY] = movingPiece;
			board[fromX, fromY] = null;
			movingPiece.HasMoved = true;

			return MoveResult.Capture;
		}

		// castle
		if (movingPiece.Type == PieceType.King && Math.Abs(toX - fromX) == 2)
		{
			// Move king
			board[toX, toY] = movingPiece;
			board[fromX, fromY] = null;

			// king side castle
			if (toX > fromX)
			{
				board[5, fromY] = board[7, fromY];
				board[7, fromY] = null;
			}
			// queen side castle
			else
			{
				board[3, fromY] = board[0, fromY];
				board[0, fromY] = null;
			}

			movingPiece.HasMoved = true;
			return MoveResult.Castle;
		}

		// normal move
		board[toX, toY] = movingPiece;
		movingPiece.HasMoved = true;
		board[fromX, fromY] = null;

		return MoveResult.Normal;
    }

    public Piece GetPiece(int x, int y) => board[x, y];
    
	public void ConvertNearbyPieces(int centerX, int centerY, PieceColor playerColor)
	{
		// Check all squares around the cat (8 directions)
		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dy = -1; dy <= 1; dy++)
			{
				int targetX = centerX + dx;
				int targetY = centerY + dy;

				// Ensure we are within board bounds
				if (targetX >= 0 && targetX < 8 && targetY >= 0 && targetY < 8)
				{
					Piece piece = GetPiece(targetX, targetY);
					// If there's an enemy piece, switch its color
					if (piece != null && piece.Color != playerColor && piece.Type != PieceType.King)
					{
						piece.Color = playerColor;
						GD.Print($"Cat magic! Converted piece at {targetX}, {targetY}");
					}
				}
			}
		}
	}
}