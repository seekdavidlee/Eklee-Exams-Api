using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class TestResult : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the test result.")]
		public Guid Id { get; set; }

		[Description("Location of where the test was taken.")]
		public string Location { get; set; }

		[Connection]
		[Description("Candidate")]
		public Candidate Candidate { get; set; }

		[Description("Total number of correct answers.")]
		public int CorrectAnswers { get; set; }

		[Description("Total number of questions.")]
		public int TotalQuestions { get; set; }

		[Connection]
		[Description("Publication")]
		public TestResultPublication Publication { get; set; }
	}
}
