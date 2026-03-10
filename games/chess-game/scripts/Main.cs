using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Control
{
	private Board board = new Board();
	private BoardManager boardManager = new BoardManager();
	private SelectionManager selectionManager;

	public PackedScene squareScene;

	private AudioStreamPlayer moveSound;
	private AudioStreamPlayer captureSound;

	public PieceColor currentTurn = PieceColor.White;

	private PromotionPanel promotionPanel;
	private bool isWaitingForPromotion = false;
	private MoveEntry pendingPromotionMove = null;
	private HistoryManager historyManager;

	private Control gameOverPanel;
	private Label gameOverLabel;
	private bool isGameOver = false;

	public override void _Ready()
	{
		// Load square scene
		squareScene = GD.Load<PackedScene>("res://scenes/Square.tscn");
		
		// Initialize board logic
		board.SetupBoard();

		// Initialize board UI
		var boardUI = GetNode<GridContainer>("Board");
		boardManager.Init(boardUI, squareScene);

		// Initialize selection manager
		selectionManager = new SelectionManager(boardManager);

		// Wire clicks
		foreach (Square sq in GetNode<GridContainer>("Board").GetChildren())
		{
			sq.SquareClicked += (s, button) => OnSquareClicked((Square)s, (string)button);
		}

		moveSound = GetNode<AudioStreamPlayer>("MoveSound");
		captureSound = GetNode<AudioStreamPlayer>("CaptureSound");

		promotionPanel = GetNode<PromotionPanel>("PromotionPanel");
		promotionPanel.PromotionSelected += OnPromotionSelected;

		historyManager = GetNode<HistoryManager>("HistoryManager");

		gameOverPanel = GetNode<Control>("GameOverPanel");
		gameOverLabel = GetNode<Label>("GameOverPanel/GameOverLabel");

		// Refresh initial board
		RefreshBoard();
	}

	void OnSquareClicked(Square square, string button)
	{
		if (isWaitingForPromotion || isGameOver) return;

		if (button == "right")
		{
			selectionManager.ResetSelection();
			return;
		}

		Piece clickedPiece = board.GetPiece(square.X, square.Y);

		// Nothing selected yet
		if (!selectionManager.HasSelection())
		{
			if (clickedPiece != null && clickedPiece.Color == currentTurn)
			{
				selectionManager.SelectSquare(square, board);
			}
			return;
		}

		// Move piece
		var selected = selectionManager.GetSelectedSquare();
		Piece movingPiece = board.GetPiece(selected.X, selected.Y);

		var legalMoves = movingPiece.GetLegalMoves(selected.X, selected.Y, board);
		bool isLegal = legalMoves.Contains((square.X, square.Y));

		if (!isLegal)
		{
			GD.Print("Illegal move");
			selectionManager.ResetSelection();
			return;
		}

		var (moveResult, moveEntry) = board.MovePiece(selected.X, selected.Y, square.X, square.Y);
		
		switch (moveResult)
		{
			case MoveResult.Normal:
				moveSound.Play();
				break;

			case MoveResult.Capture:
			case MoveResult.Castle:
				captureSound.Play();
				break;

			case MoveResult.Promotion:
				captureSound.Play();
				break;
		}

		if(moveResult == MoveResult.Promotion)
		{
			pendingPromotionMove = moveEntry;
			isWaitingForPromotion = true;

			Vector2 pawnScreenPos = square.GlobalPosition;
			promotionPanel.ShowPromotionUI(square.X, square.Y, pawnScreenPos, movingPiece.Color);
			
			// DO NOT swap currentTurn here! Return early.
			RefreshBoard();
			selectionManager.ResetSelection();
			return; 
		}

		historyManager.RecordMove(moveEntry);

		currentTurn = currentTurn == PieceColor.White 
		? PieceColor.Black 
		: PieceColor.White;

		CheckGameOver();
		
		RefreshBoard();
		selectionManager.ResetSelection();
	}
	
	private void OnPromotionSelected(string typeStr)
	{
		if (!Enum.TryParse(typeStr, out PieceType type))
		{
			GD.PrintErr($"Failed to parse PieceType: {typeStr}");
			return;
		}

		// Use the data stored in PromotionPanel
		int x = promotionPanel.promotionX;
		int y = promotionPanel.promotionY;
		PieceColor color = promotionPanel.promotionColor;
		
		board.board[x, y] = new Piece(type, color);

		if (pendingPromotionMove != null)
		{
			GD.Print($"Promoting piece at {x},{y} to {type}");

			pendingPromotionMove.Notation += "=" + typeStr.Substring(0,1).ToUpper();
			historyManager.RecordMove(pendingPromotionMove);
			GD.Print(pendingPromotionMove.Notation);
			pendingPromotionMove = null;
		}

		if (typeStr == "Cat")
		{
			// 1. Perform the "Cat" special effect
			board.ConvertNearbyPieces(x, y, color); 
		}

		currentTurn = currentTurn == PieceColor.White 
		? PieceColor.Black 
		: PieceColor.White;
		
		isWaitingForPromotion = false;
		promotionPanel.HideUI();
		CheckGameOver();
		RefreshBoard();
	}

	void RefreshBoard()
	{
		foreach (Square square in GetNode<GridContainer>("Board").GetChildren())
		{
			square.SetPiece(board.GetPiece(square.X, square.Y));

			square.SetCheckHighlight(false);
		}

		HighlightCheckedKing();
	}

	void HighlightCheckedKing()
	{
		if (board.IsKingInCheck(currentTurn))
		{
			var (kx, ky) = board.FindKing(currentTurn);

			foreach (Square square in GetNode<GridContainer>("Board").GetChildren())
			{
				if (square.X == kx && square.Y == ky)
				{
					square.SetCheckHighlight(true);
					break;
				}
			}
		}
	}

	void CheckGameOver()
	{
		if (board.IsCheckmate(currentTurn))
		{
			string winner = currentTurn == PieceColor.White ? "Black" : "White";
			ShowGameOver($"Checkmate!\n{winner} wins.");
		}
		else if (board.IsStalemate(currentTurn))
		{
			ShowGameOver("Stalemate!\nIt's a draw.");
		}
	}

	void ShowGameOver(string message)
	{
		isGameOver = true;
		gameOverLabel.Text = message;
		gameOverPanel.Visible = true;
	}
}
