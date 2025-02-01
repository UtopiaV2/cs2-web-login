using CasesAPI;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using System.Net;
using System.Text.Json;

namespace cs2_web_login;

class CaseIntegration : IHostedService
{
  private bool _isRunning;
  public bool IsRunning => _isRunning;
  private IWebHost _webHost;

  readonly HttpCfg HttpCfg;
  readonly ILogger Logger;
  private PlayerCapability<IPlayerServices> Capability_PlayerServices { get; } = new("k4-cases:player-services");
  private string path;

  public CaseIntegration(ILogger logger, HttpCfg httpCfg, string path)
  {
    this.Logger = logger;
    this.HttpCfg = httpCfg;
    this.path = path;
    _webHost = null!;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    if (_isRunning)
      return;

    _webHost = new WebHostBuilder().UseKestrel(options =>
    {
      options.Listen(IPAddress.Parse(HttpCfg.Host), HttpCfg.Port);
    })
    .UseContentRoot(path)
    .ConfigureServices(services =>
    {
      services.AddControllers();
    }).Configure(app =>
    {
      app.UseRouting();
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        // Example endpoint
        endpoints.MapGet("/", async context =>
        {
          await context.Response.WriteAsync("Embedded Kestrel Service is running!");
        });
        endpoints.MapPost("/credit", async (HttpRequest request) =>
        {
          var payload = await JsonSerializer.DeserializeAsync<Payload>(request.Body);
          if (payload is null)
          {
            Logger.LogError("Failed to parse payload");
            await JsonSerializer.SerializeAsync(request.Body, new { Success = false });
            return;
          }
          CCSPlayerController? player = Utilities.GetPlayerFromSteamId(payload.SteamID);
          if (player is null)
          {
            Logger.LogError("Failed to get player");
            await JsonSerializer.SerializeAsync(request.Body, new { Success = false });
            return;
          }
          IPlayerServices? PlayerServices = Capability_PlayerServices.Get(player);
          if (PlayerServices is null)
          {
            Logger.LogError("Failed to get Player-Services API for K4-Cases.");
            await JsonSerializer.SerializeAsync(request.Body, new { Success = false });
            return;
          }
          PlayerServices.Credits += payload.Target;
          Logger.LogInformation($"Added {payload.Target} credits to {player.PlayerName}");
          await JsonSerializer.SerializeAsync(request.Body, new { Success = true });
        });
      });
    }).Build();
    await _webHost.StartAsync(cancellationToken);
    _isRunning = true;
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    if (_webHost != null && _isRunning)
    {
      await _webHost.StopAsync(cancellationToken);
      _webHost.Dispose();
      _webHost = null!;
      _isRunning = false;
    }
  }

  public async Task RestartAsync(CancellationToken cancellationToken = default)
  {
    await StopAsync(cancellationToken);
    await StartAsync(cancellationToken);
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
