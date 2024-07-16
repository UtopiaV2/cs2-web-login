using System;
using System.Security.Cryptography;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using BC = BCrypt.Net;

namespace cs2_web_login;

public class Class1 : BasePlugin, IPluginConfig<Cfg>
{
	public Database? db;
	public override string ModuleName => "WebLogin";
	public override string ModuleVersion => "v1.3.0";
	public override string ModuleAuthor => "OwnSample";
	public override string ModuleDescription => "Creates logins for cry babies! :D";
	public Cfg Config { get; set; } = new Cfg();

	public override void Load(bool hotReload) 
	{
		db = new Database($"Server={Config.Db.Host};User ID={Config.Db.User};Password={Config.Db.Password};Database={Config.Db.Database}", base.Logger);
	}
	public void OnConfigParsed(Cfg config)
	{
		Config = config;
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
		cmd.CommandText = $"SELECT t.steam_id FROM upm_user t WHERE t.steam_id = {player.SteamID}";
		MySqlDataReader reader = cmd.ExecuteReader();
		if (reader.RecordsAffected == 0)
		{
			player.PrintToCenterAlert(Config.noLogCredForDel);
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
		MySqlDataReader reader = cmd.ExecuteReader();
		if (reader.Read())
		{
			ulong SteamID = reader.GetUInt64(0);
			if (SteamID != player.SteamID)
			{
				throw new Exception("How tf was player.steamId and what we got back is not the same");
			}
			player.PrintToCenterAlert(Config.infoAbtCredDel);
			return;
		}
		reader.Close();
		byte[] pw = RandomNumberGenerator.GetBytes(Config.bwBlen);
		string pwS = Convert.ToBase64String(pw);
		cmd.CommandText = $"INSERT INTO upm_user VALUE ({player.SteamID}, \"{pwS}\", {player.SteamID})";
		var hash = BC.BCrypt.HashPassword(pwS, workFactor: Config.bc_workfactor).Replace("$2a$", "$2y$"); // upgrade hash xd
		cmd.CommandText = $"INSERT INTO {Config.Db.Prefix}{Config.Db.Table} (steam_id, pw, username) VALUE ({player.SteamID}, \"{hash}\", {player.SteamID})";
		cmd.ExecuteNonQuery();
		player.PrintToCenterAlert(Config.pwAlert);
		player.PrintToConsole("---------------------------------------------------------------");
		player.PrintToConsole(Config.UnMsg);
		player.PrintToConsole(player.SteamID.ToString());
		player.PrintToConsole(Config.pwdau);
		player.PrintToConsole(pwS);
		player.PrintToConsole("---------------------------------------------------------------");
	}
}
