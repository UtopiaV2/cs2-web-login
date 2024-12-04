using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO.Compression;

namespace cs2_web_login;

class AutoUpdater
{
  ILogger logger;
  public Release latestRelease;
  private readonly string local_version;
  string module_path;

  public AutoUpdater(string local_version, ILogger logger, string module_path, bool autoCheck = true)
  {
    this.logger = logger;
    this.local_version = local_version;
    this.module_path = module_path;
    logger.LogInformation("Checking for updates");
    if (autoCheck)
      latestRelease = GetLatestRelease().Result;
    else
      latestRelease = Rel.CreateNull();
  }

  public async Task<Release> GetLatestRelease()
  {
    using HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("User-Agent", "cs2-web-login");
    try
    {
      HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/UtopiaV2/cs2-web-login/releases/latest");
      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      latestRelease = JsonSerializer.Deserialize<Release>(responseBody) ?? throw new Exception("Failed to deserialize json");
      return latestRelease;
    }
    catch (Exception e)
    {
      logger.LogCritical(e, "Failed to get latest version");
      return Rel.CreateNull();
    }
  }

  public bool DownloadUpdate()
  {
    if (latestRelease == null)
    {
      logger.LogWarning("No release found");
      return false;
    }
    string download_url = latestRelease.Assets[0].BrowserDownloadUrl;
    string download_path = Path.Combine(module_path, "update.zip");
    using HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("User-Agent", "cs2-web-login");
    try
    {
      using (var res = client.GetStreamAsync(download_url))
      {
        using (var fileStream = File.Create(download_path))
        {
          res.Result.CopyTo(fileStream);
          fileStream.Flush();
          fileStream.Close();
        }
      }
      logger.LogInformation("Downloaded update");
      return true;
    }
    catch (Exception e)
    {
      logger.LogCritical(e, "Failed to download update");
      return false;
    }
  }

  public bool UnpackUpdate()
  {
    string download_path = Path.Combine(module_path, "update.zip");
    string extract_path = module_path.Split('/').Reverse().Skip(1).Reverse().Aggregate((a, b) => a + "/" + b);
    try
    {
      ZipFile.ExtractToDirectory(download_path, extract_path, true);
      logger.LogInformation("Extracted update");
      return true;
    }
    catch (Exception e)
    {
      logger.LogCritical(e, "Failed to extract update");
      return false;
    }
  }

  public bool IsUpdateAvailable()
  {
    latestRelease = GetLatestRelease().Result;
    return latestRelease.TagName != local_version;
  }

  public override string ToString()
  {
    return $"Local version: {local_version}, Latest version: {latestRelease.TagName}";
  }
}
/*Vim: set expandtab tabstop=2 shiftwidth=2:*/
