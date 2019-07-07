using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class Choice : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the choice.")]
		public Guid Id { get; set; }

		[Description("The description of the potential answer itself.")]
		public string Text { get; set; }
	}
}
