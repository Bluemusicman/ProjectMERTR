using CommandSystem;
using LabApi.Features.Permissions;
using ProjectMER.Features.Objects;

namespace ProjectMER.Commands.Utility;

public class Indicators : ICommand
{
	public string Command => "indicators";

	public string[] Aliases => ["i", "si"];

	public string Description => "Görünmez nesneler için göstergeleri gösterir.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.{Command}"))
		{
			response = $"Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.{Command}";
			return false;
		}

		if (IndicatorObject.Dictionary.Count > 0)
		{
			IndicatorObject.ClearIndicators();
			response = "Tüm göstergeler kaldırıldı!";
			return true;
		}

		IndicatorObject.RefreshIndicators();
		response = "Göstergeler gösterildi!";
		return true;
	}
}
