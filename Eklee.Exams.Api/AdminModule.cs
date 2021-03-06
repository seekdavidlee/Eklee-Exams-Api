﻿using Autofac;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.Http;
using Eklee.Exams.Api.Schema;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Exams.Api
{
	public class AdminModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();
			builder.UseJwtAuthorization<AdminJwtConfig>();
			builder.RegisterGraphQl<AdminSchema>();
			builder.RegisterType<AdminQuery>();
			builder.RegisterType<AdminMutation>();
			builder.RegisterType<OrganizationsRepository>().As<IOrganizationsRepository>();
		}
	}
}
