using System.Collections.Generic;
using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	[Description("Types of organization")]
	public enum OrganizationTypes
	{
		Education,
		TradeGroup,
		Business
	}

	[Description("Top level entity which represents the organization that will execute the exams on behalf of the Exam owners.")]
	public class Organization
	{
		public string Name { get; set; }

		public List<Candidate> Candidates { get; set; }

		public List<Exam> Exams { get; set; }
	}
}
