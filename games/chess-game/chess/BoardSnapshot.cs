public class BoardSnapshot
{
    public Piece[,] Board { get; }
    public (int x, int y)? EnPassantTarget { get; }
    public PieceColor Turn { get; }

    public BoardSnapshot(Piece[,] board, (int x, int y)? enPassantTarget, PieceColor turn)
    {
        Board = board;
        EnPassantTarget = enPassantTarget;
        Turn = turn;
    }
}
