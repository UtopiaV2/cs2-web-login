using CasesAPI;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace cs2_web_login;

class CaseIntegration
{
  public static PlayerCapability<IPlayerServices> Capability_PlayerServices { get; } = new("k4-cases:player-services");

  public static PluginCapability<IModuleServices> Capability_ModuleServices { get; } = new("k4-cases:module-services");
  public static PluginCapability<IDatabaseServices> Capability_DatabaseServices { get; } = new("k4-cases:database-services");
  public static PluginCapability<IConfigServices> Capability_ConfigServices { get; } = new("k4-cases:config-services");

  ILogger Logger;
  HttpCfg HttpCfg;
  public CaseIntegration(ILogger logger, HttpCfg httpCfg)
  {
    this.Logger = logger;
    this.HttpCfg = httpCfg;

    var builder = WebApplication.CreateBuilder();
    var app = builder.Build();

    app.MapPost("/credit", (HttpRequest request) =>
    {
      var payload = JsonSerializer.Deserialize<Payload>(request.Body);
      if (payload is null)
      {
        Logger.LogError("Failed to parse payload");
        return;
      }

      CCSPlayerController? player = Utilities.GetPlayerFromSteamId(payload.SteamID);
      if (player is null)
      {
        Logger.LogError("Failed to get player");
        return;
      }
      IPlayerServices? PlayerServices = Capability_PlayerServices.Get(player);
      if (PlayerServices is null)
      {
        Logger.LogError("Failed to get Player-Services API for K4-Cases.");
        return;
      }
      PlayerServices.Credits += payload.Target;
    });
    app.RunAsync();
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
    PlayerServices.Credits += 1000;
  }
}
