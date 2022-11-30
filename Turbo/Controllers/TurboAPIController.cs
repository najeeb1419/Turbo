using System;

using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web.Http;
//using System.Web.Mvc;
using Turbo.Contaxt;
using Turbo.Models;
using Turbo.ViewModel;
using Turbo.ViewModel.EmployeeAPIviewModel;
using System.Net.Mail;
using Turbo.Migrations;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;

namespace Turbo.Controllers
{
    public class TurboAPIController : ApiController
    {

        // GET: TurboAPI
        TurboContext db = new TurboContext();

        // list of trade ideas
        [HttpPost]
        public HttpResponseMessage TradeIdeas([FromBody] ApiDTO apiDTO)
        {
            ResponseAPI res = new ResponseAPI();
            List<TradingSignalViewModel> TradingList = new List<TradingSignalViewModel>();
            List<TakeProfitViewModel> TPList = new List<TakeProfitViewModel>();
            List<StopLoseViewModel> SLList = new List<StopLoseViewModel>();
            TradingDTO trading = new TradingDTO();
            List<TradingSignals> TradList = new List<TradingSignals>();
            DateTime date = DateTime.Now.AddHours(5);
            try
            {
                var findcompany = db.RegisterComapany.Where(x => x.Enable == true && x.RegisterComapanyID == apiDTO.companyId).FirstOrDefault();
                if (findcompany != null)
                {
                    if (CheckApiToken(apiDTO.companyId, apiDTO.apiToken))
                    {
                        var IdeasList = db.TradingSignals.Include(x => x.CompanyEmployee).Where(x => x.Companyid == apiDTO.companyId.ToString() && x.CompanyEmployee.IsHide == false).OrderByDescending(x => x.TradingSignalId).ToList();
                        if (apiDTO.reportType.ToLower() == "monthly")
                        {
                            if (apiDTO.date != "" && apiDTO.date != null)
                            {
                                date = DateTime.ParseExact(apiDTO.date, "dd/MM/yyyy", null);
                            }
                            // get first and last date of month.
                            var daysInMonth = System.DateTime.DaysInMonth(date.Year, date.Month);
                            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                            var lastDayOfMonth = new DateTime(date.Year, date.Month, daysInMonth);
                            //IdeasList = IdeasList.Where(x => x.CreatedTime.Date >= firstDayOfMonth.Date && x.CreatedTime.Date <= lastDayOfMonth.Date).ToList();
                            IdeasList = IdeasList.Where(x => x.ModifyTime.Month == lastDayOfMonth.Month).ToList();
                        }
                        else if (apiDTO.reportType.ToLower() == "weekly")
                        {
                            //get firt and last date of week
                            DayOfWeek currentDay = DateTime.Now.DayOfWeek;
                            if (apiDTO.date != "" && apiDTO.date != null)
                            {
                                DateTime week = DateTime.ParseExact(apiDTO.date, "dd/MM/yyyy", null);
                                currentDay = week.DayOfWeek;
                            }
                            int daysTillCurrentDay = currentDay - DayOfWeek.Monday;
                            DateTime currentWeekStartDate = date.AddDays(-daysTillCurrentDay);
                            DateTime CurrentWeekLastDate = currentWeekStartDate.AddDays(6);
                            IdeasList = IdeasList.Where(x => x.ModifyTime.Date >= currentWeekStartDate.Date && x.ModifyTime.Date <= CurrentWeekLastDate.Date).ToList();
                        }

                        if (apiDTO.employeeId > 0)
                        {
                            var list = IdeasList.Where(x => x.CreatedById == apiDTO.employeeId).OrderBy(x => Convert.ToInt32(x.Status)).ToList();
                            if(list.Count>0)
                            {
                                var runingIdeas = list.Where(x => x.Status == "1").ToList();
                                var wonAndLossIdeas = list.Where(x => x.Status == "5" || x.Status == "4").OrderByDescending(x => x.CreatedTime).ToList();
                                var totalIdeas =  runingIdeas;
                                if(wonAndLossIdeas.Count>0)
                                {
                                    totalIdeas.AddRange(wonAndLossIdeas);
                                    IdeasList = totalIdeas;
                                }
                            }
                            
                        }
                        else
                        {
                            IdeasList = IdeasList.Where(x => x.Disable == false && x.Status == "1").ToList();
                        }
                        foreach (var item in IdeasList)
                        {
                            TradingSignalViewModel tradingSignalViewModel = new TradingSignalViewModel();
                            tradingSignalViewModel.TradingSignalId = item.TradingSignalId;
                            tradingSignalViewModel.CurrencyNo = item.CurrencyList.CurrencyNo;
                            tradingSignalViewModel.CurrencyName = item.CurrencyList.CurrencyName;
                            string currencyname = item.CurrencyList.CurrencyName;
                            var currency = db.Currencies.Where(x => x.Disable == false && x.Companyid == apiDTO.companyId.ToString() && x.CurrencyListId == item.CurrencyListId).FirstOrDefault();
                            tradingSignalViewModel.FromImage = "/Images/Currency/" + currency.FirstCurrencyImage;
                            int companyid = Convert.ToInt32(item.Companyid);
                            tradingSignalViewModel.CreatedByImageImage = "/Images/Employee/" + item.CompanyEmployee.Image;
                            tradingSignalViewModel.ToImage = "/Images/Currency/" + currency.SecondCurrencyImage;
                            currencyname.Substring(currencyname.Length - 3);
                            tradingSignalViewModel.Buy = item.Buy;
                            if (item.Type == "1")
                            {
                                tradingSignalViewModel.Type = "Buy";
                            }
                            else if (item.Type == "2")
                            {
                                tradingSignalViewModel.Type = "Sell";
                            }
                            else if (item.Type == "3")
                            {
                                tradingSignalViewModel.Type = "Buy limit";
                            }
                            else if (item.Type == "4")
                            {
                                tradingSignalViewModel.Type = "Sell limit";
                            }
                            else if (item.Type == "5")
                            {
                                tradingSignalViewModel.Type = "Sell stop";
                            }
                            else if (item.Type == "6")
                            {
                                tradingSignalViewModel.Type = "Buy stop";
                            }
                            tradingSignalViewModel.Status = item.Status;
                            tradingSignalViewModel.Image = "/Images/Charts/" + item.Image;
                            tradingSignalViewModel.Remarks = item.Remarks;
                            tradingSignalViewModel.CurrentPrice = item.CurrentPrice;
                            tradingSignalViewModel.PIPS = item.PIPS;
                            tradingSignalViewModel.CreatedTime = item.CreatedTime.AddHours(5).ToString("dd/MM/yyyy HH:mm");
                            tradingSignalViewModel.Companyid = item.Companyid;
                            tradingSignalViewModel.CreatedBy = item.CompanyEmployee.fName + " " + item.CompanyEmployee.lName;
                            tradingSignalViewModel.CreatedId = item.CreatedById;

                            // Take Profit List 
                            var takeProfitList = db.TakeProfits.Where(x => x.TradingSignalId == item.TradingSignalId && x.Companyid == apiDTO.companyId.ToString()).ToList();

                            int i = 0;
                            foreach (var meta in takeProfitList)
                            {
                                i = i + 1;
                                TakeProfitViewModel TPobject = new TakeProfitViewModel();
                                TPobject.TakeProfitId = meta.TakeProfitId;
                                TPobject.TradingSignalId = meta.TradingSignalId;
                                TPobject.PIPS = meta.PIPS;
                                TPobject.Disble = meta.Disable;
                                TPobject.TP = meta.TP;
                                if (meta.Disable == true)
                                {
                                    tradingSignalViewModel.LatestHitTp = "TP" + i + " Achived @ " + meta.TP + " +" + meta.PIPS + " PIPS";
                                }
                                TPList.Add(TPobject);
                            }
                            if (takeProfitList.Count == 0)
                            {
                                TPList = new List<TakeProfitViewModel>();
                                tradingSignalViewModel.TakeProfitList = TPList;
                            }
                            else
                            {
                                tradingSignalViewModel.TakeProfitList = TPList;
                            }
                            TPList = new List<TakeProfitViewModel>();
                            //End take profit list
                            //start Stop Lose
                            var StopLoseobj = db.StopLoses.Where(x => x.TradingSignalId == item.TradingSignalId && x.Companyid == apiDTO.companyId.ToString()).FirstOrDefault();
                            if (StopLoseobj != null)
                            {
                                StopLoseViewModel SLobject = new StopLoseViewModel();
                                SLobject.StopLoseId = StopLoseobj.StopLoseId;
                                SLobject.TradingSignalId = StopLoseobj.TradingSignalId;
                                SLobject.PIPS = StopLoseobj.PIPS;
                                SLobject.SL = StopLoseobj.SL;
                                SLobject.Disble = StopLoseobj.Disable;
                                if (string.IsNullOrEmpty(tradingSignalViewModel.LatestHitTp) && StopLoseobj.Disable == true)
                                {
                                    tradingSignalViewModel.LatestHitTp = "SL Hit @ " + StopLoseobj.SL + " " + StopLoseobj.PIPS + " PIPS";
                                }
                                tradingSignalViewModel.StopLose = SLobject;
                            }
                            else
                            {
                                tradingSignalViewModel.StopLose = new StopLoseViewModel();
                            }
                            TradingList.Add(tradingSignalViewModel);
                        }
                        trading.TradingSignalList = TradingList;
                        res.Message = "Ok";
                        res.StatusCode = "200";
                        trading.Response = res;
                        return Request.CreateResponse(HttpStatusCode.OK, trading);
                    }
                    else
                    {
                        res.Message = "You are not authorized user.";
                        res.StatusCode = "1001";
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                    }
                }
                else
                {
                    res.Message = "Your company not registered.";
                    res.StatusCode = "1001";
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                }
            }
            catch (Exception ex)
            {
                trading.TradingSignalList = null;
                res.Message = ex.Message;
                res.StatusCode = "403";
                trading.Response = res;
                return Request.CreateResponse(HttpStatusCode.NotImplemented, trading);
            }
        }


