using System;

namespace Eklee.Exams.Api.Schema
{
	public class AdminSchema : GraphQL.Types.Schema
	{
		public AdminSchema(IServiceProvider resolver, AdminQuery myQuery, AdminMutation myMutation) : base(resolver)
		{
			Query = myQuery;
			Mutation = myMutation;
		}
	}
}
