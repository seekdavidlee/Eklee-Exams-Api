using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class CandidateSearch : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the candidate")]
		public Guid Id { get; set; }

		[Description("The display name of the candidate.")]
		public string Name { get; set; }
	}
}