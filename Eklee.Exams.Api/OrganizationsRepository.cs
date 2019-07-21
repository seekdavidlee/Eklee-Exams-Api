using Eklee.Exams.Api.Schema.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
		private readonly ILogger _logger;

		public OrganizationsRepository(
			IConfiguration configuration,
			IAdminBearerTokenClient adminBearerTokenClient,
			ILogger logger)
		{
			string endpoint = $"{configuration["AdminApi:Endpoint"]}/api/appadmin";
			logger.LogInformation($"Admin endpoint: {endpoint}");
			_client = new GraphQLClient(endpoint);
			_adminBearerTokenClient = adminBearerTokenClient;
			_logger = logger;
		}

		public async Task<string[]> GetIssuers()
		{
			_client.DefaultRequestHeaders.Authorization = await _adminBearerTokenClient.GetAuthenticationHeaderValue();
			_client.Options.MediaType = new MediaTypeHeaderValue("application/json");

			var request = new GraphQLRequest { Query = QueryAllTenants };

			var response = await _client.PostAsync(request);

			AssertError(response);

			JArray items = response.Data.getAllOrganizations as JArray;
			return items.ToObject<Organization[]>().ToList().Select(x => $"https://sts.windows.net/{x.TenantId}/").ToArray();
		}

		private void AssertError(GraphQLResponse response)
		{
			if (response.Errors != null && response.Errors.Length > 0)
			{
				var messages = string.Concat(response.Errors.Select(x => x.Message), "\r\n");
				_logger.LogError(JsonConvert.SerializeObject(response.Errors));
				throw new ApplicationException(messages);
			}
		}
	}
}
