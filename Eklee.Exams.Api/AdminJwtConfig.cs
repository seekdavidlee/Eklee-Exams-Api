using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Configuration;

namespace Eklee.Exams.Api
{
	public class AdminJwtConfig : IJwtTokenValidatorParameters
	{
		public AdminJwtConfig(IConfiguration configuration)
		{
			Audience = configuration["Admin:Audience"];
			Issuers = new string[] { configuration["Admin:Issuer"] };
		}

		public string Audience { get; }

		public string[] Issuers { get; }
	}
}