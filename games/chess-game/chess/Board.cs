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

    public void MovePiece(int fromX, int fromY, int toX, int toY)
    {
        board[toX, toY] = board[fromX, fromY];
        board[fromX, fromY] = null;
    }

    public Piece GetPiece(int x, int y) => board[x, y];
    
}