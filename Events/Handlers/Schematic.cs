using System;
using ProjectMER.Events.Arguments;

namespace ProjectMER.Events.Handlers;

/// <summary>
/// Şematik olaylarını barındıran statik sınıf.
/// </summary>
public static class Schematic
{
    public static event Action<SchematicSpawningEventArgs>? SchematicSpawning;
    public static event Action<SchematicSpawnedEventArgs>? SchematicSpawned;
    public static event Action<ButtonInteractedEventArgs>? ButtonInteracted;
    public static event Action<SchematicDestroyedEventArgs>? SchematicDestroyed;

    internal static void OnSchematicSpawning(SchematicSpawningEventArgs ev) => SchematicSpawning?.Invoke(ev);
    internal static void OnSchematicSpawned(SchematicSpawnedEventArgs ev) => SchematicSpawned?.Invoke(ev);
    internal static void OnButtonInteracted(ButtonInteractedEventArgs ev) => ButtonInteracted?.Invoke(ev);
    internal static void OnSchematicDestroyed(SchematicDestroyedEventArgs ev) => SchematicDestroyed?.Invoke(ev);
}
