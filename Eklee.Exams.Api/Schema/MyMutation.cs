using System;
using System.Security.Claims;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.GraphQl.Connections;
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
			return claimsPrincipal.IsInRole("Eklee.User.Writer");
		}

		public MyMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration, IOrganizationsRepository organizationsRepository, ILogger logger)
		{
			_configuration = configuration;
			_logger = logger;
			_logger.LogInformation("Building app mutations.");

			Name = "mutations";

			var issuers = organizationsRepository.GetIssuers().GetAwaiter().GetResult();
			if (issuers.Length == 0)
			{
				throw new ArgumentException("Issuer(s) not configured.");
			}

			_logger.LogInformation($"{issuers.Length} issuers are configured.");

			Add<Employee, ItemWithGuidId>(issuers, inputBuilderFactory, builder => builder.AddPartition(x => x.Department));
			Add<Exam, ItemWithGuidId>(issuers, inputBuilderFactory, builder => builder.AddPartition(x => x.Category));
			Add<Publication, ItemWithGuidId>(issuers, inputBuilderFactory, builder => builder.AddPartition(x => x.Name));
			AddSearch<ExamSearch, Exam>(issuers, inputBuilderFactory, "Exam search template index has been removed.");

			string key = _configuration["DocumentDb:Key"];
			string url = _configuration["DocumentDb:Url"];
			int requestUnits = Convert.ToInt32(_configuration["DocumentDb:RequestUnits"]);

			foreach (var issuer in issuers)
			{
				string db = issuer.GetTenantIdFromIssuer();

				db = _configuration.IsLocalEnvironment() ? $"lcl{db}" :
					(_configuration["EnableDeleteAll"] == "true" ? $"stg{db}" : db);

				inputBuilderFactory.Create<ConnectionEdge>(this)
					.ConfigureDocumentDb<ConnectionEdge>()
					.AddGraphRequestContextSelector(ctx => ctx.ContainsIssuer(issuer))
					.AddKey(key)
					.AddUrl(url)
					.AddRequestUnit(requestUnits)
					.AddDatabase(db)
					.AddPartition(x => x.SourceId)
					.BuildDocumentDb()
					.DeleteAll(() => new Status { Message = "All ConnectionEdges have been removed." })
					.Build();
			}
		}

		private void AddSearch<TEntity, TModel>(string[] issuers,
			InputBuilderFactory inputBuilderFactory, string deleteMessage) where TEntity : class
		{
			string tenantSearchApiKey = _configuration["Search:ApiKey"];
			string tenantServiceName = _configuration["Search:ServiceName"];

			foreach (var issuer in issuers)
			{
				var builder = inputBuilderFactory.Create<TEntity>(this)
					.AssertWithClaimsPrincipal(DefaultAssertion)
					.DeleteAll(() => new Status { Message = deleteMessage })
					.ConfigureSearchWith<TEntity, TModel>()
					.AddGraphRequestContextSelector(ctx => ctx.ContainsIssuer(issuer))
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
			}
		}

		private void Add<TEntity, TDeleteEntity>(
			string[] issuers,
			InputBuilderFactory inputBuilderFactory,
			Action<DocumentDbConfiguration<TEntity>> action) where TEntity : class, IEntityWithGuidId, new() where TDeleteEntity : IEntityWithGuidId, new()
		{
			string tenantDocumentDbKey = _configuration["DocumentDb:Key"];
			string tenantDocumentDbUrl = _configuration["DocumentDb:Url"];
			int tenantRequestUnits = Convert.ToInt32(_configuration["DocumentDb:RequestUnits"]);

			foreach (var issuer in issuers)
			{
				_logger.LogInformation($"Setting up tenant: {issuer}.");

				string databaseName = issuer.GetTenantIdFromIssuer();
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
			}
		}
	}
}
