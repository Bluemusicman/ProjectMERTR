using CommandSystem;
using LabApi.Features.Permissions;
using NorthwoodLib.Pools;
using ProjectMER.Features;
using ProjectMER.Features.Serializable;

namespace ProjectMER.Commands.Utility;

public class Merge : ICommand
{
	/// <inheritdoc/>
	public string Command => "merge";

	/// <inheritdoc/>
	public string[] Aliases { get; } = [];

	/// <inheritdoc/>
	public string Description => "İki veya daha fazla haritayı tek bir haritada birleştirir.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.{Command}"))
		{
			response = $"Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.{Command}";
			return false;
		}

		if (arguments.Count < 3)
		{
			response = "\nUsage:\n" +
				"mp merge outputMapName inputMap1 inputMap2 [inputMap3 ...]";

			return false;
		}

		List<MapSchematic> maps = ListPool<MapSchematic>.Shared.Rent();

		for (int i = 1; i < arguments.Count; i++)
		{
			MapSchematic map = MapUtils.GetMapData(arguments.At(i));

			if (map is null)
			{
				response = $"{arguments.At(i)} adlı harita mevcut değil veya geçersiz!";

				ListPool<MapSchematic>.Shared.Return(maps);
				return false;
			}

			maps.Add(map);
		}

		string mapName = arguments.At(0);
		MapSchematic outputMap = new(mapName);
		foreach (MapSchematic map in maps)
		{
			outputMap.Merge(map);
		}

		ListPool<MapSchematic>.Shared.Return(maps);

		string path = Path.Combine(ProjectMER.MapsDir, $"{mapName}.yml");
		File.WriteAllText(path, YamlParser.Serializer.Serialize(outputMap));

		response = $"Tüm haritalar ({arguments.Count - 1} adet) başarıyla tek bir haritada birleştirildi!";
		return true;
	}
}
