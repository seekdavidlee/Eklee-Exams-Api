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

			queryBuilderFactory.Create<TestResult>(this, "SearchExams")
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.WithCache(TimeSpan.FromSeconds(30))
				.WithParameterBuilder()
				.BeginSearch()
				.Add<EmployeeSearch>().Add<ExamSearch>().Build()
					.BuildQueryResult(ctx =>
					{
						var searches = ctx.GetQueryResults<SearchResultModel>();
						ctx.Items["examIdList"] = searches.GetTypeList<ExamSearch>().Select(x => (object)x.Id).ToList();
						ctx.Items["employeeIdList"] = searches.GetTypeList<EmployeeSearch>().Select(x => (object)x.Id).ToList();
					})
				.WithConnectionEdgeBuilder<Candidate>()
					.WithDestinationIdFromSource(ctx => (List<object>)ctx.Items["employeeIdList"])
					.BuildConnectionEdgeParameters(ctx =>
					{
						// Source Id refers to the TestResult.
						ctx.Items["testResultIdList"] = ctx.GetResults<ConnectionEdge>()
							.Select(x => (object)x.SourceId).ToList();
					})
				.WithConnectionEdgeBuilder<ExamPublication>()
					.WithSourceIdFromSource<Exam>(ctx => (List<object>)ctx.Items["examIdList"])
					.BuildConnectionEdgeParameters(ctx =>
					{
						// Use Publication Id to find TestResult
						ctx.Items["publicationIdList"] = ctx.GetResults<ConnectionEdge>()
							.Select(x => (object)x.DestinationId).ToList();
					})
				.WithConnectionEdgeBuilder<TestResultPublication>()
					.WithDestinationIdFromSource(ctx => (List<object>)ctx.Items["publicationIdList"])
					.BuildConnectionEdgeParameters(ctx =>
					{
						var list = ((List<object>)ctx.Items["testResultIdList"]).Select(x => (string)x).ToList();
						list.AddRange(ctx.GetResults<ConnectionEdge>().Select(x => x.SourceId));
						ctx.Items["testResultIdList"] = list.Distinct().Select(x => (object)x).ToList();
					})
				// WithConnectionEdgeBuilder allows us to search back to TestResult using the WithSourceIdFromSource.
				// Don't use WithPropertyFromSource as it doesn't allow us to query correctly on connections.
				.WithConnectionEdgeBuilder<TestResultPublication>()
					.WithSourceIdFromSource<TestResult>(ctx => (List<object>)ctx.Items["testResultIdList"])
					.BuildConnectionEdgeParameters(ctx => { })
				.BuildQuery().BuildWithListResult();
		}
	}
}
