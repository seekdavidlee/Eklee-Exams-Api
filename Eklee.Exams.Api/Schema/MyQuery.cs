using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
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
						x => ((List<Exam>)x.Items["exams"]).Select(y => (object)y.ExamTemplateId).Distinct().ToList())
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

			queryBuilderFactory.Create<ExamOutput>(this, "SearchExams")
				.WithCache(TimeSpan.FromSeconds(30))
				.WithParameterBuilder()
				.BeginSearch(typeof(CandidateSearch), typeof(ExamTemplateSearch))
					.BuildQueryResult(ctx =>
					{
						var searches = ctx.GetQueryResults<SearchResultModel>();
						ctx.Items["examTemplateSearchesIdList"] = searches.GetTypeList<ExamTemplateSearch>().Select(x => (object)x.Id).ToList();
						ctx.Items["candidateSearchesIdList"] = searches.GetTypeList<CandidateSearch>().Select(x => (object)x.Id).ToList();
					})
				.ThenWithQuery<Exam>()
					.WithPropertyFromSource(x => x.CandidateId, ctx => (List<object>)ctx.Items["candidateSearchesIdList"])
					.BuildQueryResult(ctx => ctx.Items["examsOfCandidates"] = ctx.GetQueryResults<Exam>())
				.ThenWithQuery<Exam>()
					.WithPropertyFromSource(x => x.ExamTemplateId, ctx => (List<object>)ctx.Items["examTemplateSearchesIdList"])
					.BuildQueryResult(ctx =>
					{
						var exams = ctx.GetQueryResults<Exam>();
						exams.AddRange((List<Exam>)ctx.Items["examsOfCandidates"]);
						var results = exams.Distinct().Select(x => new ExamOutput
						{
							Id = x.Id,
							CandidateId = x.CandidateId,
							Category = x.Category,
							Name = x.Name,
							ExamTemplateId = x.ExamTemplateId,
							Taken = x.Taken
						}).ToList();

						ctx.Items["candidateIdList"] = results.Select(x => (object)x.CandidateId).Distinct().ToList();
						ctx.Items["examTemplateIdList"] = results.Select(x => (object)x.ExamTemplateId).Distinct().ToList();

						ctx.SetResults(results);
					})
				.ThenWithQuery<Candidate>()
					.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["candidateIdList"])
					.BuildQueryResult(ctx => ctx.GetResults<ExamOutput>().ForEach(x => x.Candidate = ctx.GetQueryResults<Candidate>().Single(c => c.Id == x.CandidateId)))
				.ThenWithQuery<ExamTemplate>()
					.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["examTemplateIdList"])
					.BuildQueryResult(ctx => ctx.GetResults<ExamOutput>().ForEach(x => x.ExamTemplate = ctx.GetQueryResults<ExamTemplate>().Single(e => e.Id == x.ExamTemplateId)))
				.BuildQuery().BuildWithListResult();
		}
	}
}
