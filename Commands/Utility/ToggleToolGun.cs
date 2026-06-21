using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using ProjectMER.Features.ToolGun;

namespace ProjectMER.Commands;

public class ToggleToolGun : ICommand
{
	public string Command => "toolgun";

	public string[] Aliases => ["tg"];

	public string Description => "Nesneleri oluşturmak ve düzenlemek için Araç Silahı (Tool Gun).";

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

		if (ToolGunItem.Remove(player))
		{
			response = "Artık Araç Silahınız (Tool Gun) yok!";
			return true;
		}

		if (ToolGunItem.TryAdd(player))
		{
			response = "Artık Araç Silahınız (Tool Gun) var!";
			return true;
		}

		response = "Envanteriniz dolu!";
		return false;
	}
}
