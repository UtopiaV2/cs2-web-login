using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace cs2_web_login;

public class Cfg : BasePluginConfig
{
	[JsonPropertyName("Database")]
	public DB Db { get; set; } = new DB();

	[JsonPropertyName("PasswordCharLenght")]
	public int bwBlen { get; set; } = 16;

	[JsonPropertyName("ConsoleAlert")]
	public string pwAlert { get; set; } = "CHECK YOUR CONSOLE!";

	[JsonPropertyName("NoLoginCredentialsForDeletionWarn")]
	public string noLogCredForDel { get; set; } = "You have not login credentials to delete! Use /credentials for one!";

	[JsonPropertyName("InfoAboutCredentialsDeletion")]
	public string infoAbtCredDel { get; set; } = "Use /delcredentials to delete your password!";

	[JsonPropertyName("UsernameMessage")]
	public string UnMsg { get; set; } = "Username, a.k.a steamid64: ";

	[JsonPropertyName("PasswordDeletionAndUpdateReminder")]
	public string pwdau { get; set; } = "The password can be removed with /delcredentials. Also you can change the password on the website!"; 

}
// "Jelszó, a /delcredentials ki lehet törölni, ill. a weboldalon betudod állítani a saját jelszavadat"