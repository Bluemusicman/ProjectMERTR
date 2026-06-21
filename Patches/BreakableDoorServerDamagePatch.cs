using HarmonyLib;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;

namespace ProjectMER.Patches;

[HarmonyPatch(typeof(BreakableDoor), nameof(BreakableDoor.ServerDamage))]
public static class BreakableDoorServerDamagePatch
{
	public static bool Prefix(BreakableDoor __instance, ref float damage, ref DoorDamageType type)
	{
		if (__instance.gameObject.TryGetComponent(out MapEditorObject mapEditorObject) && 
			mapEditorObject.Base is SerializableDoor serializableDoor)
		{
			if (!serializableDoor.IsDestructible)
			{
				return false;
			}

			type = DoorDamageType.Grenade;
		}
		return true;
	}
}
