using System.Text.Json.Serialization;

namespace cs2_web_login;

public class PusherCfg
{
  [JsonPropertyName("key")]
  public string Key { get; set; } = "";
};
