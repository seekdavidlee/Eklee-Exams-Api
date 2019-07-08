using Eklee.Azure.Functions.GraphQl.Connections;
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

		[Connection]
		[Description("Tests taken")]
		public List<TestResult> TestResults { get; set; }

		[Description("Questions.")]
		public List<Question> Questions { get; set; }
	}
}
