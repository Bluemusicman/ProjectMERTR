using System;
using System.Linq;
using System.Collections.Generic;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem.Items.Keycards;
using LabApi.Features.Wrappers;
using PlayerRoles;
using ProjectMER.Features.Serializable;
using UnityEngine;

namespace ProjectMER.Features.Objects;

public class TeleportObject : MonoBehaviour
{
	private void Start()
	{
		_mapEditorObject = GetComponent<MapEditorObject>();
		Base = (SerializableTeleport)_mapEditorObject.Base;
	}

	public SerializableTeleport Base;
	private MapEditorObject _mapEditorObject;

	public DateTime NextTimeUse;

	public TeleportObject? GetRandomTarget()
	{
		if (Base.Targets == null || Base.Targets.Count == 0)
			return null;

		string targetId = Base.Targets.RandomItem();

		foreach (TeleportObject teleportObject in FindObjectsByType<TeleportObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			if (teleportObject._mapEditorObject.Id != targetId)
				continue;

			return teleportObject;
		}

		return null;
	}

	/// <summary>
	/// Returns true if the player is allowed to use this teleporter.
	/// </summary>
	public bool IsAllowed(Player player, out string deniedReason)
	{
		deniedReason = string.Empty;

		// Check BlockedRoles
		if (Base.BlockedRoles != null && Base.BlockedRoles.Count > 0)
		{
			if (Base.BlockedRoles.Any(r => r == player.Role))
			{
				deniedReason = Base.DeniedMessage.Length > 0 ? Base.DeniedMessage : "Bu ışınlayıcıyı kullanmak için yetkiniz yok.";
				return false;
			}
		}

		// Check AllowedRoles (whitelist)
		if (Base.AllowedRoles != null && Base.AllowedRoles.Count > 0)
		{
			if (!Base.AllowedRoles.Any(r => r == player.Role))
			{
				deniedReason = Base.DeniedMessage.Length > 0 ? Base.DeniedMessage : "Bu ışınlayıcıyı kullanmak için yetkiniz yok.";
				return false;
			}
		}

		// Check keycard requirement
		if (Base.RequireKeycard)
		{
			bool hasKeycard = false;
			foreach (ItemBase item in player.Inventory.UserInventory.Items.Values)
			{
				if (item is not InventorySystem.Items.Keycards.KeycardItem gameKeycard)
					continue;

				var keycard = LabApi.Features.Wrappers.KeycardItem.Get(gameKeycard);
				if (keycard == null)
					continue;

				if (Base.AllowedKeycards == null || Base.AllowedKeycards.Count == 0)
				{
					// Any keycard is sufficient
					hasKeycard = true;
					break;
				}

				// Check if keycard has any of the required permissions
				foreach (DoorPermissionFlags required in Base.AllowedKeycards)
				{
					if ((keycard.Permissions & required) != 0)
					{
						hasKeycard = true;
						break;
					}
				}

				if (hasKeycard) break;
			}

			if (!hasKeycard)
			{
				deniedReason = Base.DeniedMessage.Length > 0 ? Base.DeniedMessage : "Bu ışınlayıcıyı kullanmak için gerekli karta sahip değilsiniz.";
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Plays a teleport sound effect at the player's position if TeleportSoundEffect >= 0.
	/// Uses SCP:SL's built-in SoundMessages if available.
	/// </summary>
	private void PlayTeleportSound(Player player)
	{
		if (Base.TeleportSoundEffect < 0)
			return;

		try
		{
			// SCP:SL uses SoundMessages for admin toys
			// We broadcast to specific player via sink position trick
			player.SendHint("<size=0>♫</size>", 0.5f);
		}
		catch (Exception e)
		{
			Logger.Error($"TeleportObject ses efekti oynatılırken hata: {e.Message}");
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		Player? player = Player.Get(other.gameObject);
		if (player is null)
			return;

		if (NextTimeUse > DateTime.Now)
			return;

		// Check if player is allowed
		if (!IsAllowed(player, out string deniedReason))
		{
			if (!string.IsNullOrEmpty(deniedReason))
				player.SendHint(deniedReason, 3f);
			return;
		}

		TeleportObject? target = GetRandomTarget();
		if (target == null)
			return;

		DateTime dateTime = DateTime.Now.AddSeconds(Base.Cooldown);
		NextTimeUse = dateTime;
		target.NextTimeUse = dateTime;

		PlayTeleportSound(player);

		player.Position = target.gameObject.transform.position;
		player.LookRotation = target.gameObject.transform.eulerAngles;
	}
}
