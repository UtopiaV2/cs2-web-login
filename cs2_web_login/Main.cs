using System.Security.Cryptography;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using MySqlConnector;
using BC = BCrypt.Net;

namespace cs2_web_login;

public class Class1 : BasePlugin, IPluginConfig<Cfg>
{
  public Database? db;
  public override string ModuleName => "WebLogin";
  public override string ModuleVersion => "$V_PLACE_HOLDER";
  public override string ModuleAuthor => "OwnSample";
  public override string ModuleDescription => "Creates logins for cry babies! :D";
  public Cfg Config { get; set; } = new Cfg();
  AutoUpdater AU = null!; // AutoUpdater is not null, but it's not initialized
  private CaseIntegration CI = null!; // CaseIntegration is not null, but it's not initialized

  public override void Load(bool hotReload)
  {
    db = new Database($"Server={Config.Db.Host};User ID={Config.Db.User};Password={Config.Db.Password};Database={Config.Db.Database}", base.Logger);
    AU = new AutoUpdater(ModuleVersion, base.Logger, base.ModuleDirectory, Config.AutoUpdate, Config.Rc);
    if (AU.IsUpdateAvailable() && Config.AutoUpdate)
    {
      base.Logger.LogWarning("Update available");
      base.Logger.LogWarning(AU.ToString());
      if (AU.DownloadUpdate().Result)
      {
        base.Logger.LogInformation("Downloaded update");
        if (AU.UnpackUpdate())
        {
          base.Logger.LogInformation("Unpacked update");
          base.Logger.LogInformation("Restarting server");
        }
      }
    }
    else
    {
      base.Logger.LogInformation("No updates available");
    }
    CI = new(base.Logger, Config.Http, base.ModuleDirectory);
    var hosts = Host.CreateDefaultBuilder().ConfigureServices(services =>
    {
      services.AddSingleton<CaseIntegration>();
      services.AddHostedService(_ => CI);
    }).Build().RunAsync();
  }

  public void OnConfigParsed(Cfg config)
  {
    Config = config;
  }

  public override void Unload(bool hotReload)
  {
    if (!hotReload)
    {
      db = db ?? throw new Exception("Db is null");
      db.GetConnection().Close();
    }
    if (CI.IsRunning)
    {
      CI.StopAsync(CancellationToken.None).Wait();
    }
  }

  [ConsoleCommand("css_login_update", "Auto update")]
  public void AutoUpdateCommand(CCSPlayerController? player, CommandInfo info)
  {
    if (player != null)
    {
      return;
    }
    if (AU.IsUpdateAvailable())
    {
      base.Logger.LogWarning("Update available");
      base.Logger.LogWarning(AU.ToString());
      if (AU.DownloadUpdate().Result)
      {
        base.Logger.LogInformation("Downloaded update");
        if (AU.UnpackUpdate())
        {
          base.Logger.LogInformation("Unpacked update");
          base.Logger.LogInformation("Restarting server");
        }
      }
    }
    else
    {
      base.Logger.LogInformation("No updates available");
    }
  }

  [ConsoleCommand("css_delcredentials", "Del login cred")]
  public void OnDelGenLoginCred(CCSPlayerController? player, CommandInfo info)
  {
    if (player == null)
    {
      base.Logger.LogWarning("Console can't use this");
      return;
    }
    db = db ?? throw new Exception("Db is null");
    MySqlCommand cmd = db.GetConnection().CreateCommand();
    cmd.CommandText = $"SELECT t.steam_id FROM {Config.Db.Prefix}{Config.Db.Table} t WHERE t.steam_id = {player.SteamID}";
    MySqlDataReader reader = cmd.ExecuteReader();
    if (reader.RecordsAffected == 0)
    {
      player.PrintToCenterAlert(Config.NoLogCredForDel);
      return;
    }
    while (reader.Read())
    {
      ulong SteamID = reader.GetUInt64(0);
      if (SteamID != player.SteamID)
      {
        throw new Exception("How tf was player.steamId and what we got back is not the same");
      }
    }
    reader.Close();
    cmd.CommandText = $"DELETE FROM {Config.Db.Prefix}{Config.Db.Table} WHERE steam_id = {player.SteamID}";
    cmd.ExecuteReader();
    player.PrintToCenterAlert(Config.SuccDel);
  }

  [ConsoleCommand("css_credentials", "Gen login cred")]
  public void OnGenLoginCred(CCSPlayerController? player, CommandInfo info)
  {
    if (player == null)
    {
      base.Logger.LogWarning("Console can't use this");
      return;
    }
    db = db ?? throw new Exception("Db is null");
    MySqlCommand cmd = db.GetConnection().CreateCommand();
    cmd.CommandText = $"SELECT t.steam_id FROM {Config.Db.Prefix}{Config.Db.Table} t WHERE t.steam_id = {player.SteamID}";
    MySqlDataReader reader = cmd.ExecuteReader();
    if (reader.Read())
    {
      ulong SteamID = reader.GetUInt64(0);
      if (SteamID != player.SteamID)
      {
        throw new Exception("How tf was player.steamId and what we got back is not the same");
      }
      player.PrintToCenterAlert(Config.InfoAbtCredDel);
      return;
    }
    reader.Close();
    byte[] pw = RandomNumberGenerator.GetBytes(Config.BwBlen);
    string pwS = Convert.ToBase64String(pw);
    var hash = BC.BCrypt.HashPassword(pwS, workFactor: Config.Bc_workfactor).Replace("$2a$", "$2y$"); // upgrade hash xd
    cmd.CommandText = $"INSERT INTO {Config.Db.Prefix}{Config.Db.Table} (steam_id, pw, username) VALUE ({player.SteamID}, \"{hash}\", {player.SteamID})";
    cmd.ExecuteNonQuery();
    player.PrintToCenterAlert(Config.PwAlert);
    player.PrintToConsole("---------------------------------------------------------------");
    player.PrintToConsole(Config.UnMsg);
    player.PrintToConsole(player.SteamID.ToString());
    player.PrintToConsole(Config.Pwdau);
    player.PrintToConsole(pwS);
    player.PrintToConsole("---------------------------------------------------------------");
  }
  static void Main() { }
}
/*Vim: set expandtab tabstop=4 shiftwidth=4:*/
