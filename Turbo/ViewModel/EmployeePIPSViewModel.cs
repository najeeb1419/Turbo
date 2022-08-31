using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.Models;

namespace Turbo.ViewModel
{
    public class EmployeePIPSViewModel
    {
        public int employeePIPSId { get; set; }
        public int PIPS { get; set; }
        public int tradingSignalId { get; set; }
        public int employeeid { get; set; }
        public int companyId { get; set; }
        //public string apiKey { get; set; }
        //public bool Disable { get; set; }
        //public DateTime CreatedTime { get; set; }
    }
}