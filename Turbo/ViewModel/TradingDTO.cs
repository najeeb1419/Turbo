using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.ViewModel.EmployeeAPIviewModel;

namespace Turbo.ViewModel
{
    public class TradingDTO
    {
        public ResponseAPI Response { get; set; }
        public List<TradingSignalViewModel> TradingSignalList { get; set; }
    
    }
}