using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class Exam : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the exam.")]
		public Guid Id { get; set; }

		[Description("Name of the exam")]
		public string Name { get; set; }

		[Description("Category of the exam")]
		public string Category { get; set; }

		[Description("Date in which exam was created.")]
		public DateTime Created { get; set; }

		[Connection]
		[Description("Tests taken")]
		public List<TestResult> TestResults { get; set; }

		[Description("Questions.")]
		public List<Question> Questions { get; set; }
	}
}
