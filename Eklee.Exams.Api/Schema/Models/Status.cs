using System.ComponentModel;

namespace Eklee.Exams.Api.Schema.Models
{
	public class Status
	{
		[Description("Message describing the status of the request.")]
		public string Message { get; set; }
	}
}
