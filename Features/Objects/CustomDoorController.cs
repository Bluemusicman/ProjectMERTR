using System;
using System.Collections.Generic;
using UnityEngine;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Serializable;
using MEC;

namespace ProjectMER.Features.Objects;

public class CustomDoorController : MonoBehaviour
{
	private MapEditorObject _mapEditorObject = null!;
	private DoorVariant _doorVariant = null!;
	private BoxCollider? _teleportTrigger;

	public SerializableDoor Base { get; private set; } = null!;
	public DateTime NextTeleportTime { get; set; }

	public void Init(SerializableDoor serializableDoor)
	{
		Base = serializableDoor;
		_mapEditorObject = GetComponent<MapEditorObject>();
		_doorVariant = GetComponent<DoorVariant>();

		// Auto-Close Delay Setup
		if (Base.AutoCloseDelay > 0)
		{
			_doorVariant.OnStateChanged += OnStateChanged;
		}

		// Teleport Setup
		if (Base.IsTeleport)
		{
			Transform existingTrigger = transform.Find("TeleportTrigger");
			if (existingTrigger != null)
			{
				Destroy(existingTrigger.gameObject);
			}

			GameObject triggerGo = new GameObject("TeleportTrigger");
			triggerGo.transform.SetParent(transform, false);
			triggerGo.transform.localPosition = Vector3.zero;
			triggerGo.transform.localRotation = Quaternion.identity;

			_teleportTrigger = triggerGo.AddComponent<BoxCollider>();
			_teleportTrigger.isTrigger = true;
			_teleportTrigger.size = new Vector3(1.5f, 2.5f, 1f);

			var triggerHandler = triggerGo.AddComponent<DoorTeleportTriggerHandler>();
			triggerHandler.Controller = this;
		}
	}

	private void OnDestroy()
	{
		if (_doorVariant != null && Base != null && Base.AutoCloseDelay > 0)
		{
			_doorVariant.OnStateChanged -= OnStateChanged;
		}
	}

	private void OnStateChanged(DoorVariant door, bool open)
	{
		if (open && Base.AutoCloseDelay > 0)
		{
			Timing.CallDelayed(Base.AutoCloseDelay, () =>
			{
				if (_doorVariant != null && _doorVariant.NetworkTargetState)
				{
					_doorVariant.NetworkTargetState = false;
				}
			});
		}
	}

	public void OnPlayerEnterTrigger(Player player)
	{
		if (!Base.IsTeleport)
			return;

		if (!_doorVariant.NetworkTargetState)
			return;

		if (NextTeleportTime > DateTime.Now)
			return;

		Transform? targetTransform = GetRandomTargetTransform(out float cooldown);
		if (targetTransform == null)
			return;

		DateTime nextTime = DateTime.Now.AddSeconds(cooldown);
		NextTeleportTime = nextTime;

		player.Position = targetTransform.position;
		player.LookRotation = targetTransform.eulerAngles;
	}

	private Transform? GetRandomTargetTransform(out float cooldown)
	{
		cooldown = Base.TeleportCooldown;
		if (Base.TeleportTargets == null || Base.TeleportTargets.Count == 0)
			return null;

		string targetId = Base.TeleportTargets.RandomItem();

		foreach (TeleportObject teleportObject in FindObjectsByType<TeleportObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			if (teleportObject.gameObject.TryGetComponent(out MapEditorObject meo) && meo.Id == targetId)
			{
				teleportObject.NextTimeUse = DateTime.Now.AddSeconds(cooldown);
				return teleportObject.transform;
			}
		}

		foreach (CustomDoorController doorController in FindObjectsByType<CustomDoorController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			if (doorController.gameObject.TryGetComponent(out MapEditorObject meo) && meo.Id == targetId)
			{
				doorController.NextTeleportTime = DateTime.Now.AddSeconds(cooldown);
				return doorController.transform;
			}
		}

		return null;
	}
}

public class DoorTeleportTriggerHandler : MonoBehaviour
{
	public CustomDoorController Controller { get; set; } = null!;

	private void OnTriggerEnter(Collider other)
	{
		Player? player = Player.Get(other.gameObject);
		if (player != null)
		{
			Controller.OnPlayerEnterTrigger(player);
		}
	}
}
