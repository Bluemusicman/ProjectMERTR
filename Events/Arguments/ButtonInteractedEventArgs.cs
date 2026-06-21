using System;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using ProjectMER.Events.Arguments.Interfaces;
using ProjectMER.Features.Objects;

namespace ProjectMER.Events.Arguments;

/// <summary>
/// Bir oyuncu butona dokunduğunda tetiklenen olay argümanı.
/// </summary>
public class ButtonInteractedEventArgs : EventArgs, ISchematicEvent
{
    public ButtonInteractedEventArgs(Pickup button, Player player, SchematicObject schematic)
    {
        Button    = button;
        Player    = player;
        Schematic = schematic;
    }

    /// <summary>
    /// Tıklanan eşya (buton).
    /// </summary>
    public Pickup Button { get; }

    /// <summary>
    /// Etkileşimde bulunan oyuncu.
    /// </summary>
    public Player Player { get; }

    /// <summary>
    /// İlgili şematik nesne.
    /// </summary>
    public SchematicObject Schematic { get; }
}
