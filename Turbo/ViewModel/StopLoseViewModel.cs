﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Turbo.ViewModel
{
    public class StopLoseViewModel
    {
        public int StopLoseId { get; set; }
        public int TradingSignalId { get; set; }
        public string SL { get; set; } //SL( Stop Lose )
        public string PIPS { get; set; }
        public bool Disble { get; set; }

        public StopLoseViewModel()
        {
            StopLoseId = 0;
            TradingSignalId = 0;
            SL = "";
            PIPS = "";
            Disble = false;
        }
    }

}