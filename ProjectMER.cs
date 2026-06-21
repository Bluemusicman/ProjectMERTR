// Exiled sürümünde global Logger yerine Exiled'ın Log sınıfı kullanılır.
// Kullanım: Log.Info(...), Log.Warn(...), Log.Error(...), Log.Debug(...)

using Exiled.API.Features;
using HarmonyLib;
using MEC;
using ProjectMER.Configs;
using ProjectMER.Events.Handlers.Internal;
using ProjectMER.Features;
using System.IO;
using System.Threading;

namespace ProjectMER;

/// <summary>
/// ProjectMER'in Exiled 9.14.2 sürümündeki ana eklenti sınıfı.
/// </summary>
public class ProjectMER : Plugin<Config>
{
    private Harmony _harmony = null!;
    private FileSystemWatcher? _mapFileSystemWatcher;

    /// <summary>
    /// Eklentinin tekil örneğini alır.
    /// </summary>
    public static ProjectMER Singleton { get; private set; } = null!;

    private CoroutineHandle _autoSaveCoroutine;

    /// <summary>
    /// Eklentinin ana klasör yolunu alır.
    /// </summary>
    public static string PluginDir { get; private set; } = string.Empty;

    /// <summary>
    /// Haritaların saklandığı klasör yolunu alır.
    /// </summary>
    public static string MapsDir { get; private set; } = string.Empty;

    /// <summary>
    /// Şematiklerin saklandığı klasör yolunu alır.
    /// </summary>
    public static string SchematicsDir { get; private set; } = string.Empty;

    // Olay işleyicisi örnekleri
    public GenericEventsHandler GenericEventsHandler { get; private set; } = null!;
    public ToolGunEventsHandler ToolGunEventsHandler { get; private set; } = null!;
    public ActionOnEventHandlers ActionOnEventHandlers { get; private set; } = null!;
    public PickupEventsHandler PickupEventsHandler { get; private set; } = null!;

    public override string Name => "ProjectMER";
    public override string Author => "souin";
    public override string Prefix => "ProjectMER";
    public override Version Version => new(2025, 11, 2, 1);
    public override Version RequiredExiledVersion => new(9, 14, 2);

    public override void OnEnabled()
    {
        Singleton = this;

        // Harmony yamalarını uygula
        _harmony = new Harmony($"souin.projectMER-{DateTime.Now.Ticks}");
        _harmony.PatchAll();

        // Klasör yollarını ayarla
        PluginDir   = Path.Combine(Paths.Configs, "ProjectMER");
        MapsDir     = Path.Combine(PluginDir, "Maps");
        SchematicsDir = Path.Combine(PluginDir, "Schematics");

        // Gerekli klasörleri oluştur
        if (!Directory.Exists(PluginDir))
        {
            Log.Warn("Eklenti klasörü bulunamadı. Oluşturuluyor...");
            Directory.CreateDirectory(PluginDir);
        }

        if (!Directory.Exists(MapsDir))
        {
            Log.Warn("Harita klasörü bulunamadı. Oluşturuluyor...");
            Directory.CreateDirectory(MapsDir);
        }

        if (!Directory.Exists(SchematicsDir))
        {
            Log.Warn("Şematik klasörü bulunamadı. Oluşturuluyor...");
            Directory.CreateDirectory(SchematicsDir);
        }

        // Olay işleyicilerini oluştur ve kaydet
        GenericEventsHandler = new GenericEventsHandler();
        ToolGunEventsHandler = new ToolGunEventsHandler();
        ActionOnEventHandlers = new ActionOnEventHandlers();
        PickupEventsHandler = new PickupEventsHandler();

        GenericEventsHandler.Kaydet();
        ToolGunEventsHandler.Kaydet();
        ActionOnEventHandlers.Kaydet();
        PickupEventsHandler.Kaydet();

        // DosyaSistemİzleyici etkinleştirme
        if (Config!.EnableFileSystemWatcher)
        {
            _mapFileSystemWatcher = new FileSystemWatcher(MapsDir)
            {
                NotifyFilter      = NotifyFilters.LastWrite,
                Filter            = "*.yml",
                EnableRaisingEvents = true,
            };

            _mapFileSystemWatcher.Changed += OnMapFileChanged;

            Log.Debug("DosyaSistemİzleyici etkinleştirildi!");
        }

        if (Config!.EnableAutoSave)
        {
            _autoSaveCoroutine = Timing.RunCoroutine(AutoSaveCoroutine());
        }

        base.OnEnabled();
    }

    private IEnumerator<float> AutoSaveCoroutine()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(Config.AutoSaveInterval);
            
            if (!Config.EnableAutoSave) continue;

            foreach (var mapName in MapUtils.LoadedMaps.Keys.ToList())
            {
                if (mapName != MapUtils.UntitledMapName && MapUtils.LoadedMaps[mapName].IsDirty)
                {
                    MapUtils.SaveMap(mapName, isAutoSave: true);
                }
            }
        }
    }

    public override void OnDisabled()
    {
        // Olay işleyicilerinin aboneliğini kaldır
        GenericEventsHandler?.Kaldir();
        ToolGunEventsHandler?.Kaldir();
        ActionOnEventHandlers?.Kaldir();
        PickupEventsHandler?.Kaldir();

        // Harmony yamalarını geri al
        _harmony?.UnpatchAll(_harmony.Id);

        // DosyaSistemİzleyici'yi temizle
        if (_mapFileSystemWatcher != null)
        {
            _mapFileSystemWatcher.Changed -= OnMapFileChanged;
            _mapFileSystemWatcher.Dispose();
            _mapFileSystemWatcher = null;
        }

        Singleton = null!;

        Timing.KillCoroutines(_autoSaveCoroutine);
        
        base.OnDisabled();
    }

    private void OnMapFileChanged(object _, FileSystemEventArgs ev)
    {
        string mapName = ev.Name!.Split('.')[0];
        if (!Features.MapUtils.LoadedMaps.ContainsKey(mapName))
            return;

        Timing.CallDelayed(0.01f, () =>
        {
            try
            {
                Features.MapUtils.LoadMap(mapName);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        });
    }
}
