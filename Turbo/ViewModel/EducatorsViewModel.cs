using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Turbo.ViewModel
{
    public class EducatorsViewModel
    {
        public int employeeId { get; set; }
        public string employeeImage { get; set; }
        public string employeeName { get; set; }
        public int pipsWin { get; set; }
        public int pipsLoss { get; set; }
        public bool status { get; set; }
        public float wonPercentage { get; set; }
        public float lossPercentage { get; set; }
        public float wonSum { get; set; }
        public float lossSum { get; set; }
        public EducatorsViewModel()
        {
            employeeId = 0;
            employeeName = "";
            employeeImage = "";
            pipsWin = 0;
            pipsLoss = 0;
            status = true;
            wonPercentage = 0;
            lossPercentage = 0;
            wonSum =0;
            lossSum = 0;
        }
    }
}