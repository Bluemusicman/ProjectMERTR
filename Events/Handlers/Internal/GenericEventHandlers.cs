using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable;
using ProjectMER.Features.ToolGun;
using PlayerHandlers = Exiled.Events.Handlers.Player;
using ServerHandlers = Exiled.Events.Handlers.Server;

namespace ProjectMER.Events.Handlers.Internal;

/// <summary>
/// Genel sunucu ve oyuncu olaylarını yöneten işleyici sınıfı.
/// Exiled 9.14.2 sürümünde += / -= abonelik yöntemi kullanılır.
/// </summary>
public class GenericEventsHandler
{
    /// <summary>
    /// Exiled olaylarına abone olur.
    /// </summary>
    public void Kaydet()
    {
        ServerHandlers.WaitingForPlayers   += OnWaitingForPlayers;
        PlayerHandlers.Spawning            += OnPlayerSpawning;
        PlayerHandlers.InteractingShootingTarget += OnPlayerInteractingShootingTarget;
    }

    /// <summary>
    /// Exiled olaylarından aboneliği kaldırır.
    /// </summary>
    public void Kaldir()
    {
        ServerHandlers.WaitingForPlayers   -= OnWaitingForPlayers;
        PlayerHandlers.Spawning            -= OnPlayerSpawning;
        PlayerHandlers.InteractingShootingTarget -= OnPlayerInteractingShootingTarget;
    }

    // ---------- Olay işleyicileri ----------

    private void OnWaitingForPlayers()
    {
        // Oyuncu bekleme ekranına geçildiğinde tüm önbellekler temizlenir
        PrefabManager.RegisterPrefabs();

        MapUtils.LoadedMaps.Clear();
        ToolGunItem.ItemDictionary.Clear();
        ToolGunHandler.PlayerSelectedObjectDict.Clear();
        PickupEventsHandler.ButtonPickups.Clear();
        PickupEventsHandler.PickupUsesLeft.Clear();
    }

    private void OnPlayerSpawning(SpawningEventArgs ev)
    {

        // Harita şematiklerindeki özel doğuş noktaları kontrol edilir
        List<MapEditorObject> liste = [];
        foreach (MapSchematic harita in MapUtils.LoadedMaps.Values)
        {
            foreach (KeyValuePair<string, SerializablePlayerSpawnpoint> noktaKvp in harita.PlayerSpawnpoints)
            {
                if (!noktaKvp.Value.Roles.Contains(ev.Player.Role.Type))
                    continue;

                liste.AddRange(harita.SpawnedObjects.Where(x => x.Id == noktaKvp.Key));
            }
        }

        if (liste.Count == 0)
            return;

        // Uygun noktalardan rastgele biri seçilir
        MapEditorObject rastgeleNesne = liste[UnityEngine.Random.Range(0, liste.Count)];

        ev.Position = rastgeleNesne.transform.position;
        Timing.CallDelayed(0.05f, () =>
        {
            try
            {
                ev.Player.Rotation = UnityEngine.Quaternion.Euler(rastgeleNesne.transform.eulerAngles);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        });
    }

    private void OnPlayerInteractingShootingTarget(InteractingShootingTargetEventArgs ev)
    {
        // Harita nesnesi olan atış hedefleriyle etkileşim engellenir
        if (ev.ShootingTarget.Base.gameObject.TryGetComponent(out MapEditorObject _))
            ev.IsAllowed = false;
    }
}
