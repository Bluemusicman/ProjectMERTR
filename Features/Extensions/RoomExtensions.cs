using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using MapGeneration;
using NorthwoodLib.Pools;
using ProjectMER.Features.Serializable;
using UnityEngine;

namespace ProjectMER.Features.Extensions;

public static class RoomExtensions
{
	/// <summary>
	/// Verilen pozisyondaki odayı döndürür.
	/// </summary>
	public static Room GetRoomAtPosition(Vector3 position)
	{
		Room? found = Room.Get(position);
		return found ?? Room.List.First(x => x.Identifier != null && x.Identifier.Name == RoomName.Outside);
	}

	/// <summary>
	/// Odanın benzersiz string kimliğini döndürür (Zone_Shape_Name).
	/// </summary>
	public static string GetRoomStringId(this Room room) =>
		$"{room.Identifier.Zone}_{room.Identifier.Shape}_{room.Identifier.Name}";

	/// <summary>
	/// SerializableObject'e göre eşleşen odaları döndürür.
	/// </summary>
	public static List<Room> GetRooms(this SerializableObject serializableObject)
	{
		string[] split = serializableObject.Room.Split('_');
		if (split.Length != 3)
			return ListPool<Room>.Shared.Rent(Room.List.Where(x => x.Identifier != null && x.Identifier.Name == RoomName.Outside));

		FacilityZone facilityZone = (FacilityZone)Enum.Parse(typeof(FacilityZone), split[0], true);
		RoomShape roomShape = (RoomShape)Enum.Parse(typeof(RoomShape), split[1], true);
		RoomName roomName = (RoomName)Enum.Parse(typeof(RoomName), split[2], true);

		return ListPool<Room>.Shared.Rent(Room.List.Where(x =>
			x.Identifier != null &&
			x.Identifier.Zone == facilityZone &&
			x.Identifier.Shape == roomShape &&
			x.Identifier.Name == roomName));
	}

	/// <summary>
	/// Odanın liste içindeki indeksini döndürür.
	/// </summary>
	public static int GetRoomIndex(this Room room)
	{
		List<Room> list = ListPool<Room>.Shared.Rent(Room.List.Where(x =>
			x.Identifier != null &&
			x.Identifier.Zone == room.Identifier.Zone &&
			x.Identifier.Shape == room.Identifier.Shape &&
			x.Identifier.Name == room.Identifier.Name));
		int index = list.IndexOf(room);
		ListPool<Room>.Shared.Return(list);
		return index;
	}

	public static Vector3 GetAbsolutePosition(this Room? room, Vector3 position)
	{
		if (room is null || room.Identifier.Name == RoomName.Outside)
			return position;

		return room.Transform.TransformPoint(position);
	}

	public static Quaternion GetAbsoluteRotation(this Room? room, Vector3 eulerAngles)
	{
		if (room is null || room.Identifier.Name == RoomName.Outside)
			return Quaternion.Euler(eulerAngles);

		return room.Transform.rotation * Quaternion.Euler(eulerAngles);
	}
}
