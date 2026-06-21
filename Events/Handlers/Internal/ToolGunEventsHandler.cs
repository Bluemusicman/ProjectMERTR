using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Objects;
using ProjectMER.Features.ToolGun;
using PlayerHandlers = Exiled.Events.Handlers.Player;
using ServerHandlers = Exiled.Events.Handlers.Server;

namespace ProjectMER.Events.Handlers.Internal;

/// <summary>
/// ToolGun (Araç Silahı) mekanizmalarını ve GUI'yi yöneten işleyici sınıfı.
/// Exiled 9.14.2 sürümünde += / -= abonelik yöntemi kullanılır.
/// </summary>
public class ToolGunEventsHandler
{
    private static CoroutineHandle _toolGunCoroutine;

    /// <summary>
    /// Exiled olaylarına abone olur.
    /// </summary>
    public void Kaydet()
    {
        ServerHandlers.RoundStarted        += OnRoundStarted;
        PlayerHandlers.DryFiringWeapon     += OnPlayerDryFiringWeapon;
        PlayerHandlers.ReloadingWeapon     += OnPlayerReloadingWeapon;
        PlayerHandlers.DroppingItem        += OnPlayerDroppingItem;
    }

    /// <summary>
    /// Exiled olaylarından aboneliği kaldırır.
    /// </summary>
    public void Kaldir()
    {
        ServerHandlers.RoundStarted        -= OnRoundStarted;
        PlayerHandlers.DryFiringWeapon     -= OnPlayerDryFiringWeapon;
        PlayerHandlers.ReloadingWeapon     -= OnPlayerReloadingWeapon;
        PlayerHandlers.DroppingItem        -= OnPlayerDroppingItem;
    }

    // ---------- Olay işleyicileri ----------

    private void OnRoundStarted()
    {
        // Önceki GUI coroutine'ini durdur ve yenisini başlat
        Timing.KillCoroutines(_toolGunCoroutine);
        _toolGunCoroutine = Timing.RunCoroutine(ToolGunGUI());
    }

    /// <summary>
    /// ToolGun kullanan oyuncular için 0.1 saniyede bir HUD günceller.
    /// </summary>
    private static IEnumerator<float> ToolGunGUI()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(0.1f);

            foreach (Player oyuncu in Player.List)
            {
                // ToolGun tutan veya seçili nesnesi olan oyunculara HUD gönder
                if (!oyuncu.CurrentItem.IsToolGun(out ToolGunItem _) &&
                    !ToolGunHandler.TryGetSelectedMapObject(oyuncu, out MapEditorObject _))
                    continue;

                string hud;
                try
                {
                    hud = ToolGunUI.GetHintHUD(oyuncu);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    hud = "HATA: Sunucu konsolunu kontrol edin";
                }

                oyuncu.ShowHint(hud, 0.25f);
            }
        }
    }

    private void OnPlayerDryFiringWeapon(DryFiringWeaponEventArgs ev)
    {
        // ToolGun boş ateş → nesne oluştur/sil/seç
        if (!ev.Firearm.IsToolGun(out ToolGunItem toolGun))
            return;

        ev.IsAllowed = false;
        toolGun.Shot(ev.Player);
    }

    private void OnPlayerReloadingWeapon(ReloadingWeaponEventArgs ev)
    {
        // ToolGun doldurma → önceki nesne türüne geç
        if (!ev.Firearm.IsToolGun(out ToolGunItem toolGun))
            return;

        ev.IsAllowed = false;
        toolGun.SelectedObjectToSpawn--;
    }

    private void OnPlayerDroppingItem(DroppingItemEventArgs ev)
    {
        // ToolGun bırakma → sonraki nesne türüne geç
        if (!ev.Item.IsToolGun(out ToolGunItem toolGun))
            return;

        ev.IsAllowed = false;
        toolGun.SelectedObjectToSpawn++;
    }
}
