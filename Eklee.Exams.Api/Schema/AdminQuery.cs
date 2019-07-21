using Eklee.Azure.Functions.GraphQl;
using Eklee.Exams.Api.Schema.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;

namespace Eklee.Exams.Api.Schema
{
	public class AdminQuery : ObjectGraphType<object>
	{
		private readonly IConfiguration _configuration;

		private bool DefaultAssertion(ClaimsPrincipal claimsPrincipal)
		{
			var serviceAccounts = _configuration.GetSection("ServiceAccounts").GetChildren().ToList();
			if (serviceAccounts.Count > 0)
			{
				if (serviceAccounts.Any(sa =>
					claimsPrincipal.Claims.Any(x => x.Type == "appid" && x.Value == sa["AppId"]) &&
					claimsPrincipal.Claims.Any(x => x.Type == "iss" && x.Value == sa["Issuer"])))
				{
					return true;
				}
			}

			return claimsPrincipal.IsInRole("Eklee.Admin.Reader");
		}

		public AdminQuery(QueryBuilderFactory queryBuilderFactory,
			ILogger logger,
			IConfiguration configuration)
		{
			logger.LogInformation("Building queries.");

			Name = "query";

			queryBuilderFactory.Create<Organization>(this, "GetOrganizationById")
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.WithParameterBuilder()
				.WithKeys()
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<Organization>(this, "GetAllOrganizations")
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.WithParameterBuilder()
				.BuildQuery()
				.BuildWithListResult();
			_configuration = configuration;
		}
	}
}
