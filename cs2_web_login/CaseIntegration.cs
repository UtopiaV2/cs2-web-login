using CasesAPI;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace cs2_web_login;

class CaseIntegration
{
  public static PlayerCapability<IPlayerServices> Capability_PlayerServices { get; } = new("k4-cases:player-services");

  public static PluginCapability<IModuleServices> Capability_ModuleServices { get; } = new("k4-cases:module-services");
  public static PluginCapability<IDatabaseServices> Capability_DatabaseServices { get; } = new("k4-cases:database-services");
  public static PluginCapability<IConfigServices> Capability_ConfigServices { get; } = new("k4-cases:config-services");

  ILogger Logger;

  public CaseIntegration(ILogger logger)
  {
    this.Logger = logger;
    IModuleServices ModuleServices = Capability_ModuleServices.Get();

    if (ModuleServices is null)
    {
      logger.LogError("ModuleServices is null");
      return;
    }
  }

  public void Player(CCSPlayerController player)
  {
    IPlayerServices? PlayerServices = Capability_PlayerServices.Get(player);

    if (PlayerServices is null)
    {
      Logger.LogError("Failed to get Player-Services API for K4-Cases.");
      return;
    }
    Logger.LogInformation("Player-Services API for K4-Cases is available.");
    PlayerServices.RefreshWeapon((int)WeaponDefIndex.Ak47, true);

  }
}
