using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using ProjectMER.Features.Objects;
using PlayerHandlers = Exiled.Events.Handlers.Player;
using FirearmPickup = Exiled.API.Features.Pickups.FirearmPickup;

namespace ProjectMER.Events.Handlers.Internal;

/// <summary>
/// Eşya alıp bırakma ve özel buton mekanizmalarını yöneten işleyici sınıfı.
/// Exiled 9.14.2 sürümünde += / -= abonelik yöntemi kullanılır.
/// </summary>
public class PickupEventsHandler
{
    /// <summary>
    /// Buton olarak tanımlanan eşyaların seri → şematik eşleştirmesi.
    /// </summary>
    internal static readonly Dictionary<ushort, SchematicObject> ButtonPickups = [];

    /// <summary>
    /// Sınırlı sayıda alınabilir eşyaların kalan kullanım sayısı.
    /// </summary>
    internal static readonly Dictionary<ushort, int> PickupUsesLeft = [];

    /// <summary>
    /// Exiled olaylarına abone olur.
    /// </summary>
    public void Kaydet()
    {
        PlayerHandlers.SearchingPickup  += OnPlayerSearchingPickup;
        PlayerHandlers.PickingUpItem    += OnPlayerPickingUpItem;
    }

    /// <summary>
    /// Exiled olaylarından aboneliği kaldırır.
    /// </summary>
    public void Kaldir()
    {
        PlayerHandlers.SearchingPickup  -= OnPlayerSearchingPickup;
        PlayerHandlers.PickingUpItem    -= OnPlayerPickingUpItem;
    }

    // ---------- Olay işleyicileri ----------

    private void OnPlayerSearchingPickup(SearchingPickupEventArgs ev)
    {
        // Butona basma mekanizması — özel eşyayla etkileşim şematik olayını tetikler
        if (!ButtonPickups.TryGetValue(ev.Pickup.Serial, out SchematicObject sematik))
            return;

        ev.IsAllowed = false;
        Schematic.OnButtonInteracted(new(ev.Pickup, ev.Player, sematik));
    }

    private void OnPlayerPickingUpItem(PickingUpItemEventArgs ev)
    {
        // Harita nesnesinin çocuğu olan eşya mı?
        if (!ev.Pickup.Transform.TryGetComponentInParent(out MapEditorObject _))
            return;

        if (!PickupUsesLeft.ContainsKey(ev.Pickup.Serial))
            return;

        if (--PickupUsesLeft[ev.Pickup.Serial] == 0)
        {
            PickupUsesLeft.Remove(ev.Pickup.Serial);
            return;
        }

        // Kalan kullanım hakkı varsa yeniden bırak ve envantere ekle
        ev.IsAllowed = false;
        ev.Pickup.InUse = false;

        if (ev.Pickup is AmmoPickup ammoPickup)
        {
            ev.Player.AddAmmo(ammoPickup.AmmoType, ammoPickup.Ammo);
        }
        else
        {
            Item? esya = ev.Player.AddItem(ev.Pickup.Type);
            if (ev.Pickup is not FirearmPickup silahYeri || esya is not Firearm silah)
                return;

            // Ateşli silahın ek parça kodunu ve mermisini ayarla
            silah.Base.ApplyAttachmentsCode(silahYeri.Attachments, false);
            if (silah.Base.TryGetModule(out MagazineModule sarjorModulu))
            {
                sarjorModulu.MagazineInserted = true;
                sarjorModulu.AmmoStored = sarjorModulu.AmmoMax;
                sarjorModulu.ServerResyncData();
            }
            else if (silah.Base.TryGetModule(out CylinderAmmoModule silindirModulu))
            {
                silindirModulu.ServerModifyAmmo(silindirModulu.AmmoMax);
                silindirModulu.ServerResync();
            }
        }
    }
}
