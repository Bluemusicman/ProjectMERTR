using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;

namespace ProjectMER.Features.UndoRedo;

public enum UndoRedoActionType
{
	Create,
	Delete,
	Modify
}

public class UndoRedoRecord
{
	public UndoRedoActionType ActionType { get; set; }
	public string MapName { get; set; } = null!;
	public string ObjectId { get; set; } = null!;
	public string ObjectType { get; set; } = null!;
	public string? BeforeStateYaml { get; set; }
	public string? AfterStateYaml { get; set; }
	public string? BeforeObjectId { get; set; }
	public string? BeforeMapName { get; set; }
}

public class TempSnapshot
{
	public string Yaml { get; set; } = null!;
	public string ObjectId { get; set; } = null!;
	public string MapName { get; set; } = null!;
}

public static class UndoRedoManager
{
	private static readonly Dictionary<Player, List<UndoRedoRecord>> UndoStacks = [];
	private static readonly Dictionary<Player, List<UndoRedoRecord>> RedoStacks = [];
	private static readonly Dictionary<string, TempSnapshot> TempSnapshots = [];
	private const int MaxHistory = 20;

	private static string Serialize(object obj)
	{
		return YamlParser.Serializer.Serialize(obj);
	}

	private static object Deserialize(string yaml, Type type)
	{
		return YamlParser.Deserializer.Deserialize(yaml, type);
	}

	public static void Clear(Player player)
	{
		if (UndoStacks.ContainsKey(player)) UndoStacks[player].Clear();
		if (RedoStacks.ContainsKey(player)) RedoStacks[player].Clear();
	}

	public static void SaveBeforeState(string objectId, string mapName, object baseObj)
	{
		TempSnapshots[objectId] = new TempSnapshot
		{
			Yaml = Serialize(baseObj),
			ObjectId = objectId,
			MapName = mapName
		};
	}

	public static void SaveAfterState(Player player, MapEditorObject mapEditorObject)
	{
		if (player == null || mapEditorObject == null) return;
		if (!TempSnapshots.TryGetValue(mapEditorObject.Id, out var temp))
		{
			// If not found in temp under current ID, check if it was saved under the old ID (in case of ID change)
			// We can search the values of TempSnapshots
			foreach (var kvp in TempSnapshots)
			{
				if (kvp.Value.ObjectId != mapEditorObject.Id)
				{
					// If the object matches but ID changed, we use this one
					// (Since we can identify by checking if we are updating this object)
					// But how to identify? We can assume the last snapshot is the one.
					temp = kvp.Value;
					TempSnapshots.Remove(kvp.Key);
					break;
				}
			}

			if (temp == null)
				return;
		}
		else
		{
			TempSnapshots.Remove(mapEditorObject.Id);
		}

		string afterYaml = Serialize(mapEditorObject.Base);
		bool idChanged = temp.ObjectId != mapEditorObject.Id;
		bool mapChanged = temp.MapName != mapEditorObject.MapName;
		bool stateChanged = temp.Yaml != afterYaml;

		if (!stateChanged && !idChanged && !mapChanged)
			return;

		if (!UndoStacks.ContainsKey(player))
			UndoStacks[player] = [];
		if (!RedoStacks.ContainsKey(player))
			RedoStacks[player] = [];

		RedoStacks[player].Clear();

		var record = new UndoRedoRecord
		{
			ActionType = UndoRedoActionType.Modify,
			MapName = mapEditorObject.MapName,
			ObjectId = mapEditorObject.Id,
			ObjectType = mapEditorObject.Base.GetType().AssemblyQualifiedName,
			BeforeStateYaml = temp.Yaml,
			AfterStateYaml = afterYaml,
			BeforeObjectId = idChanged ? temp.ObjectId : null,
			BeforeMapName = mapChanged ? temp.MapName : null
		};

		UndoStacks[player].Add(record);
		if (UndoStacks[player].Count > MaxHistory)
		{
			UndoStacks[player].RemoveAt(0);
		}
	}

	public static void RecordAction(Player player, UndoRedoActionType type, string mapName, string objectId, object? beforeState, object? afterState)
	{
		if (player == null) return;

		if (!UndoStacks.ContainsKey(player))
			UndoStacks[player] = [];
		if (!RedoStacks.ContainsKey(player))
			RedoStacks[player] = [];

		RedoStacks[player].Clear();

		var record = new UndoRedoRecord
		{
			ActionType = type,
			MapName = mapName,
			ObjectId = objectId,
			ObjectType = (beforeState ?? afterState)!.GetType().AssemblyQualifiedName,
			BeforeStateYaml = beforeState != null ? Serialize(beforeState) : null,
			AfterStateYaml = afterState != null ? Serialize(afterState) : null
		};

		UndoStacks[player].Add(record);
		if (UndoStacks[player].Count > MaxHistory)
		{
			UndoStacks[player].RemoveAt(0);
		}
	}

	public static object? CloneState(object? source)
	{
		if (source == null) return null;
		string yaml = Serialize(source);
		return Deserialize(yaml, source.GetType());
	}

