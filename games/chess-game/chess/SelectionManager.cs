using System.Collections.Generic;
using Godot;

public class SelectionManager
{
    private BoardManager boardManager;
    private Square selectedSquare;
    private List<Square> highlightedSquares = new List<Square>();

    public SelectionManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    // Check if any square is currently selected
    public bool HasSelection()
    {
        return selectedSquare != null;
    }

    // Return the currently selected square
    public Square GetSelectedSquare()
    {
        return selectedSquare;
    }

    public void SelectSquare(Square square, Board board)
    {
        ClearHighlights();
        selectedSquare = square;
        square.SetSelected(true);

        var piece = board.GetPiece(square.X, square.Y);
        if (piece == null) return;

        var moves = board.GetLegalMoves(piece, square.X, square.Y);
        foreach (var (x, y) in moves)
        {
            var sq = boardManager.GetSquare(x, y);
            var targetPiece = board.GetPiece(x, y);

            if (targetPiece == null)
            {
                // empty square → normal highlight
                sq.SetHighlight(true, HighlightType.Move); 
            }
            else if (targetPiece.Color != piece.Color)
            {
                // opponent piece → attack highlight
                sq.SetHighlight(true, HighlightType.Attack);
            }
            else
            {
                // your own piece → maybe ignore or skip
                continue;
            }
            highlightedSquares.Add(sq);
        }
    }

    public void ResetSelection()
    {
        if (selectedSquare != null)
            selectedSquare.SetSelected(false);

        selectedSquare = null;
        ClearHighlights();
    }

    private void ClearHighlights()
    {
        foreach (var sq in highlightedSquares)
            sq.SetHighlight(false);
        highlightedSquares.Clear();
    }
}