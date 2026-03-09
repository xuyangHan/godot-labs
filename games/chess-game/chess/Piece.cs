using System;
using System.Collections.Generic;

public enum PieceType
{
	Pawn,
	Rook,
	Knight,
	Bishop,
	Queen,
	King
}

public enum PieceColor
{
	White,
	Black
}

public class Piece
{
	public PieceType Type { get; set; }
	public PieceColor Color { get; set; }
	public bool HasMoved { get; set; } = false;

	public Piece(PieceType type, PieceColor color)
	{
		Type = type;
		Color = color;
	}

	public List<(int x, int y)> GetLegalMoves(int fromX, int fromY, Piece[,] board)
	{
		switch (Type)
		{
			case PieceType.Pawn: return GetPawnMoves(fromX, fromY, board);
			case PieceType.Knight: return GetKnightMoves(fromX, fromY, board);
			case PieceType.Bishop: return GetBishopMoves(fromX, fromY, board);
			case PieceType.Rook: return GetRookMoves(fromX, fromY, board);
			case PieceType.Queen: return GetQueenMoves(fromX, fromY, board);
			case PieceType.King: return GetKingMoves(fromX, fromY, board);
			default: return new List<(int,int)>();
		}
	}

	private List<(int,int)> GetKnightMoves(int x, int y, Piece[,] board)
	{
		int[] dx = { 1, 2, 2, 1, -1, -2, -2, -1 };
		int[] dy = { -2, -1, 1, 2, 2, 1, -1, -2 };

		var moves = new List<(int,int)>();

		for (int i = 0; i < dx.Length; i++)
		{
			int nx = x + dx[i];
			int ny = y + dy[i];

			if (nx < 0 || nx >= 8 || ny < 0 || ny >= 8)
				continue;

			// Can move if square is empty or enemy piece
			if (board[nx, ny] == null || board[nx, ny].Color != Color)
				moves.Add((nx, ny));
		}

		return moves;
	}

	private List<(int,int)> GetPawnMoves(int x, int y, Piece[,] board)
	{
		var moves = new List<(int,int)>();

		int direction = (Color == PieceColor.White) ? -1 : 1;
		int startRow = (Color == PieceColor.White) ? 6 : 1;

		int forward = y + direction;

		// Move one square forward (must be empty)
		if (forward >= 0 && forward < 8 && board[x, forward] == null)
		{
			moves.Add((x, forward));

			// Move two squares from starting position (both empty)
			int twoForward = y + direction * 2;
			if (y == startRow && twoForward >= 0 && twoForward < 8 && board[x, twoForward] == null)
			{
				moves.Add((x, twoForward));
			}
		}

		// Capture diagonally
		int[] dx = { -1, 1 };
		foreach (int offset in dx)
		{
			int nx = x + offset;
			int ny = y + direction;

			if (nx >= 0 && nx < 8 && ny >= 0 && ny < 8)
			{
				if (board[nx, ny] != null && board[nx, ny].Color != Color)
				{
					moves.Add((nx, ny));
				}
			}
		}

		return moves;
	}

	private List<(int,int)> GetKingMoves(int x, int y, Piece[,] board)
	{
		var moves = new List<(int,int)>();

		// normal king moves
		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dy = -1; dy <= 1; dy++)
			{
				if (dx == 0 && dy == 0)
					continue;

				int nx = x + dx;
				int ny = y + dy;

				if (nx < 0 || nx >= 8 || ny < 0 || ny >= 8)
					continue;

				if (board[nx, ny] == null || board[nx, ny].Color != Color)
				{
					moves.Add((nx, ny));
				}
			}
		}

		// castling
		if (!HasMoved)
		{
			// king side
			if (CanCastleKingSide(x, y, board))
				moves.Add((x + 2, y));

			// queen side
			if (CanCastleQueenSide(x, y, board))
				moves.Add((x - 2, y));
		}

		return moves;
	}

	private bool CanCastleKingSide(int x, int y, Piece[,] board)
	{
		Piece rook = board[7, y];

		if (rook == null || rook.Type != PieceType.Rook || rook.HasMoved)
			return false;

		if (board[5, y] != null || board[6, y] != null)
			return false;

		return true;
	}

	private bool CanCastleQueenSide(int x, int y, Piece[,] board)
	{
		Piece rook = board[0, y];

		if (rook == null || rook.Type != PieceType.Rook || rook.HasMoved)
			return false;

		if (board[1, y] != null || board[2, y] != null || board[3, y] != null)
			return false;

		return true;
	}

	private List<(int,int)> GetBishopMoves(int x, int y, Piece[,] board)
	{
		var moves = new List<(int,int)>();

		int[] directions = { -1, 1 };

		foreach (int dx in directions)
		{
			foreach (int dy in directions)
			{
				int nx = x + dx;
				int ny = y + dy;

				while (nx >= 0 && nx < 8 && ny >= 0 && ny < 8)
				{
					if (board[nx, ny] == null)
					{
						moves.Add((nx, ny));
					}
					else
					{
						// Can capture enemy piece but cannot go past it
						if (board[nx, ny].Color != Color)
							moves.Add((nx, ny));

						break;
					}

					nx += dx;
					ny += dy;
				}
			}
		}

		return moves;
	}

	private List<(int,int)> GetRookMoves(int x, int y, Piece[,] board)
	{
		var moves = new List<(int,int)>();

		int[] directions = { -1, 1 };

		// Horizontal
		foreach (int dx in directions)
		{
			int nx = x + dx;

			while (nx >= 0 && nx < 8)
			{
				if (board[nx, y] == null)
				{
					moves.Add((nx, y));
				}
				else
				{
					if (board[nx, y].Color != Color)
						moves.Add((nx, y));

					break;
				}

				nx += dx;
			}
		}

		// Vertical
		foreach (int dy in directions)
		{
			int ny = y + dy;

			while (ny >= 0 && ny < 8)
			{
				if (board[x, ny] == null)
				{
					moves.Add((x, ny));
				}
				else
				{
					if (board[x, ny].Color != Color)
						moves.Add((x, ny));

					break;
				}

				ny += dy;
			}
		}

		return moves;
	}

	private List<(int,int)> GetQueenMoves(int x, int y, Piece[,] board)
	{
		var moves = new List<(int,int)>();

		moves.AddRange(GetRookMoves(x, y, board));
		moves.AddRange(GetBishopMoves(x, y, board));

		return moves;
	}
}

public enum MoveResult
{
	Normal,
	Capture,
	Castle,
	Illegal,
	Promotion
}
