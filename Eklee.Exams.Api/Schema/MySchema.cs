using System;

namespace Eklee.Exams.Api.Schema
{
	public class MySchema : GraphQL.Types.Schema
	{
		public MySchema(IServiceProvider resolver, MyQuery myQuery, MyMutation myMutation) : base(resolver)
		{
			Query = myQuery;
			Mutation = myMutation;
		}
	}
}
