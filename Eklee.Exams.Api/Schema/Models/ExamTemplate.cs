using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class ExamTemplate : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the exam template.")]
		public Guid Id { get; set; }

		[Description("Name of the exam")]
		public string Name { get; set; }

		[Description("Category of the exam")]
		public string Category { get; set; }

		[Description("Date in which exam is released for use.")]
		public DateTime Effective { get; set; }

		[Description("Date in which exam is retired for use.")]
		public DateTime Expires { get; set; }

		[Description("Date in which exam was created.")]
		public DateTime Created { get; set; }
	}

	public class ExamTemplateSearch : ExamTemplate
	{

	}
}
