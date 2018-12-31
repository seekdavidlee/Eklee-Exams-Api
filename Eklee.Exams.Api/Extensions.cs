using Microsoft.Extensions.Configuration;

namespace Eklee.Exams.Api
{
	public static class Extensions
	{
		public static bool IsLocalEnvironment(this IConfiguration configuration)
		{
			var value = configuration.GetValue<string>("AzureWebJobsStorage");
			return value == "UseDevelopmentStorage=true";
		}
	}
}
