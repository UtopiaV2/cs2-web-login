using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace cs2_web_login;

public class Cfg : BasePluginConfig
{
  [JsonPropertyName("Database")]
  public DB Db { get; set; } = new DB();

  [JsonPropertyName("Interval")]
  public float Interval { get; set; } = 30f;

  [JsonPropertyName("AutoUpdate")]
  public bool AutoUpdate { get; set; } = true;

  [JsonPropertyName("ReleaseCanidate")]
  public bool Rc { get; set; } = false;

  [JsonPropertyName("PasswordCharLenght")]
  public int BwBlen { get; set; } = 16;

  [JsonPropertyName("BCryprtCost")]
  public int Bc_workfactor { get; set; } = 12;

  [JsonPropertyName("ConsoleAlert")]
  public string PwAlert { get; set; } = "CHECK YOUR CONSOLE!";

  [JsonPropertyName("NoLoginCredentialsForDeletionWarn")]
  public string NoLogCredForDel { get; set; } = "You have not login credentials to delete! Use /credentials for one!";

  [JsonPropertyName("InfoAboutCredentialsDeletion")]
  public string InfoAbtCredDel { get; set; } = "Use /delcredentials to delete your password!";

  [JsonPropertyName("UsernameMessage")]
  public string UnMsg { get; set; } = "Username, a.k.a steamid64, you can change this later: ";

  [JsonPropertyName("PasswordDeletionAndUpdateReminder")]
  public string Pwdau { get; set; } = "The password can be removed with /delcredentials. Also you can change the password on the website!";

  [JsonPropertyName("SuccessfulDeleteion")]
  public string SuccDel { get; set; } = "The credentials has been deleted!";

  [JsonPropertyName("SuccessfulBalUpdate")]
  public string SuccBlUpd { get; set; } = "You have received {0} credits!\nNow you have: {1} credits!";

  public override string ToString()
  {
    return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
  }
}
// "Jelszó, a /delcredentials ki lehet törölni, ill. a weboldalon betudod állítani a saját jelszavadat"
/*Vim: set expandtab tabstop=4 shiftwidth=4:*/

