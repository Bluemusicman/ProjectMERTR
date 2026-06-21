using CommandSystem;
using Exiled.Permissions.Extensions;
using ProjectMER.Features;

namespace ProjectMER.Commands.Map;

public class Load : ICommand
{
	public string Command => "load";

	public string[] Aliases => ["l"];

	public string Description => "Bir harita yükler";

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

		string mapName = arguments.At(0);

		try
		{
			MapUtils.LoadMap(mapName);
		}
		catch (Exception e)
		{
			response = e.Message;
			return false;
		}

		response = $"{mapName} haritası yüklendi!";
		return true;
	}
}
