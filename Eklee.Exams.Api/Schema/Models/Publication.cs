using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class Publication : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the publication.")]
		public Guid Id { get; set; }

		[Description("Year")]
		public int Year { get; set; }

		[Description("Questions.")]
		public List<Question> Questions { get; set; }
	}
}
