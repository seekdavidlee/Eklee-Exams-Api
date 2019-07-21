using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Configuration;

namespace Eklee.Exams.Api
{
	/// <summary>
	/// This class is used internally to provide authentication and authorization to the Exam API.
	/// </summary>
	public class MyJwtConfig : IJwtTokenValidatorParameters
	{
		public MyJwtConfig(IConfiguration configuration, IOrganizationsRepository organizationsRepository)
		{
			Audience = configuration["Security:Audience"];

			// Organizations represents AAD tenants. The information is stored in the Admin API which is also a GraphQL endpoint.
			Issuers = organizationsRepository.GetIssuers().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Represents the Exam API application.
		/// </summary>
		public string Audience { get; }

		/// <summary>
		/// This represents the federated AAD instances which represents our customers' identity providers.
		/// </summary>
		public string[] Issuers { get; }
	}
}
