using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Eklee.Exams.Api.Schema.Models;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Exams.Api.Schema
{
	public class MyQuery : ObjectGraphType<object>
	{
		private bool DefaultAssertion(ClaimsPrincipal claimsPrincipal)
		{
			return claimsPrincipal.IsInRole("Eklee.User.Reader");
		}

		public MyQuery(QueryBuilderFactory queryBuilderFactory, ILogger logger)
		{
			logger.LogInformation("Building queries.");

			Name = "query";

			queryBuilderFactory.Create<Exam>(this, "GetExamById")
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.WithParameterBuilder()
				.WithKeys()
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<TestResult>(this, "GetTestResultById")
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.WithParameterBuilder()
				.WithKeys()
				.BuildQuery()
				.BuildWithSingleResult();

			queryBuilderFactory.Create<TestResult>(this, "GetTestResultByCandidateById")
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.WithParameterBuilder()
				.WithConnectionEdgeBuilder<Candidate>()
					.WithDestinationId()
					.BuildConnectionEdgeParameters()
				.BuildQuery()
				.BuildWithSingleResult();
			/*
			queryBuilderFactory.Create<TestResultOutput>(this, "GetTestResultsByNameAndTaken")
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.WithCache(TimeSpan.FromSeconds(30))
				.WithParameterBuilder()
				.BeginQuery<TestResult>()
					.WithProperty(x => x.Name)
					.WithProperty(x => x.Taken)
					.BuildQueryResult(ctx => ctx.Items["exams"] = ctx.GetQueryResults<TestResult>())
				.ThenWithQuery<Exam>()
					.WithPropertyFromSource(x => x.Id,
						x => ((List<TestResult>)x.Items["exams"]).Select(y => (object)y.ExamId).Distinct().ToList())
					.BuildQueryResult(ctx =>
					{
						List<TestResult> exams = (List<TestResult>)ctx.Items["exams"];
						var exampleTemplates = ctx.GetQueryResults<Exam>();
						ctx.SetResults(exams.Select(exam => new TestResultOutput
						{
							Id = exam.Id,
							CandidateId = exam.CandidateId,
							Category = exam.Category,
							ExamId = exam.ExamId,
							Name = exam.Name,
							Taken = exam.Taken,
							Exam = exampleTemplates.Single(x => x.Id == exam.ExamId)
						}).ToList());
					})
				.ThenWithQuery<Candidate>()
				.WithPropertyFromSource(x => x.Id,
					ctx => (ctx.GetResults<TestResultOutput>()).Select(y => (object)y.CandidateId).ToList())
					.BuildQueryResult(ctx =>
				{
					var candidates = ctx.GetQueryResults<Candidate>();
					ctx.GetResults<TestResultOutput>().ForEach(x => x.Candidate = candidates.Single(c => c.Id == x.CandidateId));
				})
				.BuildQuery()
				.BuildWithListResult();

			queryBuilderFactory.Create<TestResultOutput>(this, "SearchExams")
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.WithCache(TimeSpan.FromSeconds(30))
				.WithParameterBuilder()
				.BeginSearch(typeof(CandidateSearch), typeof(ExamSearch))
					.BuildQueryResult(ctx =>
					{
						var searches = ctx.GetQueryResults<SearchResultModel>();
						ctx.Items["examSearchesIdList"] = searches.GetTypeList<ExamSearch>().Select(x => (object)x.Id).ToList();
						ctx.Items["candidateSearchesIdList"] = searches.GetTypeList<CandidateSearch>().Select(x => (object)x.Id).ToList();
					})
				.ThenWithQuery<TestResult>()
					.WithPropertyFromSource(x => x.CandidateId, ctx => (List<object>)ctx.Items["candidateSearchesIdList"])
					.BuildQueryResult(ctx => ctx.Items["examsOfCandidates"] = ctx.GetQueryResults<TestResult>())
				.ThenWithQuery<TestResult>()
					.WithPropertyFromSource(x => x.ExamId, ctx => (List<object>)ctx.Items["examSearchesIdList"])
					.BuildQueryResult(ctx =>
					{
						var exams = ctx.GetQueryResults<TestResult>();
						exams.AddRange((List<TestResult>)ctx.Items["examsOfCandidates"]);
						var results = exams.Distinct().Select(x => new TestResultOutput
						{
							Id = x.Id,
							CandidateId = x.CandidateId,
							Category = x.Category,
							Name = x.Name,
							ExamId = x.ExamId,
							Taken = x.Taken
						}).ToList();

						ctx.Items["candidateIdList"] = results.Select(x => (object)x.CandidateId).Distinct().ToList();
						ctx.Items["examIdList"] = results.Select(x => (object)x.ExamId).Distinct().ToList();

						ctx.SetResults(results);
					})
				.ThenWithQuery<Candidate>()
					.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["candidateIdList"])
					.BuildQueryResult(ctx => ctx.GetResults<TestResultOutput>().ForEach(x => x.Candidate = ctx.GetQueryResults<Candidate>().Single(c => c.Id == x.CandidateId)))
				.ThenWithQuery<Exam>()
					.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["examIdList"])
					.BuildQueryResult(ctx => ctx.GetResults<TestResultOutput>().ForEach(x => x.Exam = ctx.GetQueryResults<Exam>().Single(e => e.Id == x.ExamId)))
				.BuildQuery().BuildWithListResult();
			*/
		}
	}
}
