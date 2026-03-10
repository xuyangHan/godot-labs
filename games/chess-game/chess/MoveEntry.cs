using System.Collections.Generic;

public class MoveEntry
{
    public int FromX, FromY;
    public int ToX, ToY;
    public string Notation; // e.g., "e2e4" or "Nf3"
    public Piece PieceMoved;
    public Piece PieceCaptured; // Important for Undoing!
    public bool IsPromotion;
    
    // For the future Tree structure
    public List<MoveEntry> Variations = new List<MoveEntry>();
}