	public static bool Undo(Player player, out string response)
	{
		if (player == null)
		{
			response = "Player is null.";
			return false;
		}

		if (!UndoStacks.TryGetValue(player, out var undoStack) || undoStack.Count == 0)
		{
			response = "Geri alınacak son işlem bulunamadı!";
			return false;
		}

		var record = undoStack[undoStack.Count - 1];
		undoStack.RemoveAt(undoStack.Count - 1);

		string currentMapName = record.MapName;
		string currentObjectId = record.ObjectId;
		string targetMapName = record.BeforeMapName ?? record.MapName;
		string targetObjectId = record.BeforeObjectId ?? record.ObjectId;

		if (!MapUtils.LoadedMaps.TryGetValue(currentMapName, out var currentMap))
		{
			response = $"Hata: '{currentMapName}' haritası yüklü değil!";
			return false;
		}

		Type objectType = Type.GetType(record.ObjectType);

		switch (record.ActionType)
		{
			case UndoRedoActionType.Create:
				if (currentMap.TryRemoveElement(currentObjectId))
				{
					currentMap.DestroyObject(currentObjectId);
				}
				break;

			case UndoRedoActionType.Delete:
				var deletedObj = (SerializableObject)Deserialize(record.BeforeStateYaml!, objectType);
				if (MapUtils.LoadedMaps.TryGetValue(targetMapName, out var targetMapDel))
				{
					if (targetMapDel.TryAddElement(targetObjectId, deletedObj))
					{
						targetMapDel.SpawnObject(targetObjectId, deletedObj);
					}
				}
				break;

			case UndoRedoActionType.Modify:
				var beforeObj = (SerializableObject)Deserialize(record.BeforeStateYaml!, objectType);
				currentMap.TryRemoveElement(currentObjectId);
				currentMap.DestroyObject(currentObjectId);
				
				if (MapUtils.LoadedMaps.TryGetValue(targetMapName, out var targetMapMod))
				{
					if (targetMapMod.TryAddElement(targetObjectId, beforeObj))
					{
						targetMapMod.SpawnObject(targetObjectId, beforeObj);
					}
				}
				break;
		}

		if (MapUtils.LoadedMaps.TryGetValue(currentMapName, out var m1)) m1.IsDirty = true;
		if (MapUtils.LoadedMaps.TryGetValue(targetMapName, out var m2)) m2.IsDirty = true;

		if (!RedoStacks.ContainsKey(player))
			RedoStacks[player] = [];
		RedoStacks[player].Add(record);

		response = $"Son işlem geri alındı.";
		return true;
	}

	public static bool Redo(Player player, out string response)
	{
		if (player == null)
		{
			response = "Player is null.";
			return false;
		}

		if (!RedoStacks.TryGetValue(player, out var redoStack) || redoStack.Count == 0)
		{
			response = "İleri alınacak işlem bulunamadı!";
			return false;
		}

		var record = redoStack[redoStack.Count - 1];
		redoStack.RemoveAt(redoStack.Count - 1);

		string currentMapName = record.BeforeMapName ?? record.MapName;
		string currentObjectId = record.BeforeObjectId ?? record.ObjectId;
		string targetMapName = record.MapName;
		string targetObjectId = record.ObjectId;

		if (!MapUtils.LoadedMaps.TryGetValue(currentMapName, out var currentMap))
		{
			response = $"Hata: '{currentMapName}' haritası yüklü değil!";
			return false;
		}

		Type objectType = Type.GetType(record.ObjectType);

		switch (record.ActionType)
		{
			case UndoRedoActionType.Create:
				var createdObj = (SerializableObject)Deserialize(record.AfterStateYaml!, objectType);
				if (MapUtils.LoadedMaps.TryGetValue(targetMapName, out var targetMapCreate))
				{
					if (targetMapCreate.TryAddElement(targetObjectId, createdObj))
					{
						targetMapCreate.SpawnObject(targetObjectId, createdObj);
					}
				}
				break;

			case UndoRedoActionType.Delete:
				if (currentMap.TryRemoveElement(currentObjectId))
				{
					currentMap.DestroyObject(currentObjectId);
				}
				break;

			case UndoRedoActionType.Modify:
				var afterObj = (SerializableObject)Deserialize(record.AfterStateYaml!, objectType);
				currentMap.TryRemoveElement(currentObjectId);
				currentMap.DestroyObject(currentObjectId);
				
				if (MapUtils.LoadedMaps.TryGetValue(targetMapName, out var targetMapMod))
				{
					if (targetMapMod.TryAddElement(targetObjectId, afterObj))
					{
						targetMapMod.SpawnObject(targetObjectId, afterObj);
					}
				}
				break;
		}

		if (MapUtils.LoadedMaps.TryGetValue(currentMapName, out var m1)) m1.IsDirty = true;
		if (MapUtils.LoadedMaps.TryGetValue(targetMapName, out var m2)) m2.IsDirty = true;

		if (!UndoStacks.ContainsKey(player))
			UndoStacks[player] = [];
		UndoStacks[player].Add(record);

		response = $"Son işlem yinelendi.";
		return true;
	}
}
