using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Turbo.ViewModel
{
    public class AllowNotificationViewModel
    {
        public int userId { get; set; }
        public string apiToken { get; set; }
        public int employeeId { get; set; }
        public int companyId { get; set; }
        public bool status { get; set; }
    }
}