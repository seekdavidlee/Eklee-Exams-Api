using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Configuration;

namespace Eklee.Exams.Api
{
	public class MyJwtConfig : IJwtTokenValidatorParameters
	{
		public MyJwtConfig(IConfiguration configuration, IOrganizationsRepository organizationsRepository)
		{
			Audience = configuration["Security:Audience"];

			Issuers = organizationsRepository.GetIssuers().GetAwaiter().GetResult();
		}

		public string Audience { get; }

		public string[] Issuers { get; }
	}
}
