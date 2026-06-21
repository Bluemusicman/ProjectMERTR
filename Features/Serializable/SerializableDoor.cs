using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace ProjectMER.Features.Serializable;

public class SerializableDoor : SerializableObject
{
	public DoorType DoorType { get; set; } = DoorType.Lcz;
	public bool IsOpen { get; set; } = false;
	public bool IsLocked { get; set; } = false;
	public DoorPermissionFlags RequiredPermissions { get; set; } = DoorPermissionFlags.None;
	public bool RequireAll { get; set; } = true;
	public float Health { get; set; } = -1f;
	public bool IsDestructible { get; set; } = false;
	public float AutoCloseDelay { get; set; } = -1f;
	public bool IsTeleport { get; set; } = false;
	public List<string> TeleportTargets { get; set; } = [];
	public float TeleportCooldown { get; set; } = 5f;
	public string TeleportSoundEffect { get; set; } = "";

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		DoorVariant doorVariant;
		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;

		if (instance == null)
		{
			doorVariant = GameObject.Instantiate(DoorPrefab);
			if (doorVariant.TryGetComponent(out DoorRandomInitialStateExtension doorRandomInitialStateExtension))
				GameObject.Destroy(doorRandomInitialStateExtension);
		}
		else
		{
			doorVariant = instance.GetComponent<DoorVariant>();
		}

		doorVariant.transform.SetPositionAndRotation(position, rotation);
		doorVariant.transform.localScale = Scale;

		_prevType = DoorType;
		SetupDoor(doorVariant);

		var controller = doorVariant.gameObject.GetComponent<CustomDoorController>() ?? doorVariant.gameObject.AddComponent<CustomDoorController>();
		controller.Init(this);

		NetworkServer.UnSpawn(doorVariant.gameObject);
		NetworkServer.Spawn(doorVariant.gameObject);

		return doorVariant.gameObject;
	}

	public void SetupDoor(DoorVariant doorVariant)
	{
		doorVariant.NetworkTargetState = IsOpen;
		doorVariant.ServerChangeLock(DoorLockReason.SpecialDoorFeature, IsLocked);
		doorVariant.RequiredPermissions = new DoorPermissionsPolicy(RequiredPermissions, RequireAll);
		var breakable = doorVariant.GetComponent<Interactables.Interobjects.BreakableDoor>();
		if (breakable != null && Health > 0)
		{
			breakable.RemainingHealth = Health;
		}
	}

	private DoorVariant DoorPrefab
	{
		get
		{
			DoorVariant prefab = DoorType switch
			{
				DoorType.Lcz => PrefabManager.DoorLcz,
				DoorType.Hcz => PrefabManager.DoorHcz,
				DoorType.Ez => PrefabManager.DoorEz,
				DoorType.Bulkdoor => PrefabManager.DoorHeavyBulk,
				DoorType.Gate => PrefabManager.DoorGate,
				_ => throw new InvalidOperationException(),
			};

			return prefab;
		}
	}

	public override bool RequiresReloading => DoorType != _prevType || base.RequiresReloading;

	internal DoorType _prevType;
}
