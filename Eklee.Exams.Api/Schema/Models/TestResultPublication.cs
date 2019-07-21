using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	public class TestResultPublication : IEntityWithGuidId
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of Publication.")]
		public Guid Id { get; set; }

		[ConnectionEdgeDestination]
		[Description("Publication.")]
		public Publication Publication { get; set; }
	}
}
