using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Objects;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.ToolGunLike;

public class Delete : ICommand
{
	/// <inheritdoc/>
	public string Command => "delete";

	/// <inheritdoc/>
	public string[] Aliases { get; } = ["del", "remove", "rm"];

	/// <inheritdoc/>
	public string Description => "Baktığınız nesneyi siller.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.{Command}"))
		{
			response = $"Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.{Command}";
			return false;
		}

		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "Bu komut sunucu konsolundan çalıştırılamaz.";
			return false;
		}

		if (arguments.Count > 0)
		{
			string id = arguments.At(0);
			if (ToolGunHandler.TryGetObjectById(id, out MapEditorObject idObject))
			{
				ToolGunHandler.DeleteObject(idObject, player);
				response = "Nesneyi başarıyla sildiniz!";
				return true;
			}

			response = $"{id} ID'sine sahip nesne bulunamadı!";
			return false;
		}

		if (ToolGunHandler.TryGetMapObject(player, out MapEditorObject mapEditorObject))
		{
			ToolGunHandler.DeleteObject(mapEditorObject, player);
			response = "Nesneyi başarıyla sildiniz!";

			return true;
		}

		response = "Herhangi bir Harita Editörü nesnesine bakmıyorsunuz!";
		return false;
	}
}
