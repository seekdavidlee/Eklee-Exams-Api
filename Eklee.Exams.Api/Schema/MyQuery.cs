using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Exams.Api.Schema.Models;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Exams.Api.Schema
{
	public class MyQuery : ObjectGraphType<object>
	{
		public MyQuery(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Building queries.");

			Name = "query";

			queryBuilderFactory.Create<Exam>(this, "GetExamById")
				.WithParameterBuilder()
				.WithKeys()
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<ExamTemplate>(this, "GetExamTemplateById")
				.WithParameterBuilder()
				.WithKeys()
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<Candidate>(this, "GetCandidateById")
				.WithParameterBuilder()
				.WithKeys()
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<ExamOutput>(this, "GetExamsByNameAndTaken")
				.WithCache(TimeSpan.FromSeconds(30))
				.WithParameterBuilder()
				.BeginQuery<Exam>()
					.WithProperty(x => x.Name)
					.WithProperty(x => x.Taken)
					.BuildQueryResult(ctx => ctx.Items["exams"] = ctx.GetQueryResults<Exam>())
				.ThenWithQuery<ExamTemplate>()
					.WithPropertyFromSource(x => x.Id,
						x => ((List<Exam>)x.Items["exams"]).Select(y => (object)y.ExamTemplateId).ToList())
					.WithProperty(x => x.Category)
					.BuildQueryResult(ctx =>
					{
						List<Exam> exams = (List<Exam>)ctx.Items["exams"];
						var exampleTemplates = ctx.GetQueryResults<ExamTemplate>();
						ctx.SetResults(exams.Select(exam => new ExamOutput
						{
							Id = exam.Id,
							CandidateId = exam.CandidateId,
							Category = exam.Category,
							ExamTemplateId = exam.ExamTemplateId,
							Name = exam.Name,
							Taken = exam.Taken,
							ExamTemplate = exampleTemplates.Single(x => x.Id == exam.ExamTemplateId)
						}).ToList());
					})
				.ThenWithQuery<Candidate>()
				.WithPropertyFromSource(x => x.Id,
					ctx => (ctx.GetResults<ExamOutput>()).Select(y => (object)y.CandidateId).ToList())
					.BuildQueryResult(ctx =>
				{
					var candidates = ctx.GetQueryResults<Candidate>();
					ctx.GetResults<ExamOutput>().ForEach(x => x.Candidate = candidates.Single(c => c.Id == x.CandidateId));
				})
				.BuildQuery()
				.BuildWithListResult();
		}
	}
}
