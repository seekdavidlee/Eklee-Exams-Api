using Eklee.Exams.Api.Security;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Eklee.Exams.Api
{
	public class AdminBearerTokenClient : IAdminBearerTokenClient
	{
		private readonly IConfiguration _configuration;
		private readonly HttpClient _httpClient = new HttpClient();

		public AdminBearerTokenClient(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue()
		{
			var formVariables = new List<KeyValuePair<string, string>>();
			formVariables.Add(new KeyValuePair<string, string>("client_secret", _configuration["AdminClient:ClientSecret"]));
			formVariables.Add(new KeyValuePair<string, string>("client_id", _configuration["AdminClient:ClientId"]));
			formVariables.Add(new KeyValuePair<string, string>("resource", _configuration["AdminClient:Resource"]));
			formVariables.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

			var formData = new FormUrlEncodedContent(formVariables);

			var response = await _httpClient.PostAsync($"https://login.microsoftonline.com/{_configuration["AdminClient:TenantId"]}/oauth2/token", formData);
			response.EnsureSuccessStatusCode();

			var body = await response.Content.ReadAsStringAsync();

			var token = JsonConvert.DeserializeObject<SecurityToken>(body);

			return new AuthenticationHeaderValue("Bearer", token.AccessToken);
		}
	}
}
