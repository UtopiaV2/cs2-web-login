using System.Text.Json.Serialization;

namespace cs2_web_login;

public class PusherCfg
{
  [JsonPropertyName("Key")]
  public string Key { get; set; } = "";

  [JsonPropertyName("Interval")]
  public float Interval { get; set; } = 0.5f;
};
