using GraphQL;

namespace Eklee.Exams.Api.Schema
{
	public class MySchema : GraphQL.Types.Schema
	{
		public MySchema(IDependencyResolver resolver, MyQuery myQuery, MyMutation myMutation) : base(resolver)
		{
			Query = myQuery;
			Mutation = myMutation;
		}
	}
}
