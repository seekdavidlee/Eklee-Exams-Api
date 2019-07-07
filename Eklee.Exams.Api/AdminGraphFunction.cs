using Eklee.Azure.Functions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl;

namespace Eklee.Exams.Api
{
	public static class AdminGraphFunction
	{
		[ExecutionContextDependencyInjection(typeof(AdminModule))]
		[FunctionName("appadmin")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "appadmin")] HttpRequest req,
			ILogger log,
			ExecutionContext executionContext)
		{
			return await executionContext.ProcessGraphQlRequest(req);
		}
	}
}
