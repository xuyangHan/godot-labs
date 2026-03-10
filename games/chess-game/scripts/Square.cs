using Godot;
using System;


public partial class Square : Button
{
	public int X;
	public int Y;
	
	Sprite2D pieceSprite;

	[Signal]
	public delegate void SquareClickedEventHandler(Square square, string button);

	StyleBoxFlat normalStyle;
	StyleBoxFlat hoverStyle;
	StyleBoxFlat selectedStyle;
	TextureRect moveHighlight;
	TextureRect attackHighlight;

	
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

		selectedStyle.BgColor = new Color("#6dbad1"); 

		AddThemeStyleboxOverride("normal", normalStyle);
		AddThemeStyleboxOverride("hover", hoverStyle);
		AddThemeStyleboxOverride("pressed", normalStyle);
		AddThemeStyleboxOverride("focus", normalStyle);

		moveHighlight = GetNode<TextureRect>("MoveHighlight");
		attackHighlight = GetNode<TextureRect>("AttackHighlight");

		SetupHighlight(moveHighlight, 0.5f, 0.5f);
		SetupHighlight(attackHighlight, 0.7f, 0.1f);
		moveHighlight.Visible = false;
		attackHighlight.Visible = false;

		AddCoordinateLabel();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	

	public override void _Pressed()
	{
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
		if (piece.Type == PieceType.Cat && piece.Color == PieceColor.White)
		{
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/white-cat.png");
			pieceSprite.Scale = new Vector2(0.25f, 0.25f);
		}

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
		if (piece.Type == PieceType.Cat && piece.Color == PieceColor.Black)
		{	
			pieceSprite.Texture = GD.Load<Texture2D>("res://assets/pieces/black-cat.png");
			pieceSprite.Scale = new Vector2(0.25f, 0.25f);
		}
	}

	public void SetSelected(bool selected)
	{
		if (selected)
			AddThemeStyleboxOverride("normal", selectedStyle);
		else
			AddThemeStyleboxOverride("normal", normalStyle);
	}

	public void SetHighlight(bool highlightOn, HighlightType type = HighlightType.Move)
	{
		moveHighlight.Visible = highlightOn && type == HighlightType.Move;
		attackHighlight.Visible = highlightOn && type == HighlightType.Attack;
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
				EmitSignal(SignalName.SquareClicked, this, "left");
			else if (mouseButton.ButtonIndex == MouseButton.Right)
				EmitSignal(SignalName.SquareClicked, this, "right");
		}
	}
	
	private void SetupHighlight(TextureRect highlight, float alpha, float margin)
	{
		highlight.Modulate = new Color(1, 1, 1, alpha); // half transparent

		// make it stretch to fill the button (the square)
		highlight.AnchorLeft = 0;
		highlight.AnchorTop = 0;
		highlight.AnchorRight = 1;
		highlight.AnchorBottom = 1;

		float marginX = 64 * margin;
		float marginY = 64 * margin;

		highlight.OffsetLeft = marginX;
		highlight.OffsetTop = marginY;
		highlight.OffsetRight = -marginX;
		highlight.OffsetBottom = -marginY;

		// scale the texture to fit perfectly
		highlight.StretchMode = TextureRect.StretchModeEnum.Scale;
	}

	private void AddCoordinateLabel()
	{
		string[] files = { "a","b","c","d","e","f","g","h" };
		string[] ranks = { "8","7","6","5","4","3","2","1" };

		// bottom row → show file letter
		if (Y == 7)
		{
			Label fileLabel = new Label();
			fileLabel.Text = files[X];

			fileLabel.Position = new Vector2(68, 60); // bottom-right corner
			fileLabel.Modulate = new Color(0.2f,0.2f,0.2f);
			fileLabel.MouseFilter = MouseFilterEnum.Ignore;

			AddChild(fileLabel);
		}

		// left column → show rank number
		if (X == 0)
		{
			Label rankLabel = new Label();
			rankLabel.Text = ranks[Y];

			rankLabel.Position = new Vector2(2, 2); // top-left corner
			rankLabel.Modulate = new Color(0.2f,0.2f,0.2f);
			rankLabel.MouseFilter = MouseFilterEnum.Ignore;

			AddChild(rankLabel);
		}
	}
}

public enum HighlightType
{
	Move,    // normal legal move
	Attack   // square occupied by opponent
}
