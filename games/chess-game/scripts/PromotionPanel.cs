using Godot;

public partial class PromotionPanel : Control
{

	[Signal]
	public delegate void PromotionSelectedEventHandler(string type);

	[Export] public Control buttonContainer;
	[Export] public ColorRect backgroundOverlay;

	public int promotionX;
	public int promotionY; 
	public PieceColor promotionColor;

	public override void _Ready()
	{
		// If you didn't link it in the Inspector, we try to find it by name
		buttonContainer ??= GetNode<Control>("HBoxContainer");
		backgroundOverlay ??= GetNode<ColorRect>("Background");
		
		HideUI();
	}

	public void ShowPromotionUI(int x, int y, Vector2 screenPos, PieceColor color)
	{
		// Force the panel to ignore parent size and fill the screen
		SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		
		promotionX = x;
		promotionY = y;
		promotionColor = color;

		// 1. Keep the main panel at (0,0) so the background covers everything
		GlobalPosition = Vector2.Zero; 
		
		// 2. Position ONLY the buttons near the pawn
		float yOffset = (color == PieceColor.White) ? 80f : -50f;
		
		// Centering logic: subtract half the width of the container 
		// so it's centered horizontally over the square
		Vector2 centerOffset = new Vector2(buttonContainer.Size.X / 2, 0);
		buttonContainer.GlobalPosition = screenPos + new Vector2(0, yOffset) - centerOffset;

		Visible = true;
		backgroundOverlay.Visible = true;
	}

	public void HideUI()
	{
		Visible = false;
		if (backgroundOverlay != null)
			backgroundOverlay.Visible = false;
	}

	public void OnQueenPressed()
	{
		EmitSignal(SignalName.PromotionSelected, "Queen");
		HideUI();
	}

	public void OnRookPressed()
	{
		EmitSignal(SignalName.PromotionSelected, "Rook");
		HideUI();
	}

	public void OnBishopPressed()
	{
		EmitSignal(SignalName.PromotionSelected, "Bishop");
		HideUI();
	}

	public void OnKnightPressed()
	{
		EmitSignal(SignalName.PromotionSelected, "Knight");
		HideUI();
	}
}
