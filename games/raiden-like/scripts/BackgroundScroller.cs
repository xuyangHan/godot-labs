using Godot;

public partial class BackgroundScroller : TileMapLayer
{
	[Export]
	public float ScrollSpeed = 80.0f;

	private float _patternHeightPixels;
	private TileMapLayer _secondaryLayer;
	private float _primaryY;
	private float _secondaryY;
	private bool _isInitialized;

	public override void _Ready()
	{
		Rect2I usedRect = GetUsedRect();
		Vector2I usedRectSize = usedRect.Size;
		Vector2I tileSize = TileSet.TileSize;
		_patternHeightPixels = usedRectSize.Y * tileSize.Y;

		if (_patternHeightPixels <= 0.0f)
		{
			GD.PushWarning($"{Name}: Background pattern height is zero. Scrolling disabled.");
			return;
		}

		CallDeferred(nameof(SetupSecondaryLayerDeferred));
	}

	private void SetupSecondaryLayerDeferred()
	{
		_secondaryLayer = new TileMapLayer();
		_secondaryLayer.Name = $"{Name}_loop_copy";
		_secondaryLayer.Set("tile_set", Get("tile_set"));
		_secondaryLayer.Set("tile_map_data", Get("tile_map_data"));
		_secondaryLayer.Set("modulate", Get("modulate"));

		Node parent = GetParent();
		if (parent == null)
		{
			return;
		}

		parent.AddChild(_secondaryLayer);
		_secondaryLayer.Owner = Owner;
		_secondaryLayer.ZIndex = ZIndex - 1;

		_primaryY = 0.0f;
		_secondaryY = -_patternHeightPixels;
		Position = new Vector2(Position.X, _primaryY);
		_secondaryLayer.Position = new Vector2(_secondaryLayer.Position.X, _secondaryY);
		_isInitialized = true;
	}

	public override void _Process(double delta)
	{
		if (_patternHeightPixels <= 0.0f)
		{
			return;
		}
		if (!_isInitialized || _secondaryLayer == null)
		{
			return;
		}

		float deltaY = ScrollSpeed * (float)delta;
		_primaryY += deltaY;
		_secondaryY += deltaY;

		if (_primaryY >= _patternHeightPixels)
		{
			_primaryY -= _patternHeightPixels * 2.0f;
		}
		if (_secondaryY >= _patternHeightPixels)
		{
			_secondaryY -= _patternHeightPixels * 2.0f;
		}

		Position = new Vector2(Position.X, _primaryY);
		_secondaryLayer.Position = new Vector2(_secondaryLayer.Position.X, _secondaryY);
	}
}
