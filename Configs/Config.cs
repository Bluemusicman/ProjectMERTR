using System.ComponentModel;
using Exiled.API.Interfaces;

namespace ProjectMER.Configs;

/// <summary>
/// ProjectMER eklentisinin yapılandırma sınıfı.
/// Exiled sürümü — IConfig arayüzünü uygulaması zorunludur.
/// </summary>
public class Config : IConfig
{
    /// <summary>
    /// Eklentinin etkin olup olmadığını belirtir.
    /// </summary>
    [Description("Eklentinin aktif olup olmadığını belirtir.")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Hata ayıklama mesajlarının gösterilip gösterilmeyeceğini belirtir.
    /// </summary>
    [Description("Hata ayıklama mesajlarının gösterilip gösterilmeyeceğini belirtir.")]
    public bool Debug { get; set; } = false;

    /// <summary>
    /// DosyaSistemİzleyici'yi etkinleştirir. Bu özellik açıkken, sunucuda yüklü olan
    /// bir harita dosyasını manuel olarak düzenleyip kaydettiğinizde, eklenti
    /// haritayı otomatik olarak yeniden yükler.
    /// </summary>
    [Description("DosyaSistemİzleyici'yi etkinleştirir. Aktifken harita dosyalarındaki değişiklikler oyun içinde otomatik uygulanır.")]
    public bool EnableFileSystemWatcher { get; set; } = false;

    /// <summary>
    /// ToolGun ile nesne oluşturulduğunda otomatik olarak seçilip seçilmeyeceğini belirtir.
    /// </summary>
    [Description("ToolGun ile nesne oluştururken otomatik seçim yapılıp yapılmayacağını belirtir.")]
    public bool AutoSelect { get; set; } = true;

    [Description(
        "\n" +
        "# ----------------------------- Olay Bazlı Eylemler -----------------------------\n" +
        "# Aşağıdaki liste, belirli oyun olaylarında çalıştırılacak eylemleri tanımlar.\n" +
        "# -------------------------------------------------------------------------------\n" +
        "#\n" +
        "# Harita Yükleme/Kaldırma\n" +
        "# İstediğiniz zaman harita yükleme veya kaldırma işlemi başlatabilirsiniz.\n" +
        "# Desen eşleştirme, birden fazla harita ve rastgele seçim desteklenmektedir.\n" +
        "# Zaten yüklü bir haritayı tekrar yüklemek onu yeniden yükler.\n" +
        "#\n" +
        "# - load:GüzelHarita\n" +
        "#   GüzelHarita adlı haritayı yükler\n" +
        "#\n" +
        "# - unload:GüzelHarita\n" +
        "#   GüzelHarita adlı haritayı kaldırır\n" +
        "#\n" +
        "# - load:LczHaritasi,HczHaritasi,EzHaritasi\n" +
        "#   Listedeki TÜM haritaları yükler.\n" +
        "#\n" +
        "# - load:VariantA||VariantB||VariantC\n" +
        "#   Listeden BİR harita yükler; olasılıklar eşittir.\n" +
        "#   Aynı harita adını birden fazla yazarak olasılığını artırabilirsiniz.\n" +
        "#\n" +
        "# - load:*\n" +
        "#   Kayıtlı tüm haritaları yükler\n" +
        "#\n" +
        "# - unload:*\n" +
        "#   Yüklü tüm haritaları kaldırır (Untitled dahil)\n" +
        "#\n" +
        "# Konsol Komutu\n" +
        "# Özel bir konsol komutu çalıştırmak için kullanılır.\n" +
        "# Remote Admin komutlarının başına \"/\" eklenmesi gerekir.\n" +
        "#\n" +
        "# - console:buildinfo\n" +
        "#   Sunucunun buildinfo bilgisini yazdırır\n" +
        "#\n" +
        "# - console:/bc 10 MER harika!\n" +
        "#   Tüm oyunculara yayın gönderir\n"
    )]
    public List<string> OnWaitingForPlayers { get; set; } = [];
    public List<string> OnRoundStarted { get; set; } = [];
    public List<string> OnLczDecontaminationStarted { get; set; } = [];
    public List<string> OnWarheadStarted { get; set; } = [];
    public List<string> OnWarheadStopped { get; set; } = [];
    public List<string> OnWarheadDetonated { get; set; } = [];
}
