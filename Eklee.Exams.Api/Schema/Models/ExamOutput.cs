using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	public class ExamOutput : TestResult
	{
		[Description("Candidate who took the exam.")]
		public Candidate Candidate { get; set; }

		[Description("Exam template used.")]
		public Exam ExamTemplate { get; set; }

	}
}
