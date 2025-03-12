using System.Text.Json.Serialization;

namespace cs2_web_login;

public class Payload
{
  [JsonPropertyName("steam_id")]
  public ulong SteamID;

  [JsonPropertyName("new_balance")]
  public decimal Bal;
}
