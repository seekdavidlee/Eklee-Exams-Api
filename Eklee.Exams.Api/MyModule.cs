using Autofac;
using Eklee.Azure.Functions.GraphQl;
using Eklee.Azure.Functions.Http;
using Eklee.Exams.Api.Schema;
using Microsoft.Extensions.Caching.Distributed;

namespace Eklee.Exams.Api
{
	public class MyModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.UseDistributedCache<MemoryDistributedCache>();

			builder.RegisterGraphQl<MySchema>();
			builder.RegisterType<MyQuery>();
			builder.RegisterType<MyMutation>();
		}
	}
}
