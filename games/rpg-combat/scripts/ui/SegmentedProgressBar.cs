using System;
using Godot;

/// <summary>
/// A progress bar built from three segments: left cap + stretchable mid + right cap.
/// Back (track): barBack_horizontalLeft + barBack_horizontalMid + barBack_horizontalRight for full length.
/// Fill: barGreen (or barBlue) left + mid + right; fill width = (value / max_value) * full width.
/// </summary>
public partial class SegmentedProgressBar : Control
{
	[Export] public Texture2D BackLeft { get; set; }
	[Export] public Texture2D BackMid { get; set; }
	[Export] public Texture2D BackRight { get; set; }
	[Export] public Texture2D FillLeft { get; set; }
	[Export] public Texture2D FillMid { get; set; }
	[Export] public Texture2D FillRight { get; set; }

	private double _value = 100;
	private double _maxValue = 100;

	private HBoxContainer _backHBox;
	private Control _fillClip;
	private HBoxContainer _fillHBox;

	[Export] public double Value
	{
		get => _value;
		set { _value = value; UpdateFillWidth(); }
	}

	[Export] public double MaxValue
	{
		get => _maxValue;
		set { _maxValue = value; UpdateFillWidth(); }
	}

	public override void _Ready()
	{
		BuildBar();
		UpdateFillWidth();
	}

	public override void _Notification(int what)
	{
		if (what == NotificationResized)
			UpdateFillWidth();
	}

	private void BuildBar()
	{
		// Back bar: full width, left + mid (stretch) + right, no gap between segments
		_backHBox = new HBoxContainer();
		_backHBox.AddThemeConstantOverride("separation", 0);
		_backHBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_backHBox.LayoutMode = 1; // Anchors
		AddChild(_backHBox);

		AddBarSegment(_backHBox, BackLeft, BackMid, BackRight);

		// Fill bar: clipped to (value/max)*width, same structure
		_fillClip = new Control();
		_fillClip.LayoutMode = 0; // Position (manual size)
		_fillClip.ClipChildren = ClipChildrenMode.AndDraw;
		AddChild(_fillClip);

		_fillHBox = new HBoxContainer();
		_fillHBox.AddThemeConstantOverride("separation", 0);
		_fillHBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_fillHBox.LayoutMode = 1; // Anchors
		_fillClip.AddChild(_fillHBox);

		AddBarSegment(_fillHBox, FillLeft, FillMid, FillRight);

		// Use texture height for bar height if we have one
		if (BackLeft != null)
		{
			var h = (int)BackLeft.GetSize().Y;
			CustomMinimumSize = new Vector2(0, h);
		}
	}

	private static void AddBarSegment(HBoxContainer box, Texture2D left, Texture2D mid, Texture2D right)
	{
		int leftW = left != null ? (int)left.GetSize().X : 4;
		int rightW = right != null ? (int)right.GetSize().X : 4;

		var texLeft = new TextureRect();
		texLeft.Texture = left;
		texLeft.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
		texLeft.StretchMode = TextureRect.StretchModeEnum.Scale;
		texLeft.CustomMinimumSize = new Vector2(leftW, 0);
		box.AddChild(texLeft);

		var texMid = new TextureRect();
		texMid.Texture = mid;
		texMid.ExpandMode = (TextureRect.ExpandModeEnum)0; // First value = scale/stretch to fill
		texMid.StretchMode = TextureRect.StretchModeEnum.Scale;
		texMid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		box.AddChild(texMid);

		var texRight = new TextureRect();
		texRight.Texture = right;
		texRight.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
		texRight.StretchMode = TextureRect.StretchModeEnum.Scale;
		texRight.CustomMinimumSize = new Vector2(rightW, 0);
		box.AddChild(texRight);
	}

	private void UpdateFillWidth()
	{
		if (_fillClip == null) return;

		float w = Size.X;
		float h = Size.Y;
		if (w <= 0 || _maxValue <= 0) return;

		double ratio = Math.Clamp(_value / _maxValue, 0.0, 1.0);
		int fillW = (int)(w * ratio);

		_fillClip.Position = new Vector2(0, 0);
		_fillClip.Size = new Vector2(fillW, h);
	}
}
