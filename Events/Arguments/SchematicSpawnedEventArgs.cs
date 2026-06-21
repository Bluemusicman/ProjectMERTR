using System;
using ProjectMER.Events.Arguments.Interfaces;
using ProjectMER.Features.Objects;

namespace ProjectMER.Events.Arguments;

/// <summary>
/// Bir şematik nesne yüklendikten sonra tetiklenen olay argümanı.
/// </summary>
public class SchematicSpawnedEventArgs : EventArgs, ISchematicEvent
{
    public SchematicSpawnedEventArgs(SchematicObject schematic, string name)
    {
        Schematic = schematic;
        Name = name;
    }

    public SchematicObject Schematic { get; }

    public string Name { get; }
}
