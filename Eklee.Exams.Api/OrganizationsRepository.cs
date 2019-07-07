using Eklee.Exams.Api.Schema.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Eklee.Exams.Api
{
	public class OrganizationsRepository : IOrganizationsRepository
	{
		private const string QueryAllTenants = @"query {
		  getAllOrganizations{
			id
			tenantId
		  }
		}";

		private readonly GraphQLClient _client;
		private readonly IAdminBearerTokenClient _adminBearerTokenClient;

		public OrganizationsRepository(
			IConfiguration configuration,
			IAdminBearerTokenClient adminBearerTokenClient)
		{
			_client = new GraphQLClient(configuration["AdminApi:Endpoint"]);
			_adminBearerTokenClient = adminBearerTokenClient;
		}

		public async Task<string[]> GetIssuers()
		{
			_client.DefaultRequestHeaders.Authorization = await _adminBearerTokenClient.GetAuthenticationHeaderValue();
			_client.Options.MediaType = new MediaTypeHeaderValue("application/json");

			var request = new GraphQLRequest { Query = QueryAllTenants };

			var results = await _client.PostAsync(request);
			JArray items = results.Data.getAllOrganizations as JArray;
			return items.ToObject<Organization[]>().ToList().Select(x => $"https://sts.windows.net/{x.TenantId}/").ToArray();
		}
	}
}
