using System.Text.Json.Serialization;

namespace cs2_web_login;

public class Payload
{
  [JsonPropertyName("steamid")]
  public ulong SteamID;

  [JsonPropertyName("bal")]
  public double Target;
}
