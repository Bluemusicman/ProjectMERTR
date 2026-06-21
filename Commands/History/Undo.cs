using System;
using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features.UndoRedo;

namespace ProjectMER.Commands.History;

public class Undo : ICommand
{
	public string Command => "undo";
	public string[] Aliases { get; } = ["un"];
	public string Description => "Son yapılan işlemi geri alır.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission("mpr.undo"))
		{
			response = "Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.undo";
			return false;
		}

		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "Bu komut sunucu konsolundan çalıştırılamaz.";
			return false;
		}

		return UndoRedoManager.Undo(player, out response);
	}
}
