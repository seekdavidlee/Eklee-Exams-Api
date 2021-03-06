﻿using Eklee.Exams.Api.Schema.Models;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eklee.Exams.Api
{
	public class AllOrganizations
	{
		public List<Organization> getAllOrganizations { get; set; }
	}
	public class OrganizationsRepository : IOrganizationsRepository
	{
		private const string QueryAllTenants = @"query {
		  getAllOrganizations{
			id
			tenantId
		  }
		}";

		private readonly GraphQLHttpClient _client;
		private readonly IAdminBearerTokenClient _adminBearerTokenClient;
		private readonly ILogger _logger;

		public OrganizationsRepository(
			IConfiguration configuration,
			IAdminBearerTokenClient adminBearerTokenClient,
			ILogger logger)
		{
			string endpoint = $"{configuration["AdminApi:Endpoint"]}/api/appadmin";
			logger.LogInformation($"Admin endpoint: {endpoint}");
			_client = new GraphQLHttpClient(endpoint, new NewtonsoftJsonSerializer());
			_adminBearerTokenClient = adminBearerTokenClient;
			_logger = logger;
		}

		public async Task<string[]> GetIssuers()
		{
			_client.HttpClient.DefaultRequestHeaders.Authorization = await _adminBearerTokenClient.GetAuthenticationHeaderValue();
			_client.Options.MediaType = "application/json";

			var request = new GraphQLRequest { Query = QueryAllTenants };

			var response = await _client.SendQueryAsync<AllOrganizations>(request);

			AssertError(response);

			if (response.Data.getAllOrganizations.Count == 0)
			{
				throw new ApplicationException("No issuers found!");
			}

			return response.Data.getAllOrganizations.Select(x => $"https://sts.windows.net/{x.TenantId}/").ToArray();
		}

		private void AssertError(GraphQLResponse<AllOrganizations> response)
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
