using System;

namespace Eklee.Exams.Api.Schema.Models
{
	public interface IEntityWithGuidId
	{
		Guid Id { get; set; }
	}
}
