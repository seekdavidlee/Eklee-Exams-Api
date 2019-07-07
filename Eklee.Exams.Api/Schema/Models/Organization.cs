using Eklee.Azure.Functions.GraphQl.Connections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	[Description("Top level entity which represents the organization.")]
	public class Organization : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the organization.")]
		public Guid Id { get; set; }

		[Description("Name of the organization.")]
		public string Name { get; set; }

		[Description("Type of organization.")]
		public string Type { get; set; }

		[Description("A reference to tenant Id managed by Azure Active Directory.")]
		public string TenantId { get; set; }

		[Connection]
		public List<Candidate> Candidates { get; set; }

		[Connection]
		public List<Publication> Publications { get; set; }
	}
}
