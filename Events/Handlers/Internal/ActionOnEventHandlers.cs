using System.Text.RegularExpressions;
using Exiled.Events.EventArgs.Warhead;
using MEC;
using NorthwoodLib.Pools;
using ProjectMER.Configs;
using ProjectMER.Features;
using ServerHandlers = Exiled.Events.Handlers.Server;
using WarheadHandlers = Exiled.Events.Handlers.Warhead;

namespace ProjectMER.Events.Handlers.Internal;

/// <summary>
/// Yapılandırma dosyasında tanımlanan olay bazlı eylemleri yöneten işleyici sınıfı.
/// Exiled 9.14.2 sürümünde += / -= abonelik yöntemi kullanılır.
/// </summary>
public class ActionOnEventHandlers
{
    private static Config Config => ProjectMER.Singleton.Config!;

    /// <summary>
    /// Exiled olaylarına abone olur.
    /// </summary>
    public void Kaydet()
    {
        ServerHandlers.WaitingForPlayers  += OnWaitingForPlayers;
        ServerHandlers.RoundStarted       += OnRoundStarted;
        ServerHandlers.LczDecontaminationStarted += OnLczDecontaminationStarted;
        WarheadHandlers.Starting          += OnWarheadStarted;
        WarheadHandlers.Stopping          += OnWarheadStopped;
        WarheadHandlers.Detonating        += OnWarheadDetonated;
    }

    /// <summary>
    /// Exiled olaylarından aboneliği kaldırır.
    /// </summary>
    public void Kaldir()
    {
        ServerHandlers.WaitingForPlayers  -= OnWaitingForPlayers;
        ServerHandlers.RoundStarted       -= OnRoundStarted;
        ServerHandlers.LczDecontaminationStarted -= OnLczDecontaminationStarted;
        WarheadHandlers.Starting          -= OnWarheadStarted;
        WarheadHandlers.Stopping          -= OnWarheadStopped;
        WarheadHandlers.Detonating        -= OnWarheadDetonated;
    }

    // ---------- Olay işleyicileri ----------

    private void OnWaitingForPlayers()
        => Timing.CallDelayed(0.1f, () => HandleActionList(Config.OnWaitingForPlayers));

    private void OnRoundStarted()
        => HandleActionList(Config.OnRoundStarted);

    private void OnLczDecontaminationStarted()
        => HandleActionList(Config.OnLczDecontaminationStarted);

    private void OnWarheadStarted(StartingEventArgs _)
        => HandleActionList(Config.OnWarheadStarted);

    private void OnWarheadStopped(StoppingEventArgs _)
        => HandleActionList(Config.OnWarheadStopped);

    private void OnWarheadDetonated(DetonatingEventArgs _)
        => HandleActionList(Config.OnWarheadDetonated);

    // ---------- Yardımcı yöntemler ----------

    private void HandleActionList(List<string> liste)
    {
        foreach (string element in liste)
        {
            string[] eylemParcalari = element.Split(':');
            string eylem = eylemParcalari[0];
            string arguman = eylemParcalari[1];

            switch (eylem.ToLowerInvariant())
            {
                case "load":
                case "l":
                {
                    List<string> tumHaritalar = ListPool<string>.Shared.Rent(
                        Directory.GetFiles(ProjectMER.MapsDir).Select(Path.GetFileNameWithoutExtension)!
                    );
                    HandleMapLoading(arguman, tumHaritalar);
                    ListPool<string>.Shared.Return(tumHaritalar);
                    continue;
                }

                case "unload":
                case "unl":
                {
                    List<string> yuklenenHaritalar = ListPool<string>.Shared.Rent(MapUtils.LoadedMaps.Keys);
                    HandleMapUnloading(arguman, yuklenenHaritalar);
                    ListPool<string>.Shared.Return(yuklenenHaritalar);
                    continue;
                }

                case "console":
                case "cs":
                {
                    // Exiled'da konsol komutu çalıştırma
                    Exiled.API.Features.Server.RunCommand(arguman);
                    continue;
                }

                default:
                {
                    Exiled.API.Features.Log.Error($"Bilinmeyen eylem: {eylem}");
                    continue;
                }
            }
        }
    }

    private void HandleMapLoading(string arguman, List<string> tumHaritalar)
    {
        string[] veyaParcalari = arguman.Split('|', '|');
        string[] veParcalari   = arguman.Split(',');

        if (veyaParcalari.Length > 1 || veParcalari.Length > 1)
        {
            if (veParcalari.Length > veyaParcalari.Length)
                veParcalari.ForEach(x => HandleMapLoading(x, tumHaritalar));
            else
                HandleMapLoading(veyaParcalari.RandomItem(), tumHaritalar);

            return;
        }

        foreach (string haritaAdi in tumHaritalar)
        {
            if (Regex.IsMatch(haritaAdi, WildCardToRegular(arguman)))
                MapUtils.LoadMap(haritaAdi);
        }
    }

    private void HandleMapUnloading(string arguman, List<string> tumHaritalar)
    {
        string[] veyaParcalari = arguman.Split('|', '|');
        string[] veParcalari   = arguman.Split(',');

        if (veyaParcalari.Length > 1 || veParcalari.Length > 1)
        {
            if (veParcalari.Length > veyaParcalari.Length)
                veParcalari.ForEach(x => HandleMapLoading(x, tumHaritalar));
            else
                HandleMapLoading(veyaParcalari.RandomItem(), tumHaritalar);

            return;
        }

        foreach (string haritaAdi in tumHaritalar)
        {
            if (Regex.IsMatch(haritaAdi, WildCardToRegular(arguman)))
                MapUtils.UnloadMap(haritaAdi);
        }
    }

    private static string WildCardToRegular(string deger)
        => "^" + Regex.Escape(deger).Replace("\\?", ".").Replace("\\*", ".*") + "$";
}
