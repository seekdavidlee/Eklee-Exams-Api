using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class Candidate : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the candidate")]
		public Guid Id { get; set; }

		[Description("The display name of the candidate.")]
		public string Name { get; set; }

		[Description("Determines whether the candidate is active.")]
		public bool Active { get; set; }

		[Description("Date/time of when the candidate account was created.")]
		public DateTime Created { get; set; }

		[Description("Type of candidate. Either student, full time employed, part time employed.")]
		public string Type { get; set; }
	}

	public class CandidateSearch : Candidate
	{

	}
}
