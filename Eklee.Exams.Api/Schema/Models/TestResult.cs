using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	public class TestResult : IEntityWithGuidId
	{
		[ConnectionEdgeDestinationKey]
		[Description("Id of the employee.")]
		public Guid Id { get; set; }

		[ConnectionEdgeDestination]
		[Description("Employee.")]
		public Employee Employee { get; set; }

		[Description("Date/time of when the exam was taken by employee.")]
		public DateTime Taken { get; set; }

		[Description("Total number of correct answers.")]
		public int CorrectAnswers { get; set; }

		[Description("Total number of questions.")]
		public int TotalQuestions { get; set; }
	}
}
