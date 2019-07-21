using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	public class ExamPublication : IEntityWithGuidId
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of the Publication.")]
		public Guid Id { get; set; }

		[Description("Date in which exam is published for use.")]
		public DateTime Published { get; set; }

		[Description("Date in which exam is retired so it can no longer be taken.")]
		public DateTime Retired { get; set; }

		[ConnectionEdgeDestination]
		[Description("Publication")]
		public Publication Publication { get; set; }
	}
}
