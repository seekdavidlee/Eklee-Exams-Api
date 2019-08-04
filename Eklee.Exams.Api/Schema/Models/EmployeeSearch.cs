﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eklee.Exams.Api.Schema.Models
{
	[Description("Search result for the employee.")]
	public class EmployeeSearch
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
	}
}
