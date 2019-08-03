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

			queryBuilderFactory.Create<TestResult>(this, "GetTestResultsByCandidateFirstNameAndTaken")
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.WithCache(TimeSpan.FromSeconds(30))
				.WithParameterBuilder()
				.BeginQuery<Employee>()
					.WithProperty(x => x.FirstName)
					.BuildQueryResult(ctx => ctx.Items["employeeIdList"] = ctx.GetQueryResults<Employee>().Select(x => (object)x.Id).ToList())
				.WithConnectionEdgeBuilder<Candidate>()
					.WithDestinationIdFromSource(ctx => (List<object>)ctx.Items["employeeIdList"])
					.WithProperty(x => x.Taken)
					.BuildConnectionEdgeParameters(ctx =>
					{
						ctx.Items["testResultIdList"] = ctx.GetQueryResults<Candidate>().Select(x => (object)x.Id).ToList();
					})
					.ThenWithQuery<TestResult>()
					.WithPropertyFromSource(x => x.Id, ctx => (List<object>)ctx.Items["testResultIdList"])
				.BuildQueryResult(ctx => { })
				.BuildQuery()
				.BuildWithListResult();

			/*
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
