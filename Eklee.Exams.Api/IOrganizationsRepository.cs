using System.Threading.Tasks;

namespace Eklee.Exams.Api
{
	public interface IOrganizationsRepository
	{
		Task<string[]> GetIssuers();
	}
}
