using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.ViewModel.EmployeeAPIviewModel;

namespace Turbo.ViewModel
{
    public class NotificationAPiViewModel
    {
      public ResponseAPI response { get; set; }
      public  List<NotificationViewModel> notificationList { get; set; }
    }
}