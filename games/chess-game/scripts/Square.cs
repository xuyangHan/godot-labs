using Godot;
using System;

public partial class Square : Button
{
	public int X;
	public int Y;
	
	Sprite2D pieceSprite;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pieceSprite = GetNode<Sprite2D>("PieceSprite");

		var style = new StyleBoxFlat();
		var hoverStyle = new StyleBoxFlat();

		if ((X + Y) % 2 == 0)
			style.BgColor = new Color("#ccdae0");
		else
			style.BgColor = new Color("#7498ad");

		hoverStyle.BgColor = style.BgColor.Lightened(0.1f);

		AddThemeStyleboxOverride("normal", style);
		AddThemeStyleboxOverride("hover", hoverStyle);
		AddThemeStyleboxOverride("pressed", style);
		AddThemeStyleboxOverride("focus", style);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	

	public override void _Pressed()
	{
		GD.Print($"Clicked square {X},{Y}");
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
}
