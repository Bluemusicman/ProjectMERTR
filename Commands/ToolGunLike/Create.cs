using System.Text;
using CommandSystem;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using NorthwoodLib.Pools;
using ProjectMER.Configs;
using ProjectMER.Features;
using ProjectMER.Features.Enums;
using ProjectMER.Features.ToolGun;
using UnityEngine;
using static ProjectMER.Features.Extensions.StructExtensions;

namespace ProjectMER.Commands.ToolGunLike;

public class Create : ICommand
{
	/// <inheritdoc/>
	public string Command => "create";

	/// <inheritdoc/>
	public string[] Aliases { get; } = ["cr", "spawn"];

	/// <inheritdoc/>
	public string Description => "Baktığınız noktada seçilen nesneyi oluşturur.";

	/// <inheritdoc/>
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.CheckPermission($"mpr.{Command}"))
		{
			response = $"Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.{Command}";
			return false;
		}

		Player? player = Player.Get(sender)!;

		if (arguments.Count == 0)
		{
			StringBuilder sb = StringBuilderPool.Shared.Rent();
			sb.AppendLine();
			sb.Append("Oluşturulabilir tüm nesnelerin listesi:");
			sb.AppendLine();
			sb.AppendLine();
			foreach (ToolGunObjectType objectType in ToolGunItem.TypesDictionary.Keys)
			{
				if (objectType == ToolGunObjectType.Schematic)
					continue;

				sb.Append($"- {objectType} ({(int)objectType})");
				sb.AppendLine();
			}

			sb.AppendLine();
			sb.Append("Özel bir şematik oluşturmak için lütfen argüman olarak dosya adını kullanın.");

			response = StringBuilderPool.Shared.ToStringReturn(sb);
			return true;
		}

		Vector3 position = Vector3.zero;
		if (arguments.Count >= 4 && !TryGetVector(arguments.At(1), arguments.At(2), arguments.At(3), out position))
		{
			response = "Invalid arguments. Usage: mp create <object> <posX> <posY> <posZ>";
			return false;
		}

		if (arguments.Count == 1)
		{
			if (!ToolGunHandler.Raycast(player, out RaycastHit hit))
			{
				response = "Nesnenin oluşturulabileceği geçerli bir yüzey bulunamadı!";
				return false;
			}

			position = hit.point;
		}
		else if (arguments.Count < 4)
		{
			response = "Invalid arguments. Usage: mp create <object> optionally: <posX> <posY> <posZ>";
			return false;
		}

		string objectName = arguments.At(0);

		if (Enum.TryParse(objectName, true, out ToolGunObjectType parsedEnum) && Enum.IsDefined(typeof(ToolGunObjectType), parsedEnum))
		{
			ToolGunHandler.CreateObject(position, parsedEnum);
			if (Config.AutoSelect && player is not null)
				ToolGunHandler.SelectObject(player, MapUtils.UntitledMap.SpawnedObjects.Last());

			response = $"{objectName} başarıyla oluşturuldu!";
			return true;
		}

		try
		{
			_ = MapUtils.GetSchematicDataByName(objectName);
		}
		catch (Exception e)
		{
			response = e.Message.ToString();
			return false;
		}

		ToolGunHandler.CreateObject(position, ToolGunObjectType.Schematic, objectName);
		if (Config.AutoSelect && player is not null)
			ToolGunHandler.SelectObject(player, MapUtils.UntitledMap.SpawnedObjects.LastOrDefault());

		response = $"{objectName} başarıyla oluşturuldu!";
		return true;
	}

	private static Config Config => ProjectMER.Singleton.Config!;
}
