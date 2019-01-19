using System;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Exams.Api.Schema.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Eklee.Exams.Api.Schema
{
	public class MyMutation : ObjectGraphType
	{
		private readonly IConfiguration _configuration;
		private readonly string _documentDbKey;
		private readonly string _documentDbUrl;
		private readonly string _database;
		private readonly string _requestUnits;
		private readonly string _searchServiceName;
		private readonly string _searchApiKey;

		public MyMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			_configuration = configuration;
			Name = "mutations";

			_documentDbKey = configuration["DocumentDb:Key"];
			_documentDbUrl = configuration["DocumentDb:Url"];
			_database = configuration["DocumentDb:Database"];
			_requestUnits = configuration["DocumentDb:RequestUnits"];

			Add<Exam, ItemWithGuidId>(inputBuilderFactory, builder => builder.AddPartition(x => x.Category));
			Add<Candidate, ItemWithGuidId>(inputBuilderFactory, builder => builder.AddPartition(x => x.Type));
			Add<ExamTemplate, ItemWithGuidId>(inputBuilderFactory, builder => builder.AddPartition(x => x.Category));

			_searchServiceName = configuration["Search:ServiceName"];
			_searchApiKey = configuration["Search:ApiKey"];

			AddSearch<CandidateSearch, Candidate>(inputBuilderFactory, "Candidate search index has been removed.");
			AddSearch<ExamTemplateSearch, ExamTemplate>(inputBuilderFactory, "Exam search template index has been removed.");
		}

		private void AddSearch<TEntity, TModel>(InputBuilderFactory inputBuilderFactory, string deleteMessage) where TEntity : class
		{
			inputBuilderFactory.Create<TEntity>(this)
				.DeleteAll(() => new Status { Message = deleteMessage })
				.ConfigureSearchWith<TEntity, TModel>()
				.AddApiKey(_searchApiKey)
				.AddServiceName(_searchServiceName)
				.BuildSearch()
				.Build();
		}

		private void Add<TEntity, TDeleteEntity>(
			InputBuilderFactory inputBuilderFactory,
			Action<DocumentDbConfiguration<TEntity>> action) where TEntity : class, IEntityWithGuidId, new() where TDeleteEntity : IEntityWithGuidId, new()
		{
			var builder = inputBuilderFactory.Create<TEntity>(this)
				.Delete<TDeleteEntity, Status>(
					input => new TEntity { Id = input.Id },
					item => new Status { Message = $"Successfully removed item with Id {item.Id}" })
				.ConfigureDocumentDb<TEntity>()
				.AddUrl(_documentDbUrl)
				.AddKey(_documentDbKey)
				.AddDatabase(rc => string.IsNullOrEmpty(_database) ? "exams" : _database)
				.AddRequestUnit(string.IsNullOrEmpty(_requestUnits) ? 400 : Convert.ToInt32(_requestUnits));

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