        // get specific (single) trade idea
        [HttpPost]
        public HttpResponseMessage GetTradeIdea([FromBody] ApiDTO apiDTO)
        {
            ResponseAPI res = new ResponseAPI();
            List<TradingSignalViewModel> TradingList = new List<TradingSignalViewModel>();
            List<TakeProfitViewModel> TPList = new List<TakeProfitViewModel>();
            List<StopLoseViewModel> SLList = new List<StopLoseViewModel>();
            TradingDTO trading = new TradingDTO();
            TradingSignals TradideaObj = new TradingSignals();
            try
            {
                var findcompany = db.RegisterComapany.Where(x => x.Enable == true && x.RegisterComapanyID == apiDTO.companyId).FirstOrDefault();
                if (findcompany != null)
                {
                    if (CheckApiToken(apiDTO.companyId, apiDTO.apiToken))
                    {
                        if (apiDTO.employeeId > 0)
                        {
                            var IdeasList = db.TradingSignals.Where(x => x.Companyid == apiDTO.companyId.ToString() && x.TradingSignalId == apiDTO.tradingSignalId && x.CompanyEmployee.IsHide == false).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                            TradideaObj = IdeasList;
                        }
                        else
                        {
                            var IdeasList = db.TradingSignals.Where(x => x.Companyid == apiDTO.companyId.ToString() && x.TradingSignalId == apiDTO.tradingSignalId && x.CompanyEmployee.IsHide == false).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                            TradideaObj = IdeasList;
                        }
                        if (TradideaObj != null)
                        {
                            TradingSignalViewModel tradingSignalViewModel = new TradingSignalViewModel();
                            tradingSignalViewModel.TradingSignalId = TradideaObj.TradingSignalId;
                            tradingSignalViewModel.CurrencyNo = TradideaObj.CurrencyList.CurrencyNo;
                            var currency = db.Currencies.Where(x => x.Disable == false && x.Companyid == apiDTO.companyId.ToString() && x.CurrencyListId == TradideaObj.CurrencyListId).FirstOrDefault();
                            tradingSignalViewModel.FromImage = "/Images/Currency/" + currency.FirstCurrencyImage;
                            int companyid = Convert.ToInt32(TradideaObj.Companyid);
                            var employee = db.CompanyEmployees.Where(x => x.CompanyEmployeeID == TradideaObj.CreatedById && x.Companyid == companyid && x.IsHide == false).FirstOrDefault();
                            if (employee != null)
                            {
                                tradingSignalViewModel.CreatedByImageImage = "/Images/Employee/" + employee.Image;
                            }
                            tradingSignalViewModel.ToImage = "/Images/Currency/" + currency.SecondCurrencyImage;
                            tradingSignalViewModel.Buy = TradideaObj.Buy;
                            if (TradideaObj.Type == "1")
                            {
                                tradingSignalViewModel.Type = "Buy";
                            }
                            else if (TradideaObj.Type == "2")
                            {
                                tradingSignalViewModel.Type = "Sell";
                            }
                            else if (TradideaObj.Type == "3")
                            {
                                tradingSignalViewModel.Type = "Buy Limit";
                            }
                            else if (TradideaObj.Type == "4")
                            {
                                tradingSignalViewModel.Type = "Sell Limit";
                            }
                            else if (TradideaObj.Type == "5")
                            {
                                tradingSignalViewModel.Type = "Sell Stop";
                            }
                            else if (TradideaObj.Type == "6")
                            {
                                tradingSignalViewModel.Type = "Buy Stop";
                            }
                            tradingSignalViewModel.Status = TradideaObj.Status;
                            tradingSignalViewModel.Image = "/Images/Charts/" + TradideaObj.Image;
                            tradingSignalViewModel.Remarks = TradideaObj.Remarks;
                            tradingSignalViewModel.CurrentPrice = TradideaObj.CurrentPrice;
                            tradingSignalViewModel.PIPS = TradideaObj.PIPS;
                            tradingSignalViewModel.CreatedBy = TradideaObj.CompanyEmployee.fName + " " + TradideaObj.CompanyEmployee.lName;
                            tradingSignalViewModel.CreatedTime = TradideaObj.CreatedTime.AddHours(1).ToString("dd/MM/yyyy HH:mm");
                            tradingSignalViewModel.Companyid = TradideaObj.Companyid;
                            tradingSignalViewModel.CreatedBy = TradideaObj.CompanyEmployee.fName + " " + TradideaObj.CompanyEmployee.lName;
                            tradingSignalViewModel.CreatedId = TradideaObj.CreatedById;
                            tradingSignalViewModel.CurrencyName = TradideaObj.CurrencyList.CurrencyName;

                            // Take Profit List 
                            int i = 0;
                            var takeProfitList = db.TakeProfits.Where(x => x.TradingSignalId == TradideaObj.TradingSignalId && x.Companyid == apiDTO.companyId.ToString()).ToList();
                            foreach (var meta in takeProfitList)
                            {
                                i = i + 1;
                                TakeProfitViewModel TPobject = new TakeProfitViewModel();
                                TPobject.TakeProfitId = meta.TakeProfitId;
                                TPobject.TradingSignalId = meta.TradingSignalId;
                                TPobject.PIPS = meta.PIPS;
                                TPobject.TP = meta.TP;
                                if (meta.Disable == true)
                                {
                                    tradingSignalViewModel.LatestHitTp = "TP" + i + " Achived @ " + meta.TP + " +" + meta.PIPS + " PIPS";
                                    double pips = Convert.ToDouble(meta.PIPS);
                                    tradingSignalViewModel.PIPS = Convert.ToInt64(pips);
                                }
                                TPList.Add(TPobject);
                            }
                            if (takeProfitList.Count == 0)
                            {
                                TPList = new List<TakeProfitViewModel>();
                                tradingSignalViewModel.TakeProfitList = TPList;
                            }
                            else
                            {
                                tradingSignalViewModel.TakeProfitList = TPList;
                            }
                            //TPList = new List<TakeProfitViewModel>();
                            //End take profit list
                            //start Stop Lose
                            var StopLoseobj = db.StopLoses.Where(x => x.TradingSignalId == TradideaObj.TradingSignalId && x.Companyid == apiDTO.companyId.ToString()).FirstOrDefault();
                            if (StopLoseobj != null)
                            {
                                StopLoseViewModel SLobject = new StopLoseViewModel();
                                SLobject.StopLoseId = StopLoseobj.StopLoseId;
                                SLobject.TradingSignalId = StopLoseobj.TradingSignalId;
                                SLobject.PIPS = StopLoseobj.PIPS;
                                SLobject.SL = StopLoseobj.SL;
                                if (string.IsNullOrEmpty(tradingSignalViewModel.LatestHitTp) && StopLoseobj.Disable == true)
                                {
                                    tradingSignalViewModel.LatestHitTp = "SL Hit @ " + StopLoseobj.SL + " " + StopLoseobj.PIPS + " PIPS";
                                    double pips = Convert.ToDouble(StopLoseobj.PIPS);
                                    tradingSignalViewModel.PIPS = Convert.ToInt64(pips);
                                }
                                tradingSignalViewModel.StopLose = SLobject;
                            }
                            else
                            {
                                tradingSignalViewModel.StopLose = new StopLoseViewModel();
                            }
                            TradingList.Add(tradingSignalViewModel);
                        }
                        trading.TradingSignalList = TradingList;
                        res.Message = "Ok";
                        res.StatusCode = "200";
                        trading.Response = res;
                        return Request.CreateResponse(HttpStatusCode.OK, trading);
                    }
                    else
                    {
                        res.Message = "You are not authorized user.";
                        res.StatusCode = "1001";
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                    }
                }
                else
                {
                    res.Message = "Your company not registered.";
                    res.StatusCode = "1001";
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                }
            }
            catch (Exception ex)
            {
                trading.TradingSignalList = null;
                res.Message = ex.Message;
                res.StatusCode = "403";
                trading.Response = res;

                return Request.CreateResponse(HttpStatusCode.NotImplemented, trading);
            }
        }


