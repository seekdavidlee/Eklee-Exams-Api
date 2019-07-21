using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	public class Candidate : IEntityWithGuidId
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of Employee.")]
		public Guid Id { get; set; }

		[Description("Date/time of when the exam was taken by employee.")]
		public DateTime Taken { get; set; }

		[ConnectionEdgeDestination]
		[Description("Employee.")]
		public Employee Employee { get; set; }
	}
}
