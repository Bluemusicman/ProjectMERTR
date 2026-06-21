using CommandSystem;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using ProjectMER.Commands.Modifying.Scale.SubCommands;

namespace ProjectMER.Commands.Modifying.Scale;

public class Scale : ParentCommand
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Scale"/> class.
	/// </summary>
	public Scale() => LoadGeneratedCommands();

	/// <inheritdoc/>
	public override string Command => "scale";

	/// <inheritdoc/>
	public override string[] Aliases { get; } = ["scl"];

	/// <inheritdoc/>
	public override string Description => "Nesnenin boyutunu değiştirir.";

	/// <inheritdoc/>
	public override void LoadGeneratedCommands()
	{
		RegisterCommand(new Add());
		RegisterCommand(new Set());
	}

	/// <inheritdoc/>
	protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		if (!sender.CheckPermission($"mpr.{Command}"))
		{
			response = $"Bu komutu yürütmek için yetkiniz yok. Gerekli yetki: mpr.{Command}";
			return false;
		}

		Player? player = Player.Get(sender);
		if (player is null)
		{
			response = "Bu komut sunucu konsolundan çalıştırılamaz.";
			return false;
		}

		response = "\nUsage:\n";
		response += "mp scale set (x) (y) (z)\n";
		response += "mp scale add (x) (y) (z)\n";
		return false;
	}
}
