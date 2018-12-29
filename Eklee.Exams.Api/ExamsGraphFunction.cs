using System.Threading.Tasks;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Eklee.Exams.Api
{
    public static class ExamsGraphFunction
    {
	    [ExecutionContextDependencyInjection(typeof(MyModule))]
	    [FunctionName("graph")]
	    public static async Task<IActionResult> Run(
		    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "graph")] HttpRequest req,
		    ILogger log,
		    ExecutionContext executionContext)
	    {
		    return await executionContext.ProcessGraphQlRequest(req);
	    }
	}
}
