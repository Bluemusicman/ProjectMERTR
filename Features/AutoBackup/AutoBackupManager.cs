using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MEC;
using ProjectMER.Configs;

namespace ProjectMER.Features.AutoBackup;

public static class AutoBackupManager
{
	private static Config Config => ProjectMER.Singleton.Config!;
	private static CoroutineHandle _backupCoroutine;

	public static void Start()
	{
		Timing.KillCoroutines(_backupCoroutine);
		_backupCoroutine = Timing.RunCoroutine(BackupLoop());
	}

	public static void Stop()
	{
		Timing.KillCoroutines(_backupCoroutine);
	}

	private static IEnumerator<float> BackupLoop()
	{
		while (true)
		{
			yield return Timing.WaitForSeconds(Config.AutoBackupInterval);

			try
			{
				if (!Config.EnableAutoBackup)
					continue;

				bool shouldBackup = MapUtils.UntitledMap.SpawnedObjects.Count > 0;
				foreach (var map in MapUtils.LoadedMaps.Values)
				{
					if (map.IsDirty)
					{
						shouldBackup = true;
						break;
					}
				}

				if (!shouldBackup)
					continue;

				string backupDir = Path.Combine(ProjectMER.PluginDir, "Backups");
				if (!Directory.Exists(backupDir))
				{
					Directory.CreateDirectory(backupDir);
				}

				if (MapUtils.UntitledMap.SpawnedObjects.Count > 0)
				{
					string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
					string path = Path.Combine(backupDir, $"Untitled_Backup_{timestamp}.yml");
					File.WriteAllText(path, YamlParser.Serializer.Serialize(MapUtils.UntitledMap));
					Logger.Info($"Otomatik yedekleme kaydedildi: {path}");
				}

				foreach (var map in MapUtils.LoadedMaps.Values)
				{
					if (map.IsDirty && map.Name != MapUtils.UntitledMapName)
					{
						string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
						string path = Path.Combine(backupDir, $"{map.Name}_Backup_{timestamp}.yml");
						File.WriteAllText(path, YamlParser.Serializer.Serialize(map));
						Logger.Info($"Harita otomatik yedekleme kaydedildi: {path}");
						map.IsDirty = false;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"Otomatik yedekleme sırasında hata: {ex}");
			}
		}
	}
}
