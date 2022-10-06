using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Turbo.Contaxt;
using Turbo.Models;
using Turbo.ViewModel;
using Turbo.ViewModel.EmployeeAPIviewModel;

namespace Turbo.Controllers
{
    public class MobileDashboadApiController : ApiController
    {
        TurboContext db = new TurboContext();
        [HttpPost]
        public HttpResponseMessage LoginUser([FromBody] UserDTO user)
        {
            ResponseAPI res = new ResponseAPI();
            try
            {
                if (GetCompany(user.companyid))
                {
                    var findUser = db.CompanyEmployees.Where(x => x.Enable == true && x.Email == user.email && x.Password == user.password && x.Companyid == user.companyid).FirstOrDefault();
                    if (findUser != null)
                    {

                        res.Uniquekey = UpdateUserToken(findUser, "login");
                        res.Message = "Login Successfully";
                        res.UserId = findUser.CompanyEmployeeID;
                        var data = GetUserData(findUser);

                        data.ApiToken = res.Uniquekey;
                        return Request.CreateResponse(HttpStatusCode.OK, data);
                    }
                    else
                    {
                        res.Message = "User Not Found";
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                    }
                }
                else
                {
                    res.Message = "Company not found.";
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                return Request.CreateResponse(HttpStatusCode.NotImplemented, res);
            }
        }

        private CompanyEmployeesApi GetUserData(CompanyEmployee input)
        {
            CompanyEmployeesApi user = new CompanyEmployeesApi();
            if (input != null)
            {
                user.CompanyEmployeeID = input.CompanyEmployeeID;
                user.fName = input.fName;
                user.lName = input.lName;
                user.Designation = input.Designation.Name;
                user.Image = "/Images/Employee/" + input.Image;
                user.Email = input.Email;
                user.Address = input.Address;
                user.Contact = input.Contact;
                user.DateOfBirth = input.DateOfBirth;
                user.Companyid = input.Companyid;
                user.IsBlocked = input.IsBlocked;
                user.IsHide = input.IsHide;
                user.DesignationId = input.DesignationId;
            }
            return user;
        }

        private CompanyEmployee SetUserData(CompanyEmployeesApi input)
        {
            CompanyEmployee user = new CompanyEmployee();
            if (input != null)
            {
                user.CompanyEmployeeID = input.CompanyEmployeeID;
                user.fName = input.fName;
                user.lName = input.lName;
                user.DesignationId = input.DesignationId;
                user.Image = input.Image;
                user.Email = input.Email;
                user.Address = input.Address;
                user.Contact = input.Contact;
                user.DateOfBirth = input.DateOfBirth;
                user.Companyid = input.Companyid;
                user.IsBlocked = input.IsBlocked;
                user.IsHide = input.IsHide;
            }
            return user;
        }


        public HttpResponseMessage UpdateUser(CompanyEmployeesApi input)
        {
            string msg = "";
            CompanyEmployeesApi User = new CompanyEmployeesApi();
            var findUser = db.CompanyEmployees.Where(x => x.Companyid == input.userDto.companyid && x.ApiToken == input.userDto.apiToken).FirstOrDefault();
            if (findUser != null)
            {
                findUser.fName = input.fName;
                findUser.lName = input.lName;
                findUser.DesignationId = input.DesignationId;
                findUser.Image = input.Image;
                findUser.Email = input.Email;
                findUser.Address = input.Address;
                findUser.Contact = input.Contact;
                findUser.DateOfBirth = input.DateOfBirth;
                findUser.IsBlocked = input.IsBlocked;
                findUser.IsHide = input.IsHide;
                findUser.ModyfiedBy = input.ModyfiedBy;
                findUser.ModyfiedDate = DateTime.Now.AddHours(5);
                db.Entry(findUser).State = EntityState.Modified;
                db.SaveChanges();
                msg = "Updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, msg);
            }
            else
            {
                msg = "User Not Uthorized";
                return Request.CreateResponse(HttpStatusCode.Unauthorized, msg);
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> GetDesignationListAsync(UserDTO user)
        {
            if (CheckApiToken(user.companyid, user.apiToken))
            {
                var data = db.Designations.Where(x => x.companyid == user.companyid.ToString()).Select(x => new SelectItemDto()
                {
                    label = x.Name,
                    value = x.DesignationID
                }).ToListAsync();
                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "User Not Uthorized");
            }
        }


        public HttpResponseMessage LogoutUser(UserDTO user)
        {
            string msg = "";
            ResponseAPI res = new ResponseAPI();
            var findUser = db.CompanyEmployees.Where(x => x.CompanyEmployeeID == user.userId && x.Companyid == user.companyid).FirstOrDefault();
            if (findUser != null)
            {
                UpdateUserToken(findUser, "logout");
                msg = "Logout Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, msg);
            }
            else
            {
                msg = "User Not Found";
                return Request.CreateResponse(HttpStatusCode.Unauthorized, msg);
            }
        }

        private string UpdateUserToken(CompanyEmployee findUser, string type)
        {
            string token = "";
            ResponseAPI res = new ResponseAPI();
            Random random = new Random();
            if (findUser != null)
            {
                if (type == "login")
                {
                    token = RandomString(random.Next(6, 60));
                }
                findUser.ModyfiedDate = DateTime.Now.AddHours(5);
                findUser.ApiToken = token;
                db.Entry(findUser).State = EntityState.Modified;
                db.SaveChanges();
            }
            return token;
        }


        //public HttpResponseMessage UpdateProfile(CompanyEmployeesApi employee)
        //{

        //}



        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }


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
                        var IdeasList = db.TradingSignals.Include(x => x.CompanyEmployee).Include(x => x.CurrencyList).Where(x => x.Companyid == apiDTO.companyId.ToString() && x.CompanyEmployee.IsHide == false).OrderByDescending(x => x.TradingSignalId).ToList();
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
                            IdeasList = IdeasList.Where(x => x.CreatedTime.Date >= firstDayOfMonth.Date && x.CreatedTime.Date <= lastDayOfMonth.Date).ToList();
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
                            IdeasList = IdeasList.Where(x => x.CreatedTime.Date >= currentWeekStartDate.Date && x.CreatedTime.Date <= CurrentWeekLastDate.Date).ToList();
                        }

