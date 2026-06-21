using System;
using Exiled.Events.Interfaces;
using ProjectMER.Features.Serializable.Schematics;

namespace ProjectMER.Events.Arguments;

/// <summary>
/// Bir şematik nesne yüklenirken (spawning) tetiklenen olay argümanı.
/// </summary>
public class SchematicSpawningEventArgs : EventArgs, IDeniableEvent
{
    public SchematicSpawningEventArgs(SchematicObjectDataList data, string name)
    {
        Data = data;
        Name = name;
        IsAllowed = true;
    }

    public SchematicObjectDataList Data { get; set; }

    public string Name { get; }

    public bool IsAllowed { get; set; }
}
