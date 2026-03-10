using System;
using System.Collections.Generic;
using Godot;

public class Board
{
    public Piece[,] board = new Piece[8,8];
	public (int x, int y)? EnPassantTarget = null;

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
        return piece.GetLegalMoves(x, y, this);
    }

    public (MoveResult Result, MoveEntry Entry) MovePiece(int fromX, int fromY, int toX, int toY)
    {
		Piece movingPiece = board[fromX, fromY];
		Piece targetPiece = board[toX, toY];

		if (movingPiece == null)
			return (MoveResult.Illegal, null);

		var previousEnPassant = EnPassantTarget;
    	EnPassantTarget = null;
		
		// Create the history entry data
		MoveEntry entry = new MoveEntry {
			FromX = fromX, FromY = fromY,
			ToX = toX, ToY = toY,
			PieceMoved = movingPiece,
			PieceCaptured = targetPiece, // Save this for UNDO later!
			Notation = GetNotation(fromX, fromY, toX, toY, movingPiece, targetPiece != null)
		};

		
		if (movingPiece.Type == PieceType.Pawn)
		{
			// EN PASSANT CAPTURE
			if (previousEnPassant != null &&
				toX == previousEnPassant.Value.x &&
				toY == previousEnPassant.Value.y)
			{
				int captureY = movingPiece.Color == PieceColor.White ? toY + 1 : toY - 1;

				Piece capturedPawn = board[toX, captureY];
				entry.PieceCaptured = capturedPawn;

				board[toX, captureY] = null;

				board[toX, toY] = movingPiece;
				board[fromX, fromY] = null;

				movingPiece.HasMoved = true;
				entry.Notation = GetNotation(fromX, fromY, toX, toY, movingPiece, true);

				return (MoveResult.Capture, entry);
			}

			// Pawn promotion
			if ((movingPiece.Color == PieceColor.White && toY == 0) ||
				(movingPiece.Color == PieceColor.Black && toY == 7))
			{
				board[toX, toY] = movingPiece;
				movingPiece.HasMoved = true;
				board[fromX, fromY] = null;
				return (MoveResult.Promotion, entry);
			}

			// Set en passant target after double step
			if (Math.Abs(toY - fromY) == 2)
			{
				int direction = movingPiece.Color == PieceColor.White ? -1 : 1;
				EnPassantTarget = (fromX, fromY + direction);
			}
		}

		// capture
		if (targetPiece != null)
		{
			board[toX, toY] = movingPiece;
			board[fromX, fromY] = null;
			movingPiece.HasMoved = true;

			return (MoveResult.Capture, entry);
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
			return (MoveResult.Castle, entry);
		}

		// normal move
		board[toX, toY] = movingPiece;
		movingPiece.HasMoved = true;
		board[fromX, fromY] = null;

		return (MoveResult.Normal, entry);
    }

    public Piece GetPiece(int x, int y) => board[x, y];
    

	private string GetNotation(int fx, int fy, int tx, int ty, Piece p, bool isCapture, string promotionType = "")
	{
		string[] files = { "a","b","c","d","e","f","g","h" };
		string[] ranks = { "8","7","6","5","4","3","2","1" };

		// Castling
		if (p.Type == PieceType.King && Math.Abs(tx - fx) == 2)
			return tx > fx ? "O-O" : "O-O-O";

		string piecePrefix = "";

		switch (p.Type)
		{
			case PieceType.Knight: piecePrefix = "N"; break;
			case PieceType.Bishop: piecePrefix = "B"; break;
			case PieceType.Rook:   piecePrefix = "R"; break;
			case PieceType.Queen:  piecePrefix = "Q"; break;
			case PieceType.King:   piecePrefix = "K"; break;
		}

		// Pawn capture needs source file
		if (p.Type == PieceType.Pawn && isCapture)
			piecePrefix = files[fx];

		string moveStr = piecePrefix;

		if (isCapture)
			moveStr += "x";

		moveStr += files[tx] + ranks[ty];

		if (!string.IsNullOrEmpty(promotionType))
			moveStr += "=" + promotionType.Substring(0,1).ToUpper();

		return moveStr;
	}

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

	public bool MoveLeavesKingInCheck(int fx, int fy, int tx, int ty, PieceColor color)
	{
		Piece movingPiece = board[fx, fy];
		Piece captured = board[tx, ty];

		// simulate move
		board[tx, ty] = movingPiece;
		board[fx, fy] = null;

		bool inCheck = IsKingInCheck(color);

		// undo move
		board[fx, fy] = movingPiece;
		board[tx, ty] = captured;

		return inCheck;
	}

	public bool IsKingInCheck(PieceColor color)
	{
		(int kx, int ky) = FindKing(color);

		PieceColor enemy = color == PieceColor.White
			? PieceColor.Black
			: PieceColor.White;

		for (int x = 0; x < 8; x++)
		{
			for (int y = 0; y < 8; y++)
			{
				Piece p = board[x, y];

				if (p != null && p.Color == enemy)
				{
					var moves = p.GetPseudoMoves(x, y, this);

					foreach (var m in moves)
					{
						if (m.x == kx && m.y == ky)
							return true;
					}
				}
			}
		}

		return false;
	}

	public (int,int) FindKing(PieceColor color)
	{
		for (int x = 0; x < 8; x++)
		{
			for (int y = 0; y < 8; y++)
			{
				if (board[x,y] != null &&
					board[x,y].Type == PieceType.King &&
					board[x,y].Color == color)
				{
					return (x,y);
				}
			}
		}

		return (-1,-1);
	}

	public bool HasAnyLegalMove(PieceColor color)
	{
		for (int x = 0; x < 8; x++)
			for (int y = 0; y < 8; y++)
			{
				Piece p = board[x, y];
				if (p != null && p.Color == color && p.GetLegalMoves(x, y, this).Count > 0)
					return true;
			}
		return false;
	}
	
	public bool IsCheckmate(PieceColor color) =>
		IsKingInCheck(color) && !HasAnyLegalMove(color);
	
	public bool IsStalemate(PieceColor color) =>
		!IsKingInCheck(color) && !HasAnyLegalMove(color);
}