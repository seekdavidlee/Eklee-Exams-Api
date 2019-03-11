using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	[Description("Search result for the exam.")]
	public class ExamSearch
	{
		[Key]
		[Description("Id of the exam.")]
		public Guid Id { get; set; }

		[Description("Name of the exam")]
		public string Name { get; set; }

		[Description("Category of the exam")]
		public string Category { get; set; }
	}
}