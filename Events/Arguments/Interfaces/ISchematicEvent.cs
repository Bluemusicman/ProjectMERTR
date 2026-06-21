using ProjectMER.Features.Objects;

namespace ProjectMER.Events.Arguments.Interfaces;

/// <summary>
/// Şematik nesnesi içeren olaylar için arayüz.
/// </summary>
public interface ISchematicEvent
{
    public SchematicObject Schematic { get; }
}
