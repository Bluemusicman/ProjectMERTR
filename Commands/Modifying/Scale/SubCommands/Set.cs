using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Objects;
using ProjectMER.Features.ToolGun;
using UnityEngine;
using static ProjectMER.Features.Extensions.StructExtensions;

namespace ProjectMER.Commands.Modifying.Scale.SubCommands;

public class Set : ICommand
{
	/// <inheritdoc/>
	public string Command => "set";

	/// <inheritdoc/>
	public string[] Aliases { get; } = [];

	/// <inheritdoc/>
	public string Description => string.Empty;

	/// <inheritdoc/>
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.scale"))
		{
			response = $"Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.scale";
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

		if (arguments.Count >= 3 && TryGetVector(arguments.At(0), arguments.At(1), arguments.At(2), out Vector3 newScale))
		{
			UndoRedo.UndoRedoManager.SaveBeforeState(mapEditorObject.Id, mapEditorObject.MapName, mapEditorObject.Base);
			mapEditorObject.Base.Scale = newScale;
			mapEditorObject.UpdateObjectAndCopies();
			UndoRedo.UndoRedoManager.SaveAfterState(player, mapEditorObject);

			response = mapEditorObject.Base.Scale.ToString("F3");
			return true;
		}

		response = "Geçersiz değerler.";
		return false;
	}
}