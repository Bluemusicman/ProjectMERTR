using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using InventorySystem.Items.Pickups;
using MapGeneration;
using MapGeneration.Distributors;
using MEC;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using UnityEngine;

// Exiled Locker wrapper
using ExiledLocker = Exiled.API.Features.Lockers.Locker;
using ExiledLockerChamber = Exiled.API.Features.Lockers.LockerChamber;

namespace ProjectMER.Features.Serializable.Lockers;

public class SerializableLocker : SerializableObject
{
	public LockerType LockerType { get; set; } = LockerType.PedestalScp500;

	public List<SerializableLockerLoot> Loot { get; set; } = new();

	public List<SerializableLockerChamber> Chambers { get; set; } = new();

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		Locker locker = instance == null ? UnityEngine.Object.Instantiate(LockerPrefab) : instance.GetComponent<Locker>();
		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;

		locker.transform.SetPositionAndRotation(position, rotation);
		locker.transform.localScale = Scale;

		if (locker.TryGetComponent(out StructurePositionSync structurePositionSync))
		{
			structurePositionSync.Network_position = locker.transform.position;
			structurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(locker.transform.rotation.eulerAngles.y / 5.625f);
		}

		ExiledLocker? exiledLocker = ExiledLocker.Get(locker);
		if (exiledLocker == null)
		{
			NetworkServer.UnSpawn(locker.gameObject);
			NetworkServer.Spawn(locker.gameObject);
			return locker.gameObject;
		}

		if (LockerType != _prevType)
			SetDefaultSettings(exiledLocker);

		// Loot ayarları — Exiled üzerinden locker loot yönetimi
		// (Exiled API'da AddLockerLoot bulunmayabilir; doğrudan base API kullanılıyor)
		foreach (LockerLoot lockerLoot in locker.Loot)
		{
			lockerLoot.RemainingUses = 0;
		}

		foreach (SerializableLockerLoot loot in Loot)
		{
			for (int idx = 0; idx < locker.Loot.Length; idx++)
			{
				if (locker.Loot[idx].TargetItem == loot.TargetItem)
				{
					locker.Loot[idx].RemainingUses = loot.RemainingUses;
					locker.Loot[idx].ProbabilityPoints = loot.ProbabilityPoints;
					locker.Loot[idx].MinPerChamber = loot.MinPerChamber;
					locker.Loot[idx].MaxPerChamber = loot.MaxPerChamber;
					break;
				}
			}
		}

		int i = 0;
		foreach (ExiledLockerChamber chamber in exiledLocker.Chambers)
		{
			if (i > Chambers.Count - 1)
				break;

			chamber.Base.AcceptableItems = Chambers[i].AcceptableItems.ToArray();
			chamber.Base.RequiredPermissions = Chambers[i].RequiredPermissions;
			i++;
		}

		_prevType = LockerType;
		NetworkServer.UnSpawn(locker.gameObject);
		NetworkServer.Spawn(locker.gameObject);

		Timing.CallDelayed(0.25f, () =>
		{
			foreach (ItemPickupBase itemPickupBase in locker.GetComponentsInChildren<ItemPickupBase>())
			{
				if (itemPickupBase.TryGetComponent(out Rigidbody rigidbody))
					rigidbody.isKinematic = false;
			}

			int j = 0;
			foreach (ExiledLockerChamber chamber in exiledLocker.Chambers)
			{
				if (j > Chambers.Count - 1) break;
				chamber.IsOpen = Chambers[j].IsOpen;
				j++;
			}
		});

		return locker.gameObject;
	}

	private void SetDefaultSettings(ExiledLocker exiledLocker)
	{
		Loot.Clear();
		Chambers.Clear();

		foreach (LockerLoot loot in exiledLocker.Base.Loot)
		{
			Loot.Add(new SerializableLockerLoot(loot.TargetItem, loot.RemainingUses, loot.MaxPerChamber, loot.ProbabilityPoints, loot.MinPerChamber));
		}

		foreach (ExiledLockerChamber chamber in exiledLocker.Chambers)
		{
			Chambers.Add(new SerializableLockerChamber(chamber.Base.AcceptableItems, chamber.IsOpen, chamber.Base.RequiredPermissions));
		}
	}

	private Locker LockerPrefab
	{
		get
		{
			Locker prefab = LockerType switch
			{
				LockerType.PedestalScp500 => PrefabManager.PedestalScp500,
				LockerType.LargeGun => PrefabManager.LockerLargeGun,
				LockerType.RifleRack => PrefabManager.LockerRifleRack,
				LockerType.Misc => PrefabManager.LockerMisc,
				LockerType.Medkit => PrefabManager.LockerRegularMedkit,
				LockerType.Adrenaline => PrefabManager.LockerAdrenalineMedkit,
				LockerType.PedestalScp018 => PrefabManager.PedestalScp018,
				LockerType.PedestalScp207 => PrefabManager.PedstalScp207,
				LockerType.PedestalScp244 => PrefabManager.PedestalScp244,
				LockerType.PedestalScp268 => PrefabManager.PedestalScp268,
				LockerType.PedestalScp1853 => PrefabManager.PedstalScp1853,
				LockerType.PedestalScp2176 => PrefabManager.PedestalScp2176,
				LockerType.PedestalScpScp1576 => PrefabManager.PedestalScp1576,
				LockerType.PedestalAntiScp207 => PrefabManager.PedestalAntiScp207,
				LockerType.PedestalScp1344 => PrefabManager.PedestalScp1344,
				LockerType.ExperimentalWeapon => PrefabManager.LockerExperimentalWeapon,
				_ => throw new InvalidOperationException(),
			};

			return prefab;
		}
	}

	public override bool RequiresReloading => true;

	internal LockerType _prevType = LockerType.None;
}
