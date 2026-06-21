using System;
using System.Collections.Generic;
using AdminToys;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using PlayerRoles;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using ProjectMER.Features.Objects;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable;

public class SerializableTeleport : SerializableObject, IIndicatorDefinition
{
	/// <summary>Işınlama hedef ID'leri.</summary>
	public List<string> Targets { get; set; } = [];

	/// <summary>Işınlama bekleme süresi (saniye).</summary>
	public float Cooldown { get; set; } = 5f;

	/// <summary>Bu ışınlayıcıyı kullanabilecek roller. Boşsa herkese açık.</summary>
	public List<RoleTypeId> AllowedRoles { get; set; } = [];

	/// <summary>Bu ışınlayıcıyı kullanamayacak roller.</summary>
	public List<RoleTypeId> BlockedRoles { get; set; } = [];

	/// <summary>Işınlanmak için kart gerekip gerekmediği.</summary>
	public bool RequireKeycard { get; set; } = false;

	/// <summary>Kabul edilen kart yetkileri (RequireKeycard=true iken). Boşsa herhangi bir kart yeterli.</summary>
	public List<DoorPermissionFlags> AllowedKeycards { get; set; } = [];

	/// <summary>Işınlanma anında oynatılacak ses ID'si (SCP:SL SoundID). -1 = ses yok.</summary>
	public int TeleportSoundEffect { get; set; } = -1;

	/// <summary>Işınlanamayan oyuncuya gösterilecek mesaj.</summary>
	public string DeniedMessage { get; set; } = "";

	public override GameObject? SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		GameObject gameObject = instance ?? new GameObject("Teleport");
		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;
		gameObject.transform.SetLocalPositionAndRotation(position, rotation);

		if (instance == null)
			gameObject.AddComponent<TeleportObject>();

		if (!gameObject.TryGetComponent(out BoxCollider boxCollider))
			boxCollider = gameObject.AddComponent<BoxCollider>();

		boxCollider.isTrigger = true;
		boxCollider.size = Scale;

		return gameObject;
	}

	public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
	{
		PrimitiveObjectToy root;
		PrimitiveObjectToy trigger;
		PrimitiveObjectToy arrowY;
		PrimitiveObjectToy arrowX;
		PrimitiveObjectToy arrow;

		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);

		if (instance == null)
		{
			root = UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject);
			root.NetworkPrimitiveFlags = PrimitiveFlags.None;
			root.name = "Indicator";
			root.transform.position = position;

			trigger = UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject);
			trigger.NetworkPrimitiveFlags = PrimitiveFlags.Visible;
			trigger.name = "Trigger";
			trigger.NetworkPrimitiveType = PrimitiveType.Cube;
			trigger.transform.localScale = Scale;
			trigger.transform.position = position;
			trigger.transform.parent = root.transform;

			arrowY = UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject);
			arrowY.NetworkPrimitiveFlags = PrimitiveFlags.None;
			arrowY.name = "Arrow Y Axis";
			arrowY.transform.parent = root.transform;

			arrowX = UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject);
			arrowX.NetworkPrimitiveFlags = PrimitiveFlags.None;
			arrowX.name = "Arrow X Axis";
			arrowX.transform.parent = arrowY.transform;

			arrow = GameObject.Instantiate(PrefabManager.PrimitiveObject, arrowX.transform);
			arrow.transform.localPosition = root.transform.forward;
			arrow.NetworkPrimitiveType = PrimitiveType.Cube;
			arrow.NetworkPrimitiveFlags = PrimitiveFlags.Visible;
			arrow.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
		}
		else
		{
			root = instance.GetComponent<PrimitiveObjectToy>();

			trigger = root.transform.Find("Trigger").GetComponent<PrimitiveObjectToy>();
			arrowY = root.transform.Find("Arrow Y Axis").GetComponent<PrimitiveObjectToy>();
			arrowX = arrowY.transform.Find("Arrow X Axis").GetComponent<PrimitiveObjectToy>();

			trigger.transform.localScale = Scale;
		}

		root.transform.position = position;
		trigger.transform.localEulerAngles = rotation.eulerAngles;
		arrowY.transform.localPosition = Vector3.up * 0.6f;
		arrowY.transform.localEulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
		arrowX.transform.localPosition = Vector3.zero;
		arrowX.transform.localEulerAngles = new Vector3(-rotation.eulerAngles.x, 0f, 0f);

		foreach (PrimitiveObjectToy primitive in root.GetComponentsInChildren<PrimitiveObjectToy>())
		{
			if (Targets.Count > 0)
			{
				primitive.NetworkMaterialColor = new Color(0.11f, 0.98f, 0.92f, 0.5f);
			}
			else
			{
				primitive.NetworkMaterialColor = new Color(1f, 1f, 1f, 0.25f);
			}
		}

		return root.gameObject;
	}
}
