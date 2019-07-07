using GraphQL;

namespace Eklee.Exams.Api.Schema
{
	public class AdminSchema : GraphQL.Types.Schema
	{
		public AdminSchema(IDependencyResolver resolver, AdminQuery myQuery, AdminMutation myMutation) : base(resolver)
		{
			Query = myQuery;
			Mutation = myMutation;
		}
	}
}
