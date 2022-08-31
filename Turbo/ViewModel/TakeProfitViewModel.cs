using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Turbo.ViewModel
{
    public class TakeProfitViewModel
    {
        public int TakeProfitId { get; set; }
        public int TradingSignalId { get; set; }
        public string TP { get; set; } //TP( take profit )
        public string PIPS { get; set; }
        public bool Disble { get; set; }

        public TakeProfitViewModel()
        {
            TakeProfitId = 0;
            TradingSignalId = 0;
            TP = "";
            PIPS = "";
            Disble = false;
        }
    }
}