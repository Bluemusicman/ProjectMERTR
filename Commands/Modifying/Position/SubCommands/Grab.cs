using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.ToolGun;
using UnityEngine;

namespace ProjectMER.Commands.Modifying.Position.SubCommands;

/// <summary>
/// Grabs a specific <see cref="MapEditorObject"/>.
/// </summary>
public class Grab : ICommand
{
	public string Command => "grab";

	public string[] Aliases => [];

	public string Description => "Bir nesneyi tutar.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.HasAnyPermission($"mpr.position"))
		{
			response = $"Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.position";
			return false;
		}

		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "Bu komut sunucu konsolundan çalıştırılamaz.";
			return false;
		}

		if (!ToolGunHandler.TryGetSelectedMapObject(player, out MapEditorObject mapEditorObject))
		{
			response = "Önce bir nesne seçmelisiniz!";
			return false;
		}

		if (GrabbingPlayers.ContainsKey(player))
		{
			Timing.KillCoroutines(GrabbingPlayers[player]);
			GrabbingPlayers.Remove(player);

			Room room = mapEditorObject.Room;
			mapEditorObject.Base.Position = room.Name == MapGeneration.RoomName.Outside ? mapEditorObject.transform.position : mapEditorObject.Room.Transform.InverseTransformPoint(mapEditorObject.transform.position);
			mapEditorObject.UpdateObjectAndCopies();
			UndoRedo.UndoRedoManager.SaveAfterState(player, mapEditorObject);

			response = "Bırakıldı";
			return true;
		}

		UndoRedo.UndoRedoManager.SaveBeforeState(mapEditorObject.Id, mapEditorObject.MapName, mapEditorObject.Base);
		GrabbingPlayers.Add(player, Timing.RunCoroutine(GrabbingCoroutine(player, mapEditorObject)));

		response = "Tutuldu";
		return true;
	}

	private IEnumerator<float> GrabbingCoroutine(Player player, MapEditorObject mapEditorObject)
	{
		Vector3 position = player.Camera.position;
		float multiplier = Vector3.Distance(position, mapEditorObject.transform.position);
		Vector3 prevPos = position + (player.Camera.forward * multiplier);

		while (true)
		{
			yield return Timing.WaitForSeconds(0.1f);

			if (mapEditorObject == null || !ToolGunHandler.TryGetSelectedMapObject(player, out _))
				break;

			Vector3 newPos = mapEditorObject.transform.position = player.Camera.position + (player.Camera.forward * multiplier);

			if (prevPos == newPos)
				continue;

			prevPos = newPos;
			mapEditorObject.transform.position = prevPos;
			if (mapEditorObject.Base is SerializableDoor _)
			{
				NetworkServer.UnSpawn(mapEditorObject.gameObject);
				NetworkServer.Spawn(mapEditorObject.gameObject);
			}
		}

		GrabbingPlayers.Remove(player);
		if (mapEditorObject != null)
		{
			mapEditorObject.Base.Position = mapEditorObject.Room.Transform.InverseTransformPoint(mapEditorObject.transform.position);
			mapEditorObject.UpdateObjectAndCopies();
			UndoRedo.UndoRedoManager.SaveAfterState(player, mapEditorObject);
		}
	}

	/// <summary>
	/// The <see cref="Dictionary{TKey, TValue}"/> which contains all <see cref="Player"/> and <see cref="CoroutineHandle"/> pairs.
	/// </summary>
	private static readonly Dictionary<Player, CoroutineHandle> GrabbingPlayers = [];
}
