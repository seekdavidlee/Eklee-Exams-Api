using System;
using System.Linq;
using System.Security.Claims;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Exams.Api.Schema.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eklee.Exams.Api.Schema
{
	public class MyMutation : ObjectGraphType
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;

		private bool DefaultAssertion(ClaimsPrincipal claimsPrincipal, AssertAction assertAction)
		{
			return claimsPrincipal.IsInRole("Eklee.User.Write");
		}

		public MyMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration, ILogger logger)
		{
			_configuration = configuration;
			_logger = logger;
			Name = "mutations";

			Add<Exam, ItemWithGuidId>(inputBuilderFactory, builder => builder.AddPartition(x => x.Category));
			Add<Candidate, ItemWithGuidId>(inputBuilderFactory, builder => builder.AddPartition(x => x.Type));
			Add<ExamTemplate, ItemWithGuidId>(inputBuilderFactory, builder => builder.AddPartition(x => x.Category));

			AddSearch<CandidateSearch, Candidate>(inputBuilderFactory, "Candidate search index has been removed.");
			AddSearch<ExamTemplateSearch, ExamTemplate>(inputBuilderFactory, "Exam search template index has been removed.");
		}

		private void AddSearch<TEntity, TModel>(InputBuilderFactory inputBuilderFactory, string deleteMessage) where TEntity : class
		{
			var tenants = _configuration.GetSection("Tenants").GetChildren().ToList();

			if (tenants.Count == 0)
			{
				_logger.LogError("Tenants not set!");
			}

			tenants.ForEach(tenant =>
			{
				string tenantSearchApiKey = tenant["Search:ApiKey"];
				string tenantServiceName = tenant["Search:ServiceName"];

				var builder = inputBuilderFactory.Create<TEntity>(this)
					.AssertWithClaimsPrincipal(DefaultAssertion)
					.DeleteAll(() => new Status { Message = deleteMessage })
					.ConfigureSearchWith<TEntity, TModel>()
					.AddGraphRequestContextSelector(ctx => ctx.ContainsIssuer(tenant["Issuer"]))
					.AddApiKey(tenantSearchApiKey)
					.AddServiceName(tenantServiceName);

				var prefix = _configuration.IsLocalEnvironment()
					? "lcl"
					: (_configuration["EnableDeleteAll"] == "true" ? "stg" : "");

				if (!string.IsNullOrEmpty(prefix))
				{
					builder.AddPrefix(prefix);
				}

				builder.BuildSearch().Build();
			});
		}

		private void Add<TEntity, TDeleteEntity>(
			InputBuilderFactory inputBuilderFactory,
			Action<DocumentDbConfiguration<TEntity>> action) where TEntity : class, IEntityWithGuidId, new() where TDeleteEntity : IEntityWithGuidId, new()
		{
			var tenants = _configuration.GetSection("Tenants").GetChildren().ToList();
			tenants.ForEach(tenant =>
			{
				var issuer = tenant["Issuer"];

				_logger.LogInformation($"Setting up tenant: {issuer}.");

				string tenantDocumentDbKey = tenant["DocumentDb:Key"];
				string tenantDocumentDbUrl = tenant["DocumentDb:Url"];
				int tenantRequestUnits = Convert.ToInt32(tenant["DocumentDb:RequestUnits"]);

				var databaseName = issuer.GetTenantIdFromIssuer();
				databaseName = _configuration.IsLocalEnvironment() ? $"lcl{databaseName}" :
					(_configuration["EnableDeleteAll"] == "true" ? $"stg{databaseName}" : databaseName);

				var builder = inputBuilderFactory.Create<TEntity>(this)
					.AssertWithClaimsPrincipal(DefaultAssertion)
					.Delete<TDeleteEntity, Status>(
						input => new TEntity { Id = input.Id },
						item => new Status { Message = $"Successfully removed item with Id {item.Id}" })
					.ConfigureDocumentDb<TEntity>()
					.AddGraphRequestContextSelector(ctx => ctx.ContainsIssuer(issuer))
					.AddUrl(tenantDocumentDbUrl)
					.AddKey(tenantDocumentDbKey)
					.AddDatabase(databaseName)
					.AddRequestUnit(tenantRequestUnits);

				action?.Invoke(builder);

				var model = builder.BuildDocumentDb();

				// DeleteAll is only applicable in local testing environment.
				if (_configuration.IsLocalEnvironment() || _configuration["EnableDeleteAll"] == "true")
				{
					model.DeleteAll(() => new Status { Message = "All entities are removed." });
				}

				model.Build();
			});
		}
	}
}
