using Godot;
using System;


public partial class Square : Button
{
	public int X;
	public int Y;
	
	Sprite2D pieceSprite;

	[Signal]
	public delegate void SquareClickedEventHandler(Square square);

	StyleBoxFlat normalStyle;
	StyleBoxFlat hoverStyle;
	StyleBoxFlat selectedStyle;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pieceSprite = GetNode<Sprite2D>("PieceSprite");

		normalStyle = new StyleBoxFlat();
		hoverStyle = new StyleBoxFlat();
		selectedStyle = new StyleBoxFlat();

		if ((X + Y) % 2 == 0)
			normalStyle.BgColor = new Color("#ccdae0");
		else
			normalStyle.BgColor = new Color("#7498ad");

		hoverStyle.BgColor = normalStyle.BgColor.Lightened(0.1f);

		selectedStyle.BgColor = new Color("#6dbad1"); // yellow highlight

		AddThemeStyleboxOverride("normal", normalStyle);
		AddThemeStyleboxOverride("hover", hoverStyle);
		AddThemeStyleboxOverride("pressed", normalStyle);
		AddThemeStyleboxOverride("focus", normalStyle);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	

	public override void _Pressed()
	{
		GD.Print($"Clicked square {X},{Y}");
		EmitSignal(SignalName.SquareClicked, this);
	}
	
	public void SetPiece(Piece piece)
	{
		if (piece == null)
		{
			pieceSprite.Texture = null;
			return;
		}
		if (piece.Type == PieceType.Pawn && piece.Color == PieceColor.White)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/white-pawn.png");
		if (piece.Type == PieceType.Rook && piece.Color == PieceColor.White)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/white-rook.png");
		if (piece.Type == PieceType.Knight && piece.Color == PieceColor.White)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/white-knight.png");
		if (piece.Type == PieceType.Bishop && piece.Color == PieceColor.White)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/white-bishop.png");
		if (piece.Type == PieceType.Queen && piece.Color == PieceColor.White)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/white-queen.png");
		if (piece.Type == PieceType.King && piece.Color == PieceColor.White)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/white-king.png");

		if (piece.Type == PieceType.Pawn && piece.Color == PieceColor.Black)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/black-pawn.png");
		if (piece.Type == PieceType.Rook && piece.Color == PieceColor.Black)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/black-rook.png");
		if (piece.Type == PieceType.Knight && piece.Color == PieceColor.Black)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/black-knight.png");
		if (piece.Type == PieceType.Bishop && piece.Color == PieceColor.Black)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/black-bishop.png");
		if (piece.Type == PieceType.Queen && piece.Color == PieceColor.Black)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/black-queen.png");
		if (piece.Type == PieceType.King && piece.Color == PieceColor.Black)
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/black-king.png");
	}

	public void SetSelected(bool selected)
	{
		if (selected)
			AddThemeStyleboxOverride("normal", selectedStyle);
		else
			AddThemeStyleboxOverride("normal", normalStyle);
	}
	
}
