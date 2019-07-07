using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	[Description("Candidate Relationship")]
	public class Candidate : IEntityWithGuidId
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of the Employee.")]
		public Guid Id { get; set; }

		[Description("Determines whether the candidate is active.")]
		public bool Active { get; set; }

		[Description("Date/time of when the candidate account was created.")]
		public DateTime Created { get; set; }

		[Description("Type of candidate. Either student, full time employed, part time employed.")]
		public string Type { get; set; }

		[ConnectionEdgeDestination]
		[Description("User details.")]
		public Employee User { get; set; }
	}
}
