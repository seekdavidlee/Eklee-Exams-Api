using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Eklee.Exams.Api
{
	public interface IAdminBearerTokenClient
	{
		Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue();
	}
}
