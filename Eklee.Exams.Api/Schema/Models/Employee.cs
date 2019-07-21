using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	public class Employee : IEntityWithGuidId
	{
		[Key]
		[Description("Id of the Employee.")]
		public Guid Id { get; set; }

		[Description("Employee first name.")]
		public string FirstName { get; set; }

		[Description("Employee last name.")]
		public string LastName { get; set; }

		[Description("Employee Department.")]
		public string Department { get; set; }

		[Description("Employee email address.")]
		public string Email { get; set; }

		[Description("Determines whether the employee candidate is active.")]
		public bool Active { get; set; }

		[Description("Date/time of when the employee candidate account was created.")]
		public DateTime Created { get; set; }

		[Description("Type of employee candidate. Either student, full time employed, part time employed.")]
		public string Type { get; set; }
	}
}
