using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LabApi.Features.Wrappers;
using ProjectMER.Configs;

namespace ProjectMER.Features.MapRotator;

public static class MapRotatorManager
{
	private static Config Config => ProjectMER.Singleton.Config!;

	public static void CheckAndRotateMaps()
	{
		if (!Config.EnableMapRotator || Config.MapRotationRules == null || Config.MapRotationRules.Count == 0)
			return;

		int playerCount = Player.List.Count;
		Logger.Debug($"Harita rotasyonu kontrol ediliyor. Oyuncu sayısı: {playerCount}");

		MapRotationRule? matchedRule = null;
		foreach (var rule in Config.MapRotationRules)
		{
			if (playerCount >= rule.MinPlayers && playerCount <= rule.MaxPlayers)
			{
				matchedRule = rule;
				break;
			}
		}

		if (matchedRule != null && matchedRule.MapsToLoad != null && matchedRule.MapsToLoad.Count > 0)
		{
			string mapToLoad = matchedRule.MapsToLoad.RandomItem();
			Logger.Info($"Harita rotasyon kuralı eşleşti. Oyuncu sayısı: {playerCount}. Yüklenen harita: {mapToLoad}");

			// Unload all loaded maps first
			foreach (string loadedMap in MapUtils.LoadedMaps.Keys.ToList())
			{
				MapUtils.UnloadMap(loadedMap);
			}

			if (MapUtils.TryGetMapData(mapToLoad, out _))
			{
				MapUtils.LoadMap(mapToLoad);
			}
			else
			{
				Logger.Error($"Harita rotasyon kuralı hatası: '{mapToLoad}' adlı harita dosyası mevcut değil!");
			}
		}
	}
}
