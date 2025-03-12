using CasesAPI;
using PusherClient;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;

namespace cs2_web_login;

class CI2
{
  private Pusher PC;

  private Channel Channel;

  private readonly ILogger Logger;

  private readonly PusherCfg Cfg;

  private PlayerCapability<IPlayerServices> Capability_PlayerServices { get; } = new("k4-cases:player-services");

  public CI2(PusherCfg cfg, ILogger logger)
  {
    Logger = logger;
    Cfg = cfg;
    Init();
  }
  public async void Init()
  {
    PC = new Pusher(Cfg.Key, new PusherOptions
    {
      Cluster = "eu",
      Encrypted = true
    });
    PC.Error += OnError;
    PC.ConnectionStateChanged += OnConnectionStateChanged;
    PC.Connected += (sender) => Logger.LogInformation("Connected");
    PC.Subscribed += OnSubscribed;
    PC.BindAll(GeneralLog);
    await PC.ConnectAsync().ConfigureAwait(false);

    try
    {
      Channel = await PC.SubscribeAsync("nebula-panel-test");
      Channel.BindAll(OnMessage);
    }
    catch (ChannelUnauthorizedException e)
    {
      Logger.LogError($"Authorization failed for {e.ChannelName}. {e.Message}");
    }
  }

  private void OnMessage(object sender, PusherEvent data)
  {
    Logger.LogInformation("Message: " + data.Data);
    Payload? payload = JsonSerializer.Deserialize<Payload>(data.Data);
    if (payload == null)
    {
      Logger.LogError("Payload is null");
      return;
    }
    Logger.LogInformation("SteamID: " + payload.SteamID);
    Logger.LogInformation("Balance: " + payload.Bal);

    CCSPlayerController? player = Utilities.GetPlayerFromSteamId(payload.SteamID);
    if (player == null)
    {
      Logger.LogError("Player is null");
      return;
    }
    IPlayerServices? PlayerServices = Capability_PlayerServices.Get(player);
    if (PlayerServices == null)
    {
      Logger.LogError("PlayerServices is null");
      return;
    }
    PlayerServices.Credits = payload.Bal;
  }

  private void GeneralLog(string eventName, PusherEvent data)
  {
    Logger.LogInformation("Event: " + eventName + " Data: " + data.Data);
  }


  private void OnError(object sender, PusherException error)
  {
    Logger.LogError("Error: " + error.Message);
  }

  private void OnConnectionStateChanged(object sender, ConnectionState state)
  {
    Logger.LogInformation("Connection state: " + state.ToString());
  }

  private void OnSubscribed(object sender, Channel channelName)
  {
    Logger.LogInformation("Subscribed to channel: " + channelName.Name);
  }
}
