using System.Security.Cryptography;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using MySqlConnector;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Core.Capabilities;
using CasesAPI;
using BC = BCrypt.Net;
using CounterStrikeSharp.API.Modules.Entities;

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
  private PlayerCapability<IPlayerServices> Capability_PlayerServices { get; } = new("k4-cases:player-services");
  private CounterStrikeSharp.API.Modules.Timers.Timer T = null!;

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
    T = new(Config.Interval, () =>
    {
      if (db == null)
        throw new Exception("Db is null");
      MySqlCommand cmd = db.GetConnection().CreateCommand();
      cmd.CommandText = $"LOCK TABLE {Config.Db.TransactionTable} WRITE";
      cmd.ExecuteNonQuery();
      cmd.CommandText = $"SELECT id, steam_id, amount FROM {Config.Db.Prefix}{Config.Db.TransactionTable}";
      MySqlDataReader reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        int TransactionID = reader.GetInt32(0);
        ulong SteamID = reader.GetUInt64(1);
        decimal Bal = reader.GetDecimal(2);
        base.Logger.LogInformation($"TransactionID: {TransactionID}, SteamID: {SteamID}, Amount: {Bal}");
        var player = Utilities.GetPlayerFromSteamId(SteamID);
        if (player == null)
        {
          base.Logger.LogWarning("Player is null no need for correction");
          continue;
        }
        IPlayerServices? PlayerServices = Capability_PlayerServices.Get(player);
        if (PlayerServices == null)
        {
          base.Logger.LogError("PlayerServices is null");
          continue;
        }
        PlayerServices.Credits += Bal;
        player.PrintToCenter(string.Format(Config.SuccBlUpd, Bal, PlayerServices.Credits));
      }
      reader.Close();
      cmd.CommandText = $"DELETE FROM {Config.Db.Prefix}{Config.Db.TransactionTable}";
      cmd.ExecuteNonQuery();
      cmd.CommandText = $"UNLOCK TABLES";
      cmd.ExecuteNonQuery();
    }, TimerFlags.REPEAT);
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
    T.Kill();
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

  [ConsoleCommand("css_printconfig", "Print config")]
  public void OnPrintConfigCmd(CCSPlayerController? player, CommandInfo info)
  {
    if (player != null)
    {
      return;
    }
    base.Logger.LogInformation(Config.ToString());
  }
}
/*Vim: set expandtab tabstop=4 shiftwidth=4:*/
