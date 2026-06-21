using System.Reflection;
using Exiled.API.Features;
using Mirror;
using ProjectMER.Features.Objects;

namespace ProjectMER.Features.Extensions;

/// <summary>
/// Culling (görünürlük) ile ilgili uzantı metotları.
/// </summary>
public static class CullingExtensions
{
	private static MethodInfo? _sendSpawnMessage;

	private static MethodInfo SendSpawnMessage =>
		_sendSpawnMessage ??= typeof(NetworkServer).GetMethod("SendSpawnMessage", BindingFlags.NonPublic | BindingFlags.Static)!;

	/// <summary>
	/// Belirtilen oyuncuya şematik nesnesini spawn eder.
	/// </summary>
	public static void SpawnSchematic(this Player player, SchematicObject schematic)
	{
		foreach (NetworkIdentity networkIdentity in schematic.NetworkIdentities)
			player.SpawnNetworkIdentity(networkIdentity);
	}

	/// <summary>
	/// Belirtilen oyuncudan şematik nesnesini yok eder.
	/// </summary>
	public static void DestroySchematic(this Player player, SchematicObject schematic)
	{
		foreach (NetworkIdentity networkIdentity in schematic.NetworkIdentities)
			player.DestroyNetworkIdentity(networkIdentity);
	}

	/// <summary>
	/// Belirtilen oyuncuya bir NetworkIdentity spawn eder.
	/// </summary>
	public static void SpawnNetworkIdentity(this Player player, NetworkIdentity networkIdentity) =>
		SendSpawnMessage.Invoke(null, new object[] { networkIdentity, player.Connection });

	/// <summary>
	/// Belirtilen oyuncudan bir NetworkIdentity yok eder.
	/// </summary>
	public static void DestroyNetworkIdentity(this Player player, NetworkIdentity networkIdentity) =>
		player.Connection.Send(new ObjectDestroyMessage { netId = networkIdentity.netId });
}