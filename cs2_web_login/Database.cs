using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace cs2_web_login;

public class Database
{
	
	readonly private string ConnectionStr;

	readonly private ILogger Logger;

	public Database(string dbConnectionString, ILogger logger)
	{
		ConnectionStr = dbConnectionString;
		this.Logger = logger;
		//base._002Ector();
	}

	public MySqlConnection GetConnection()
	{
		try
		{
			MySqlConnection connection = new(ConnectionStr);
			connection.Open();
			return connection;
		}
		catch (Exception ex)
		{
			Logger.LogCritical(ex.ToString());
			throw;
		}
	}

	public async Task<MySqlConnection> GetConnectionAsync()
	{
		try
		{
			MySqlConnection connection = new(ConnectionStr);
			await connection.OpenAsync();
			return connection;
		}
		catch (Exception ex)
		{
			Logger.LogCritical(ex.ToString());
			throw;
		}
	}

	public bool CheckDatabaseConnection()
	{
		using MySqlConnection connection = GetConnection();
		try
		{
			return connection.Ping();
		}
		catch
		{
			return false;
		}
	}
}
