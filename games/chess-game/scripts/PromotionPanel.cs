using Godot;

public partial class PromotionPanel : Control
{

	[Signal]
	public delegate void PromotionSelectedEventHandler(string type);

	public int promotionX;
	public int promotionY; 
	public PieceColor promotionColor;

	public void ShowPromotionUI(int x, int y, PieceColor color)
	{
		promotionX = x;
		promotionY = y;
		promotionColor = color;

		ZIndex = 100;
		Visible = true;

		GD.Print("ShowPromotionUI");
	}

	public void HideUI() => Visible = false;

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
