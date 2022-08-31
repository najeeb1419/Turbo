using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.ViewModel.EmployeeAPIviewModel;

namespace Turbo.ViewModel
{
    public class TradingReportViewModel
    {

        public int TradingSignalId { get; set; }
        public int CreatedById { get; set; }
        public string Image { get; set; }
        public string TradeIdea { get; set; }
        public string CurrencyName { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeId { get; set; }
        public int PIPSGain { get; set; }
        public int PIPSLose { get; set; }
        public string Badges { get; set; }
        public float wonPercentage { get; set; }
        public float lossPercentage { get; set; }
        public float wonSum { get; set; }
        public float lossSum { get; set; }
        public float tradeLossSum { get; set; }
        public float tradeWonSum { get; set; }

        public TradingReportViewModel()
        {
            TradingSignalId = 0;
            CreatedById = 0;
            Image = "";
            TradeIdea = "";
            CurrencyName = "";
            EmployeeName = "";
            EmployeeId = 0;
            PIPSGain = 0;
            PIPSLose = 0;
            Badges = "";
            wonPercentage = 0;
            lossPercentage = 0;
            wonSum = 0;
            lossSum = 0;
         
        }

    }
}