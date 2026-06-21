using CommandSystem;
using Exiled.Permissions.Extensions;
using ProjectMER.Features;

namespace ProjectMER.Commands.Map;

public class Save : ICommand
{
	public string Command => "save";

	public string[] Aliases => ["s"];

	public string Description => "Bir haritayı kaydeder";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.{Command}"))
		{
			response = $"Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.{Command}";
			return false;
		}

		if (arguments.Count == 0)
		{
			response = "Bir harita adı belirtmeniz gerekiyor!";
			return false;
		}

		MapUtils.SaveMap(arguments.At(0));

		response = $"{arguments.At(0)} isimli harita başarıyla kaydedildi!";
		return true;
	}
}
