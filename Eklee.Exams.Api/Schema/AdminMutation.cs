using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Exams.Api.Schema.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;

namespace Eklee.Exams.Api.Schema
{
	public class AdminMutation : ObjectGraphType
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger _logger;

		private bool DefaultAssertion(ClaimsPrincipal claimsPrincipal, AssertAction assertAction)
		{

			return claimsPrincipal.IsInRole("Eklee.Admin.Writer");
		}

		public AdminMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration, ILogger logger)
		{
			_configuration = configuration;
			_logger = logger;
			_logger.LogInformation("Building mutations.");

			Add<Organization, ItemWithGuidId>(inputBuilderFactory, builder => builder.AddPartition(x => x.Type));
		}

		private void Add<TEntity, TDeleteEntity>(
			InputBuilderFactory inputBuilderFactory,
			Action<DocumentDbConfiguration<TEntity>> action) where TEntity : class, IEntityWithGuidId, new() where TDeleteEntity : IEntityWithGuidId, new()
		{
			var configuration = _configuration.GetSection("Admin");
			var issuer = configuration["Issuer"];

			_logger.LogInformation($"Setting up admin: {issuer}.");

			string adminDocumentDbKey = configuration["DocumentDb:Key"];
			string adminDocumentDbUrl = configuration["DocumentDb:Url"];
			int adminRequestUnits = Convert.ToInt32(configuration["DocumentDb:RequestUnits"]);

			string databaseName = "admindb";

			var builder = inputBuilderFactory.Create<TEntity>(this)
				.AssertWithClaimsPrincipal(DefaultAssertion)
				.Delete<TDeleteEntity, Status>(
					input => new TEntity { Id = input.Id },
					item => new Status { Message = $"Successfully removed item with Id {item.Id}" })
				.ConfigureDocumentDb<TEntity>()
				.AddGraphRequestContextSelector(ctx => ctx.ContainsIssuer(issuer))
				.AddUrl(adminDocumentDbUrl)
				.AddKey(adminDocumentDbKey)
				.AddDatabase(databaseName)
				.AddRequestUnit(adminRequestUnits);

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
