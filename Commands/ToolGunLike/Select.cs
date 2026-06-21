using CommandSystem;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands.ToolGunLike;

/// <summary>
/// Command used for selecting the objects.
/// </summary>
public class Select : ICommand
{
	/// <inheritdoc/>
	public string Command => "select";

	/// <inheritdoc/>
	public string[] Aliases { get; } = ["sel", "choose"];

	/// <inheritdoc/>
	public string Description => "Baktığınız nesneyi seçer.";

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
				ToolGunHandler.SelectObject(player, idObject);
				response = "Nesneyi başarıyla seçtiniz!";
				return true;
			}

			response = $"{id} ID'sine sahip nesne bulunamadı!";
			return false;
		}

		// Try getting and selecting the object.
		if (ToolGunHandler.TryGetMapObject(player, out MapEditorObject mapEditorObject))
		{
			ToolGunHandler.SelectObject(player, mapEditorObject);
			response = "Nesneyi başarıyla seçtiniz!";
			return true;
		}

		// If object wasn't found deselect currently selected object.
		if (ToolGunHandler.TryGetSelectedMapObject(player, out MapEditorObject _))
		{
			ToolGunHandler.SelectObject(player, null!);
			response = "Nesne seçimini başarıyla kaldırdınız!";
			return false;
		}

		response = "Herhangi bir nesneye bakmıyorsunuz!";
		return false;
	}
}
