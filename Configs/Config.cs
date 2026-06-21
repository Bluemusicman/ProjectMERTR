using System.Collections.Generic;
using System.ComponentModel;

namespace ProjectMER.Configs;

public class Config
{
	[Description("DosyaSistemİzleyici'yi etkinleştirir. Aktifken harita dosyalarındaki değişiklikler oyun içinde otomatik uygulanır.")]
	public bool EnableFileSystemWatcher { get; set; } = false;

	[Description("ToolGun ile nesne oluştururken otomatik seçim yapılıp yapılmayacağını belirtir.")]
	public bool AutoSelect { get; set; } = true;

	[Description("Haritayı belirli aralıklarla otomatik olarak kaydeder.")]
	public bool EnableAutoSave { get; set; } = true;

	[Description("Otomatik kayıt işleminin kaç saniyede bir yapılacağını belirler (Varsayılan: 300).")]
	public float AutoSaveInterval { get; set; } = 300f;

	[Description("Harita kaydedilirken otomatik yedekleme alınıp alınmayacağını belirtir.")]
	public bool EnableAutoBackup { get; set; } = false;

	[Description("Eski yedeklerin silinmeden önce tutulacak maksimum yedek sayısı. (Sınırsız için -1)")]
	public int MaxBackups { get; set; } = 5;

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

	[Description("Harita rotasyonunun etkin olup olmayacağı.")]
	public bool EnableMapRotator { get; set; } = false;

	[Description("Oyuncu sayısına göre otomatik harita yükleme kuralları.")]
	public List<MapRotationRule> MapRotationRules { get; set; } = [];



	[Description("Yedekleme sıklığı (saniye cinsinden). Varsayılan 300 saniye (5 dakika).")]
	public int AutoBackupInterval { get; set; } = 300;
}

public class MapRotationRule
{
	public int MinPlayers { get; set; } = 0;
	public int MaxPlayers { get; set; } = 99;
	public List<string> MapsToLoad { get; set; } = [];
}
