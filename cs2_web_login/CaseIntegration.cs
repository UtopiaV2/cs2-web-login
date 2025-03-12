using PusherClient;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace cs2_web_login;

class CI2
{
  private Pusher PC;
  private Channel Channel;
  private readonly ILogger Logger;
  private readonly PusherCfg Cfg;
  private List<Payload> Payloads;

  public CI2(PusherCfg cfg, ILogger logger, ref List<Payload> payloads)
  {
    Logger = logger;
    Cfg = cfg;
    Payloads = payloads;
    Init();
  }

  ~CI2()
  {
    PC.DisconnectAsync().Wait();
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
    Logger.LogInformation("Adding payload to list");
    Payloads.Add(payload);
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
