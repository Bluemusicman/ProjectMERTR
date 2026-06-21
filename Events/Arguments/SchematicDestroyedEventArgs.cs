using ProjectMER.Events.Arguments.Interfaces;
using ProjectMER.Features.Objects;

namespace ProjectMER.Events.Arguments;

/// <summary>
/// Bir şematik nesne yok edildiğinde tetiklenen olay argümanı.
/// </summary>
public class SchematicDestroyedEventArgs : EventArgs, ISchematicEvent
{
    public SchematicDestroyedEventArgs(SchematicObject sematik, string ad)
    {
        Schematic = sematik;
        Name      = ad;
    }

    /// <summary>
    /// Yok edilen şematik nesne.
    /// </summary>
    public SchematicObject Schematic { get; }

    /// <summary>
    /// Şematiğin adı.
    /// </summary>
    public string Name { get; }
}
