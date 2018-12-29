using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	public class ExamOutput : Exam
	{
		[Description("Candidate who took the exam.")]
		public Candidate Candidate { get; set; }

		[Description("Exam template used.")]
		public ExamTemplate ExamTemplate { get; set; }

	}
}
