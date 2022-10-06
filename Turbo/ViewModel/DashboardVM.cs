using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Turbo.ViewModel
{
    public class DashboardVM
    {
        public decimal Employees { get; set; }
        public decimal TradingIdeas { get; set; }
        public decimal Currencies { get; set; }
        public List<int> mountCount { get; set; }
        public List<int> dailyCount { get; set; }
        public List<TradingSignalViewModel> tradeList { get; set; }
    }
}