                        if (apiDTO.employeeId > 0)
                        {
                            IdeasList = IdeasList.Where(x => x.CreatedById == apiDTO.employeeId && x.Status != "0" && x.Status != "2").OrderBy(x => Convert.ToInt32(x.Status)).ToList();
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
                            tradingSignalViewModel.CreatedTime = item.CreatedTime.AddHours(1).ToString("dd/MM/yyyy HH:mm");
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
                                if (StopLoseobj.Disable == true)
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
                        trading.Response = res;
                        return Request.CreateResponse(HttpStatusCode.OK, trading);
                    }
                    else
                    {
                        res.Message = "You are not authorized user.";
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                    }
                }
                else
                {
                    res.Message = "Your company not registered.";
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, res);
                }
            }
            catch (Exception ex)
            {
                trading.TradingSignalList = null;
                res.Message = ex.Message;
                trading.Response = res;
                return Request.CreateResponse(HttpStatusCode.NotImplemented, trading);
            }
        }



        [HttpPost]
        public HttpResponseMessage CreateIdea([FromBody] TradingSignalViewModel input)
        {
            if (input != null && input.StopLose != null && input.TakeProfitList.Count > 0)
            {
                TradingSignals trad = new TradingSignals();
                trad.Buy = input.Buy;
                trad.Type = input.Type;
                trad.Status = input.Status;
                trad.Image = input.Image;
                trad.Remarks = input.Remarks;
                trad.CurrentPrice = input.CurrentPrice;
                trad.PIPS = input.PIPS;
                trad.CreatedTime = DateTime.Now.AddHours(5);
                trad.Companyid = input.Companyid;
                trad.CreatedBy = input.CreatedBy;
                trad.CreatedById = input.CreatedId;
                trad.CurrencyListId = input.CurrencyListId;
                trad.Disable = false;
                db.TradingSignals.Add(trad);

                AddTakeProfit(input.TakeProfitList, input.Companyid, input.CreatedId, trad.TradingSignalId, input.CreatedBy);
                AddStopLoss(input.StopLose, input.Companyid, input.CreatedId, trad.TradingSignalId, input.CreatedBy);
                return Request.CreateResponse(HttpStatusCode.NotImplemented, "Saved Successfully.");

            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotImplemented, "");

            }
        }



        private void AddTakeProfit(List<TakeProfitViewModel> takeProfitList, string companyId, int? createdById, int TradingSignalId, string createdByName)
        {
            foreach (var item in takeProfitList)
            {
                TakeProfit takeProfit = new TakeProfit();
                takeProfit.Companyid = companyId;
                takeProfit.PIPS = item.PIPS;
                takeProfit.TP = item.TP;
                takeProfit.CreatedBy = createdByName;
                takeProfit.TradingSignalId = TradingSignalId;
                takeProfit.CreatedTime = DateTime.Now.AddHours(5);
                takeProfit.ModifyTime = DateTime.Now.AddHours(5);
                db.TakeProfits.Add(takeProfit);
                db.SaveChanges();
            }
        }


        private void AddStopLoss(StopLoseViewModel stopLossVM, string companyId, int? createdById, int TradingSignalId, string createdByName)
        {

            StopLose stopLoss = new StopLose();
            stopLoss.Companyid = companyId;
            stopLoss.PIPS = stopLossVM.PIPS;
            stopLoss.SL = stopLossVM.SL;
            stopLoss.CreatedBy = createdByName;
            stopLoss.TradingSignalId = TradingSignalId;
            stopLoss.CreatedTime = DateTime.Now.AddHours(5);
            stopLoss.ModifyTime = DateTime.Now.AddHours(5);
            db.StopLoses.Add(stopLoss);
            db.SaveChanges();
        }


        [HttpPost]
        public HttpResponseMessage UpdateIdea([FromBody] TradingSignalViewModel input)
        {
            if (input != null && input.StopLose != null && input.TakeProfitList.Count > 0)
            {
                TradingSignals trad = new TradingSignals();
                trad.Buy = input.Buy;
                trad.Type = input.Type;
                trad.Status = input.Status;
                trad.Image = input.Image;
                trad.Remarks = input.Remarks;
                trad.CurrentPrice = input.CurrentPrice;
                trad.PIPS = input.PIPS;
                trad.CreatedTime = DateTime.Now.AddHours(5);
                trad.Companyid = input.Companyid;
                trad.CreatedBy = input.CreatedBy;
                trad.CreatedById = input.CreatedId;
                trad.CurrencyListId = input.CurrencyListId;
                trad.Disable = false;
                db.Entry(trad).State = EntityState.Modified;
                db.SaveChanges();
                UpdateTakeProfit(input.TakeProfitList, input.Companyid, input.CreatedId, trad.TradingSignalId, input.CreatedBy);
                UpdateStopLoss(input.StopLose, input.Companyid, input.CreatedId, trad.TradingSignalId, input.CreatedBy);

                return Request.CreateResponse(HttpStatusCode.OK, "Update Successfully");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotImplemented, "");

            }
        }



        private void UpdateTakeProfit(List<TakeProfitViewModel> takeProfitList, string companyId, int? createdById, int TradingSignalId, string modifyByName)
        {
            foreach (var item in takeProfitList)
            {
                TakeProfit takeProfit = new TakeProfit();
                takeProfit.Companyid = companyId;
                takeProfit.PIPS = item.PIPS;
                takeProfit.TP = item.TP;
                takeProfit.ModifyBy = modifyByName;
                takeProfit.TradingSignalId = TradingSignalId;
                takeProfit.ModifyTime = DateTime.Now.AddHours(5);
                db.Entry(takeProfit).State = EntityState.Modified;
                db.SaveChanges();
            }
        }


        private void UpdateStopLoss(StopLoseViewModel stopLossVM, string companyId, int? modifyById, int TradingSignalId, string ModifyByName)
        {
            StopLose stopLoss = new StopLose();
            stopLoss.Companyid = companyId;
            stopLoss.PIPS = stopLossVM.PIPS;
            stopLoss.SL = stopLossVM.SL;
            stopLoss.ModifyBy = ModifyByName;
            stopLoss.TradingSignalId = TradingSignalId;
            stopLoss.ModifyTime = DateTime.Now.AddHours(5);
            db.Entry(stopLoss).State = EntityState.Modified;
            db.SaveChanges();
        }



        [HttpPost]
        public HttpResponseMessage CurrencyList([FromBody] ApiDTO apiDTO)
        {
            var checkToken = CheckApiToken(apiDTO.companyId, apiDTO.apiToken);
            if (checkToken)
            {
                var list = db.Currencies.Where(x => x.Disable == false && x.Companyid == apiDTO.companyId.ToString()).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotImplemented, "Token Expired");
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> DashboardData(UserDTO  user)
        {
            DashboardVM dashboard = new DashboardVM();
            var findUser = db.CompanyEmployees.Where(x => x.Companyid == user.companyid && x.ApiToken == user.apiToken).FirstOrDefault();
            if(findUser !=null)
            {
                dashboard.Currencies = db.Currencies.Where(x => x.Companyid == user.companyid.ToString()).Count();
                dashboard.TradingIdeas = db.TradingSignals.Where(x => x.Companyid == user.companyid.ToString()).Count();
                dashboard.Employees = db.CompanyEmployees.Where(x => x.Companyid == user.companyid).Count();
                var list = await db.TradingSignals.Include(x=>x.CompanyEmployee).Where(x => x.Disable == false && x.Companyid == user.companyid.ToString())
                    .AsNoTracking().Select(x => new TradingSignalViewModel()
                    {
                        TradingSignalId = x.TradingSignalId,
                        CurrentPrice = x.CurrentPrice,
                        PIPS = x.PIPS,
                        CreatedBy = x.CompanyEmployee.fName + " " + x.CompanyEmployee.lName,
                        CurrencyName = x.CurrencyList.CurrencyName
                    }).Take(10).ToListAsync();
                dashboard.tradeList = list;
                return Request.CreateResponse(HttpStatusCode.OK, dashboard);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, dashboard);
            }
        }


        public CompanyEmployeesApi GetUserDetail(string token)
        {
            var findData = db.CompanyEmployees.Where(x => x.ApiToken == token).FirstOrDefault();
            var data = GetUserData(findData);

            return data;
        }


        private bool CheckApiToken(int companyId, string apiToken)
        {
            bool result = false;
            var finduser = db.CompanyEmployees.Where(x => x.Companyid == companyId && x.ApiToken == apiToken).FirstOrDefault();
            if (finduser != null)
            {
                result = true;
            }
            return result;
        }


        private bool GetCompany(int companyId)
        {
            var findcompany = db.RegisterComapany.Where(x => x.Enable == true && x.RegisterComapanyID == companyId).FirstOrDefault();
            if (findcompany != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
