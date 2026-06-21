using System;
using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features.UndoRedo;

namespace ProjectMER.Commands.History;

public class Redo : ICommand
{
	public string Command => "redo";
	public string[] Aliases { get; } = ["re"];
	public string Description => "Geri alınan son işlemi yineler.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission("mpr.redo"))
		{
			response = "Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.redo";
			return false;
		}

		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "Bu komut sunucu konsolundan çalıştırılamaz.";
			return false;
		}

		return UndoRedoManager.Redo(player, out response);
	}
}
