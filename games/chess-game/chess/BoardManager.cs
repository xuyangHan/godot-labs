using Godot;

public class BoardManager
{
    private GridContainer boardUI;
    private Square[,] squares = new Square[8,8];

    public void Init(GridContainer boardUI, PackedScene squareScene)
    {
        this.boardUI = boardUI;

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Square sq = squareScene.Instantiate<Square>();
                sq.X = x; sq.Y = y;
                boardUI.AddChild(sq);
                squares[x, y] = sq;
            }  
        }
        
    }

    public Square GetSquare(int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return null;
        return squares[x, y];
    }
}