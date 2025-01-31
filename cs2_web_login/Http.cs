using System.Text.Json.Serialization;

namespace cs2_web_login;

public class HttpCfg
{
  [JsonPropertyName("Host")]
  public string Host { get; set; } = "localhost";

  [JsonPropertyName("Port")]
  public int Port { get; set; } = 8080;

  [JsonPropertyName("BehindProxy")]
  public bool BehindProxy { get; set; } = true;

  [JsonPropertyName("AllowedOrigins")]
  public string[] AllowedOrigins { get; set; } = new string[] { "*" };

  [JsonPropertyName("UseSSL")]

  public bool UseSSL { get; set; } = false;

  [JsonPropertyName("SSLCertPath")]
  public string SSLCertPath { get; set; } = "";

  [JsonPropertyName("SSLCertPassword")]
  public string SSLCertPassword { get; set; } = "";
}
