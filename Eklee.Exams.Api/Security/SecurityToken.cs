using Newtonsoft.Json;

namespace Eklee.Exams.Api.Security
{
	public class SecurityToken
	{
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }
	}
}