        [HttpPost]
        public HttpResponseMessage LoginUser([FromBody] UserDTO user)
        {
            ResponseAPI res = new ResponseAPI();
            try
            {
                if (user != null)
                {
                    if (user.deviceToken == "" || user.apiToken == "" || user.companyid == 0 || user.userId == 0)
                    {
                        if (user.deviceToken == "" || user.deviceToken == null)
                        {
                            res.Message = "deviceToken can't be null";
                            res.StatusCode = "1001";
                        }
                        else if (user.apiToken == "" || user.apiToken == null)
                        {
                            res.Message = "apiToken can't be null";
                            res.StatusCode = "1001";
                        }
                        else if (user.companyid == 0)
                        {
                            res.Message = "companyid can't be null";
                            res.StatusCode = "1001";
                        }
                        else if (user.userId == 0)
                        {
                            res.Message = "userId can't be null";
                            res.StatusCode = "1001";
                        }
                        return Request.CreateResponse(HttpStatusCode.NotImplemented, res);

                    }
                    User user1 = new User();
                    user1.ApiToken = user.apiToken;
                    user1.DeviceToken = user.deviceToken;
                    user1.RegisterComapanyID = user.companyid;
                    user1.DeviceUserId = user.userId;
                    user1.CreatedTime = DateTime.Now.AddHours(5);
                    user1.ModifyTime = DateTime.Now.AddHours(5);
                    user1.Disable = false;
                    user1.DeviceUserId = user.userId;

                    var findcompany = db.RegisterComapany.Where(x => x.RegisterComapanyID == user.companyid && x.Enable == true).FirstOrDefault();
                    if (findcompany == null)
                    {
                        res.Message = "your company not registerd";
                        res.StatusCode = "1001";
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                    }
                    else
                    {
                        var finduser = db.Users.Where(x => x.RegisterComapanyID == user.companyid && x.Disable == false && x.DeviceUserId == user.userId).FirstOrDefault();
                        if (finduser != null)
                        {
                            finduser.ApiToken = user.apiToken;
                            finduser.DeviceToken = user.deviceToken;
                            finduser.ModifyTime = DateTime.Now.AddHours(5);
                            db.Entry(finduser).State = EntityState.Modified;
                            db.SaveChanges();
                            res.Message = "User data updated successfully";
                            res.StatusCode = "200";
                        }
                        else
                        {
                            db.Users.Add(user1);
                            db.SaveChanges();
                            res.Message = "User data added successfully";
                            res.StatusCode = "200";

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                }
                else
                {
                    res.Message = "object is null";
                    res.StatusCode = "1001";
                    return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
                }

            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.StatusCode = "403";
                return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
            }
        }


        [HttpPost]
        public HttpResponseMessage TradingReports([FromBody] ApiDTO apiDTO)
        {
            List<EmployeePIPS> employeePIPsList = new List<EmployeePIPS>();

            ReportList reportList = new ReportList();
            TradingReportObjectViewModel tradingReportObjectViewModel = new TradingReportObjectViewModel();

            ResponseAPI res = new ResponseAPI();
            List<TradingReportViewModel> TradList = new List<TradingReportViewModel>();
            List<TradingReportViewModel> WonRank = new List<TradingReportViewModel>();
            List<TradingReportViewModel> LossRank = new List<TradingReportViewModel>();
            TradingReportViewModel tradingReportVM = new TradingReportViewModel();
            DateTime date = DateTime.Now.AddHours(5);
            try
            {
                var findcompany = db.RegisterComapany.Where(x => x.Enable == true && x.RegisterComapanyID == apiDTO.companyId).FirstOrDefault();
                if (findcompany != null)
                {
                    if (CheckApiToken(apiDTO.companyId, apiDTO.apiToken))
                    {
                        var emplist = db.CompanyEmployees.Where(x => x.Companyid == apiDTO.companyId && x.Enable == true && x.IsHide == false).ToList();
                        foreach (var item1 in emplist)
                        {
                            tradingReportVM = new TradingReportViewModel();
                            var query = db.EmployeePIPs.Include(x => x.CompanyEmployee).Where(x => x.CompanyId == apiDTO.companyId.ToString() && x.CompanyEmployeeID == item1.CompanyEmployeeID).ToList();
                            if (apiDTO.reportType.ToLower() == "monthly")
                            {

                                if (apiDTO.date != "" && apiDTO.date != null)
                                {
                                    date = DateTime.ParseExact(apiDTO.date, "dd/MM/yyyy", null);
                                }
                                // get first and last date of month.
                                //var daysInMonth = System.DateTime.DaysInMonth(date.Year, date.Month);
                                //var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                                //var lastDayOfMonth = new DateTime(date.Year, date.Month, daysInMonth);
                                //query = query.Where(x => x.CreatedTime.Date >= firstDayOfMonth.Date && x.CreatedTime.Date <= lastDayOfMonth.Date).ToList();
                                query = query.Where(x => DateTime.ParseExact(x.CreatedTime.Date.ToString("dd/MM/yyyy"), "dd/MM/yyyy", null).Month == date.Month).ToList();


                            }
                            else if (apiDTO.reportType.ToLower() == "weekly")
                            {
                                //get firt and last date of week
                                DayOfWeek currentDay = DateTime.Now.DayOfWeek;
                                if (apiDTO.date != "" && apiDTO.date != null)
                                {
                                    DateTime week = DateTime.ParseExact(apiDTO.date, "dd/MM/yyyy", null);
                                    currentDay = week.DayOfWeek;
                                }
                                int daysTillCurrentDay = currentDay - DayOfWeek.Monday;
                                DateTime currentWeekStartDate = date.AddDays(-daysTillCurrentDay);
                                DateTime CurrentWeekLastDate = currentWeekStartDate.AddDays(6);
                                query = query.Where(x => x.CreatedTime.Date >= currentWeekStartDate.Date && x.CreatedTime.Date <= CurrentWeekLastDate.Date).ToList();
                            }
                            //
                            var calculate = CalculatePIPS(query);
                            tradingReportVM.PIPSGain = calculate.PIPSWonCount;
                            tradingReportVM.PIPSLose = calculate.PIPSLossCount;
                            //
                            tradingReportVM.CreatedById = item1.CompanyEmployeeID;
                            tradingReportVM.EmployeeId = item1.CompanyEmployeeID;
                            tradingReportVM.Image = "/Images/Employee/" + item1.Image;
                            tradingReportVM.EmployeeName = item1.fName + " " + item1.lName;
                            tradingReportVM.wonSum = calculate.PIPSWonSum;
                            tradingReportVM.lossSum = Math.Abs(calculate.PIPSLossSum);
                            calculate.PIPSLossSum = Math.Abs(calculate.PIPSLossSum);
                            float wonPercentage = 0;
                            float lossPercentage = 0;
                            if (calculate.PIPSWonSum > 0 || calculate.PIPSLossSum > 0)
                            {
                                float total = calculate.PIPSWonSum + calculate.PIPSLossSum;
                                wonPercentage = (calculate.PIPSWonSum / total) * 100;
                                lossPercentage = (calculate.PIPSLossSum / total) * 100;
                                tradingReportVM.wonPercentage = wonPercentage;
                                tradingReportVM.lossPercentage = lossPercentage;
                            }
                            if (tradingReportVM.EmployeeId > 0)
                            {
                                tradingReportVM.EmployeeId = tradingReportVM.EmployeeId;
                            }
                            if (tradingReportVM.PIPSLose > 0 || tradingReportVM.PIPSGain > 0)
                            {
                                TradList.Add(tradingReportVM);
                            }
                        }
                        // to manage top pips stars apply order by wonrank and lossrank
                        if (TradList.Count > 0)
                        {
                            WonRank = TradList.Where(x => x.wonSum > 0).OrderByDescending(x => x.wonSum).ToList();
                            LossRank = TradList.Where(x => x.lossSum > 0 && x.wonSum == 0).OrderBy(x => (Math.Abs(x.lossSum))).ToList();
                            if (WonRank.Count > 0)
                            {
                                TradList = WonRank;
                                if (LossRank.Count > 0)
                                {
                                    TradList.AddRange(LossRank);
                                }
                            }
                            else if (LossRank.Count > 0)
                            {
                                TradList = LossRank;
                            }
                        }
                        res.Message = "Ok";
                        res.StatusCode = "200";
                        tradingReportObjectViewModel.response = res;
                        tradingReportObjectViewModel.tradingReportList = TradList;
                        reportList.tradingReport = tradingReportObjectViewModel;
                        return Request.CreateResponse(HttpStatusCode.OK, reportList);
                    }
                    else
                    {
                        res.Message = "Your are not authorized.";
                        res.StatusCode = "1001";
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                    }
                }
                else
                {
                    res.Message = "Your company not registerd.";
                    res.StatusCode = "1001";
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.StatusCode = "403";
                return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
            }

        }

        public bool CheckApiToken(int companyId, string apiToken)
        {
            bool result = false;

            var finduser = db.Users.Where(x => x.RegisterComapanyID == companyId && x.ApiToken == apiToken).FirstOrDefault();
            if (finduser != null)
            {
                result = true;
            }
            return result;
        }

        [HttpPost]
        public HttpResponseMessage GetNotifications([FromBody] ApiDTO apiDTO)
        {
            ResponseAPI res = new ResponseAPI();
            NotificationAPiViewModel notificationAPi = new NotificationAPiViewModel();
            List<NotificationViewModel> notificationVMlist = new List<NotificationViewModel>();
            try
            {
                var findcompany = db.RegisterComapany.Where(x => x.Enable == true && x.RegisterComapanyID == apiDTO.companyId).FirstOrDefault();
                if (findcompany != null)
                {
                    if (CheckApiToken(apiDTO.companyId, apiDTO.apiToken))
                    {
                        var findNotifications = db.Notifications.Include(x => x.CompanyEmployee).Where(x => x.RegisterComapanyID == apiDTO.companyId && x.Type != "custome" && x.CompanyEmployee.IsHide == false).OrderByDescending(x => x.NotificationId).ToList();
                        if (findNotifications.Count > 0)
                        {
                            foreach (var item in findNotifications)
                            {
                                NotificationViewModel notificationVM = new NotificationViewModel();
                                notificationVM.title = item.Title;
                                notificationVM.body = item.Body;
                                notificationVM.createdTime = item.CreatedTime.AddHours(1).ToString("dd/MM/yyyy HH:mm");
                                notificationVM.companyId = item.RegisterComapanyID;
                                notificationVM.currencyName = item.CurrencyName;
                                notificationVM.TradingSignalId = item.TradingSignalId;
                                notificationVM.employeeName = item.CompanyEmployee.fName + " " + item.CompanyEmployee.lName;
                                notificationVM.price = item.Price;
                                if (item.Type == "1")
                                {
                                    notificationVM.type = "Buy";
                                }
                                else if (item.Type == "2")
                                {
                                    notificationVM.type = "Sell";
                                }
                                else if (item.Type == "3")
                                {
                                    notificationVM.type = "Buy limit";
                                }
                                else if (item.Type == "4")
                                {
                                    notificationVM.type = "Sell limit";
                                }
                                else if (item.Type == "5")
                                {
                                    notificationVM.type = "Sell stop";
                                }
                                else if (item.Type == "6")
                                {
                                    notificationVM.type = "Buy stop";
                                }
                                notificationVM.price = item.Price;
                                if (item.Status == "0")
                                {
                                    notificationVM.status = "Withdrawn";
                                }
                                else if (item.Status == "1")
                                {
                                    notificationVM.status = "Sent by";
                                }
                                else if (item.Status == "2")
                                {
                                    notificationVM.status = "Rejected";
                                }
                                else if (item.Status == "4")
                                {
                                    notificationVM.status = "Loss";
                                }
                                else if (item.Status == "5")
                                {
                                    notificationVM.status = "Won";
                                }
                                notificationVMlist.Add(notificationVM);
                            }
                        }
                        notificationAPi.notificationList = notificationVMlist;
                        res.Message = "Ok";
                        res.StatusCode = "200";
                        notificationAPi.response = res;
                        return Request.CreateResponse(HttpStatusCode.OK, notificationAPi);
                    }
                    else
                    {
                        res.Message = "You are not authorized user.";
                        res.StatusCode = "1001";
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                    }
                }
                else
                {
                    res.Message = "Your company not registered.";
                    res.StatusCode = "1001";
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.StatusCode = "403";
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, res);
            }

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Educators([FromBody] UserDTO user)
        {
            ResponseAPI res = new ResponseAPI();
            //creating these objects on the demand of andriod developer 
            EducatorsListViewModel eduobj = new EducatorsListViewModel();

            List<EducatorsViewModel> edulist = new List<EducatorsViewModel>();
            EducatorsListViewModel educatorobj = new EducatorsListViewModel();

            try
            {
                var findcompany = db.RegisterComapany.Where(x => x.RegisterComapanyID == user.companyid).FirstOrDefault();
                if (findcompany != null)
                {
                    if (CheckApiToken(user.companyid, user.apiToken))
                    {

                        var employees = await db.CompanyEmployees.Where(x => x.Companyid == user.companyid && x.IsHide == false).ToListAsync();
                        //var user = db.Users.Where(x=>x.Disable==false && x.DeviceUserId==)
                        var disallowNotifiation = db.AllowNotifications.Where(x => x.RegisterComapanyID == user.companyid && x.DeviceUserId == user.userId).ToList();
                        if (employees.Count > 0)
                        {
                            for (int i = 0; i < employees.Count(); i++)
                            {
                                EducatorsViewModel edu = new EducatorsViewModel();
                                for (int j = 0; j < disallowNotifiation.Count(); j++)
                                {
                                    if (employees[i].CompanyEmployeeID == disallowNotifiation[j].CompanyEmployeeID)
                                    {
                                        edu.status = false;
                                    }
                                }
                                edu.employeeName = employees[i].fName + " " + employees[i].lName;
                                edu.employeeId = employees[i].CompanyEmployeeID;
                                edu.employeeImage = "/Images/Employee/" + employees[i].Image;
                                List<EmployeePIPS> pipsList = new List<EmployeePIPS>();
                                pipsList = await db.EmployeePIPs.Include(x => x.CompanyEmployee).Where(x => x.CompanyId == user.companyid.ToString() && x.CompanyEmployeeID == edu.employeeId && x.CompanyEmployee.IsHide == false).ToListAsync();
                                var calculate = CalculatePIPS(pipsList);
                                //edu.pipsWin = pipsList.Where(x => x.Status.ToLower() == "won").Count();
                                //edu.pipsLoss = pipsList.Where(x => x.Status.ToLower() == "loss").Count();
                                //edu.wonSum = pipsList.Where(x => x.Status.ToLower() == "won").Sum(x => x.PIPS);
                                //edu.lossSum = pipsList.Where(x => x.Status.ToLower() == "loss").Sum(x => x.PIPS);
                                edu.pipsWin = calculate.PIPSWonCount;
                                edu.pipsLoss = calculate.PIPSLossCount;
                                edu.wonSum = calculate.PIPSWonSum;
                                edu.lossSum = calculate.PIPSLossSum;
                                edu.wonPercentage = (edu.wonSum / edu.wonSum + edu.lossSum) * 100;
                                edu.lossPercentage = (100 - edu.wonPercentage);
                                edulist.Add(edu);
                            }
                        }
                        eduobj.EducatorList = edulist;
                        return Request.CreateResponse(HttpStatusCode.OK, eduobj);
                    }
                    else
                    {
                        res.Message = "You are not authorized user.";
                        res.StatusCode = "1001";
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                    }
                }
                else
                {
                    res.Message = "Your company not registered.";
                    res.StatusCode = "1001";
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.StatusCode = "403";
            }
            return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
        }
        [HttpPost]
        public HttpResponseMessage NotificationStatus([FromBody] AllowNotificationViewModel notif)
        {
            ResponseAPI res = new ResponseAPI();
            try
            {
                if (CheckApiToken(notif.companyId, notif.apiToken))
                {
                    if (notif != null)
                    {
                        var findcompany = db.RegisterComapany.Where(x => x.RegisterComapanyID == notif.companyId && x.Enable == true).FirstOrDefault();
                        if (findcompany != null)
                        {
                            if (notif.companyId == 0 || notif.employeeId == 0 || notif.userId == 0 || notif.apiToken == "" || notif.apiToken == null)
                            {
                                if (notif.companyId == 0)
                                {
                                    res.Message = "CompanyId can't be null";
                                    res.StatusCode = "1001";
                                    return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
                                }
                                else if (notif.employeeId == 0)
                                {
                                    res.Message = "employeeId can't be null";
                                    res.StatusCode = "1001";
                                    return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
                                }
                                else if (notif.apiToken == "" || notif.apiToken == null)
                                {
                                    res.Message = "apiToken can't be null";
                                    res.StatusCode = "1001";
                                    return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
                                }
                                else if (notif.userId == 0)
                                {
                                    res.Message = "userId can't be null";
                                    res.StatusCode = "1001";
                                    return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
                                }
                            }
                            AllowNotification notification = new AllowNotification();
                            var finduser = db.AllowNotifications.Where(x => x.DeviceUserId == notif.userId && x.CompanyEmployeeID == notif.employeeId).FirstOrDefault();
                            if (notif.status == true)
                            {
                                if (finduser != null)
                                {
                                    db.AllowNotifications.Remove(finduser);
                                    db.SaveChanges();
                                    res.Message = "Notificatons allow successfully";
                                    res.StatusCode = "200";
                                    return Request.CreateResponse(HttpStatusCode.OK, res);
                                }
                                res.Message = "";
                                res.StatusCode = "200";
                                return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
                            }
                            else
                            {
                                if (finduser == null)
                                {
                                    notification.DeviceUserId = notif.userId;
                                    notification.CompanyEmployeeID = notif.employeeId;
                                    notification.RegisterComapanyID = notif.companyId;
                                    notification.CreatedTime = DateTime.Now.AddHours(5);
                                    notif.status = false;
                                    db.AllowNotifications.Add(notification);
                                    db.SaveChanges();
                                    res.Message = "Notifications off successfully";
                                    res.StatusCode = "200";
                                    return Request.CreateResponse(HttpStatusCode.OK, res);
                                }
                                res.Message = "Notifications already off";
                                res.StatusCode = "200";
                                return Request.CreateResponse(HttpStatusCode.OK, res);
                            }
                        }
                        else
                        {
                            res.Message = "Your company not registerd";
                            res.StatusCode = "1001";
                            return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                        }
                    }
                    else
                    {
                        res.Message = "object is null";
                        res.StatusCode = "1001";
                        return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
                    }
                }
                else
                {
                    res.Message = "You are not authorized user.";
                    res.StatusCode = "1001";
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.StatusCode = "403";
                return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
            }
        }

        [HttpPost]
        public HttpResponseMessage PastResult([FromBody] ApiDTO apiDTO)
        {
            ResponseAPI res = new ResponseAPI();
            PastResultList pastobj = new PastResultList();

            List<PastResultViewModel> pastlist = new List<PastResultViewModel>();
            try
            {
                var findcompany = db.RegisterComapany.Where(x => x.Enable == true && x.RegisterComapanyID == apiDTO.companyId).FirstOrDefault();
                if (findcompany != null)
                {
                    if (CheckApiToken(apiDTO.companyId, apiDTO.apiToken))
                    {
                        var trade = db.TradingSignals.Include(x => x.CompanyEmployee).Where(x => x.Status == "4" || x.Status == "5" && x.CompanyEmployee.IsHide == false).OrderByDescending(x => x.ModifyTime).ToList();
                        foreach (var item in trade)
                        {
                            PastResultViewModel past = new PastResultViewModel();

                            past.CurrencyName = item.CurrencyList.CurrencyName;
                            past.TradingSignalId = item.TradingSignalId;
                            if (item.Type == "1")
                            {
                                past.Type = "Buy";
                            }
                            else if (item.Type == "2")
                            {
                                past.Type = "Sell";
                            }
                            else if (item.Type == "3")
                            {
                                past.Type = "Buy limit";
                            }
                            else if (item.Type == "4")
                            {
                                past.Type = "Sell limit";
                            }
                            else if (item.Type == "5")
                            {
                                past.Type = "Sell stop";
                            }
                            else if (item.Type == "6")
                            {
                                past.Type = "Buy stop";
                            }
                            //past.Type = item.TradingSignals.Type;
                            past.CreatedTime = item.ModifyTime.ToString("dd/MM/yyyy hh:mm tt");
                            if (item.Status == "5")
                            {
                                past.Badges = "WON";
                            }
                            if (item.Status == "4")
                            {
                                past.Badges = "SL HIT";
                            }
                            pastlist.Add(past);
                        }
                        pastobj.pastResultList = pastlist;
                        return Request.CreateResponse(HttpStatusCode.OK, pastobj);
                    }
                    else
                    {
                        res.Message = "You are not authorized user.";
                        res.StatusCode = "1001";
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                    }
                }
                else
                {
                    res.Message = "Your company not registered.";
                    res.StatusCode = "1001";
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.StatusCode = "403";
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, res);
            }
        }

        public PIPSCalculation CalculatePIPS(List<EmployeePIPS> input)
        {
            PIPSCalculation pipsCalculation = new PIPSCalculation();
            var total = input.Where(x => x.Status.ToLower() == "won").Count();
            pipsCalculation.PIPSWonCount = total;
            pipsCalculation.PIPSLossCount = input.Where(x => x.Status.ToLower() == "loss").Count();
            pipsCalculation.PIPSWonSum = input.Where(x => x.Status.ToLower() == "won").Sum(x => x.PIPS);
            pipsCalculation.PIPSLossSum = input.Where(x => x.Status.ToLower() == "loss").Sum(x => x.PIPS);
            return pipsCalculation;
        }
    }
}





