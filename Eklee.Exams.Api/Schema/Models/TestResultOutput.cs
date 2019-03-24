using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	public class TestResultOutput : TestResult
	{
		[Description("Candidate who took the exam.")]
		public Candidate Candidate { get; set; }

		[Description("Exam used.")]
		public Exam Exam { get; set; }

	}
}
