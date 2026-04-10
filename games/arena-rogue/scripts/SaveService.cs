using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

public partial class SaveService : Node
{
	public const int SlotCount = 3;

	private const string SavesDir = "user://saves";
	private const string MetaFile = $"{SavesDir}/meta.json";

	private static SaveService _instance;
	public static SaveService Instance => _instance;

	public override void _EnterTree()
	{
		_instance = this;
	}

	public bool CanContinue()
	{
		if (!FileAccess.FileExists(MetaFile))
			return false;

		using var file = FileAccess.Open(MetaFile, FileAccess.ModeFlags.Read);
		if (file == null)
			return false;

		var text = file.GetAsText();
		if (string.IsNullOrWhiteSpace(text))
			return false;

		SaveMetaData meta;
		try
		{
			meta = JsonSerializer.Deserialize<SaveMetaData>(text);
		}
		catch (JsonException)
		{
			return false;
		}

		if (meta?.LastSlot is not int slot)
			return false;

		if (slot < 0 || slot >= SlotCount)
			return false;

		return SlotExists(slot);
	}

	public static string GetSlotPath(int slot) => $"{SavesDir}/slot_{slot}.json";

	public bool SlotExists(int slot)
	{
		if (slot < 0 || slot >= SlotCount)
			return false;

		return FileAccess.FileExists(GetSlotPath(slot));
	}

	private sealed class SaveMetaData
	{
		[JsonPropertyName("last_slot")]
		public int? LastSlot { get; set; }
	}
}
