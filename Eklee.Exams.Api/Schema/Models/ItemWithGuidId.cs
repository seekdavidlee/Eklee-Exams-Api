using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class ItemWithGuidId : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the item instance.")]
		public Guid Id { get; set; }
	}
}
