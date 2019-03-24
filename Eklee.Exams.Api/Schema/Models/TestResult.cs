using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class TestResult : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the exam.")]
		public Guid Id { get; set; }

		[Description("Name of the exam")]
		public string Name { get; set; }

		[Description("Category of the exam")]
		public string Category { get; set; }

		[Description("Id of the candidate.")]
		public Guid CandidateId { get; set; }

		[Description("Id of the exam template used.")]
		public Guid ExamId { get; set; }

		[Description("Date/time of when the exm was taken.")]
		public DateTime Taken { get; set; }
	}
}
