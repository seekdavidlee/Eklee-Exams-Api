using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class Question : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the question.")]
		public Guid Id { get; set; }

		[Description("The question itself.")]
		public string Text { get; set; }

		[Description("Choice.")]
		public List<Choice> Choices { get; set; }

		[Description("Answer.")]
		public Guid Answer { get; set; }
	}
}
