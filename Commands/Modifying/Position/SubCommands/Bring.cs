using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Objects;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.Modifying.Position.SubCommands;

/// <summary>
/// Modifies object's position by setting it to the sender's current position.
/// </summary>
public class Bring : ICommand
{
	/// <inheritdoc/>
	public string Command => "bring";

	/// <inheritdoc/>
	public string[] Aliases { get; } = [];

	/// <inheritdoc/>
	public string Description => "Nesneyi oyuncunun konumuna getirir.";

	/// <inheritdoc/>
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.position"))
		{
			response = $"Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.position";
			return false;
		}

		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "Bu komut sunucu konsolundan çalıştırılamaz.";
			return false;
		}

		if (!ToolGunHandler.TryGetSelectedMapObject(player, out MapEditorObject mapEditorObject))
		{
			response = "Önce bir nesne seçmelisiniz!";
			return false;
		}

		UndoRedo.UndoRedoManager.SaveBeforeState(mapEditorObject.Id, mapEditorObject.MapName, mapEditorObject.Base);
		mapEditorObject.Base.Position = mapEditorObject.Room.Transform.InverseTransformPoint(player.Position);
		mapEditorObject.UpdateObjectAndCopies();
		UndoRedo.UndoRedoManager.SaveAfterState(player, mapEditorObject);

		response = mapEditorObject.Base.Position.ToString("F3");
		return true;
	}
}