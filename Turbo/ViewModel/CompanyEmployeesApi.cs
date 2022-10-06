﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Turbo.ViewModel
{
    public class CompanyEmployeesApi
    {
        public int CompanyEmployeeID { get; set; }
        [Display(Name = "First Name")]
        public string fName { get; set; }
        [Display(Name = "Last Name")]
        public string lName { get; set; }
        [Display(Name = "Employee Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Display(Name = "Employee Image")]
        public string Image { get; set; }
        public int Companyid { get; set; }
        [Display(Name = "Employee Address")]
        public string Address { get; set; }
        [Display(Name = "Contact No")]
        public string Contact { get; set; }
        public string DateOfBirth { get; set; }
        public  string Designation { get; set; }
        public int DesignationId { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsHide { get; set; }
        public string ApiToken { get; set; }
        public string CreatedBy { get; set; }
        public string ModyfiedBy { get; set; }
        public UserDTO userDto { get; set; }

    }
}