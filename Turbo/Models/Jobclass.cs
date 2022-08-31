using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using Turbo.Contaxt;
using Turbo.ViewModel;

namespace Turbo.Models
{
    public class Jobclass:IJob
    {
        public static string SERVERAPIKEY = WebConfigurationManager.AppSettings["SERVER_API_KEY"];
        public static string SENDERID = WebConfigurationManager.AppSettings["SENDER_ID"];
        TurboContext db = new TurboContext();

        Task IJob.Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                BackgroundService();
            });
        }
        public  bool BackgroundService()
        {
            var distinctCurrency = db.TradingSignals.Include(x => x.CompanyEmployee).Where(x => x.Status == "1" && x.CompanyEmployee.IsHide == false).ToList();
            foreach (var item in distinctCurrency)
            {
                string currenyPair = item.CurrencyList.CurrencyName.Replace(@"/", string.Empty);
                string Baseurl = "https://api.finage.co.uk/last/forex/";
                decimal currentRate = 0;
                string body = "";
                string currencyname = item.CurrencyList.CurrencyName;
                JavaScriptSerializer jss = new JavaScriptSerializer();
                currencyname = currencyname.Substring(currencyname.Length - 3);
                using (var client = new HttpClient())
                {
                    //Passing service base url
                    client.BaseAddress = new Uri(Baseurl);
                    client.DefaultRequestHeaders.Clear();
                    //Define request data format
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //Sending request to find web api REST service resource GetAllEmployees using HttpClient
                    var Res = client.GetAsync(currenyPair + "?apikey=API_KEY38T8YULAK4WQQ163ACWCHL6BSND4T4TT");
                    Res.Wait();
                    var result = Res.Result;
                    //Checking the response is successful or not which is sent using HttpClient
                    if (result.IsSuccessStatusCode)
                    {
                        //Storing the response details recieved from web api
                        var EmpResponse = result.Content.ReadAsStringAsync().Result;
                        var obj = JsonConvert.DeserializeObject(EmpResponse);
                        currencyJson user = jss.Deserialize<currencyJson>(obj.ToString());
                        var tradeideas = db.TradingSignals.Include(x => x.CompanyEmployee).Where(x => x.CurrencyListId == item.CurrencyListId && x.Disable == false && x.Status == "1" && x.CompanyEmployee.IsHide == false).ToList();

                        if (currencyname == "JPY")
                        {
                            foreach (var trad in tradeideas)
                            {
                                int i = 0;
                                decimal countPIPS = 0;
                                var takeProfit = db.TakeProfits.Where(x => x.Disable == false && x.TradingSignalId == trad.TradingSignalId && x.Companyid == trad.Companyid).ToList();
                                foreach (var tpItem in takeProfit)
                                {
                                    i = i + 1;
                                    currentRate = Convert.ToDecimal(Convert.ToDecimal(user.ask).ToString("F2"));
                                    if (trad.Type == "1" || trad.Type == "3" || trad.Type == "6")
                                    {
                                        countPIPS = (Convert.ToDecimal(trad.Buy) - Convert.ToDecimal(currentRate)) * 100;
                                        countPIPS = Convert.ToDecimal(trad.Buy) > Convert.ToDecimal(currentRate) ? System.Math.Abs(countPIPS) * (-1) : System.Math.Abs(countPIPS);
                                    }
                                    if (trad.Type == "2" || trad.Type == "4" || trad.Type == "5")
                                    {
                                        countPIPS = (Convert.ToDecimal(trad.Buy) - Convert.ToDecimal(currentRate)) * 100;
                                        countPIPS = Convert.ToDecimal(currentRate) > Convert.ToDecimal(trad.Buy) ? System.Math.Abs(countPIPS) * (-1) : System.Math.Abs(countPIPS);
                                    }
                                    if (countPIPS >= Convert.ToDecimal(tpItem.PIPS))
                                    {
                                        //Take Profit 3 Achieved +100 PIPS - EUR/JPY
                                        body = "Take Profit " + tpItem.No + " Achieved +" + tpItem.PIPS + " PIPS - " + trad.CurrencyList.CurrencyName;

                                        // add in past result
                                        EmployeePIPS employeePIPS = new EmployeePIPS();
                                        employeePIPS.TradingSignalId = trad.TradingSignalId;
                                        employeePIPS.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);
                                        employeePIPS.CompanyId = trad.Companyid.ToString();
                                        employeePIPS.CreatedTime = DateTime.Now;
                                        employeePIPS.ModifyTime = DateTime.Now;
                                        // disable is true because this will not show in pass result , these records will only show in past result when the idea is completely won or loss
                                        employeePIPS.Disable = true;
                                        double pipsResult = Convert.ToDouble(tpItem.PIPS);
                                        employeePIPS.PIPS = Convert.ToInt32(pipsResult);
                                        db.EmployeePIPs.Add(employeePIPS);
                                        db.SaveChanges();
                                        // change tP status
                                        tpItem.Disable = true;
                                        tpItem.ModifyTime = DateTime.Now;
                                        db.Entry(tpItem).State = EntityState.Modified;
                                        db.SaveChanges();

                                        Notification notification = new Notification();
                                        notification.RegisterComapanyID = Convert.ToInt32(trad.Companyid);
                                        notification.Title = "Trade Idea";
                                        notification.CreatedTime = DateTime.Now;
                                        notification.CurrencyName = trad.CurrencyList.CurrencyName;
                                        notification.EmployeeName = trad.CompanyEmployee.fName + " " + trad.CompanyEmployee.lName;
                                        notification.Type = trad.Type;
                                        notification.Body = body;
                                        notification.TradingSignalId = trad.TradingSignalId.ToString();
                                        notification.CreatedById = Convert.ToInt32(trad.CreatedById);
                                        notification.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);
                                        notification.Status = "5";
                                        db.Notifications.Add(notification);
                                        db.SaveChanges();
                                        SendNotification(Convert.ToInt32(trad.CreatedById), notification.Body, trad.TradingSignalId);

                                    }
                                }

                                // when all tp are hitted then end the trade idea
                                var findTakePrfit = db.TakeProfits.Where(x => x.Disable == false && x.TradingSignalId == trad.TradingSignalId && x.Companyid == trad.Companyid).ToList();
                                if (findTakePrfit.Count() == 0)
                                {
                                    EmployeePIPS employeePIPS = new EmployeePIPS();
                                    employeePIPS.TradingSignalId = trad.TradingSignalId;
                                    employeePIPS.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);
                                    employeePIPS.CompanyId = trad.Companyid.ToString();
                                    employeePIPS.CreatedTime = DateTime.Now;
                                    employeePIPS.ModifyTime = DateTime.Now;
                                    employeePIPS.Disable = false;
                                    employeePIPS.Status = "Won";
                                    var tpend = db.TakeProfits.Where(x => x.TradingSignalId == trad.TradingSignalId).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                                    double pipsResult = Convert.ToDouble(tpend.PIPS);
                                    // if employeePips greater than 0 than won
                                    employeePIPS.Status = "Won";
                                    employeePIPS.PIPS =Convert.ToInt32(pipsResult);
                                    db.EmployeePIPs.Add(employeePIPS);
                                    db.SaveChanges();
                                    body = "Trade Ended & Idea Won - " + trad.CurrencyList.CurrencyName;

                                    trad.Disable = true;
                                    trad.Status = "5";
                                    db.Entry(trad).State = EntityState.Modified;
                                    db.SaveChanges();
                                    // Save notification
                                    Notification notification = new Notification();
                                    notification.RegisterComapanyID = Convert.ToInt32(trad.Companyid);
                                    notification.Title = "Trade Idea";
                                    notification.CreatedTime = DateTime.Now;
                                    notification.CurrencyName = trad.CurrencyList.CurrencyName;
                                    notification.EmployeeName = trad.CompanyEmployee.fName + " " + trad.CompanyEmployee.lName;
                                    notification.Type = trad.Type;
                                    notification.Body = body;
                                    notification.TradingSignalId = trad.TradingSignalId.ToString();
                                    notification.CreatedById = Convert.ToInt32(trad.CreatedById);
                                    notification.Status = trad.Status;
                                    notification.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);
                                    db.Notifications.Add(notification);
                                    db.SaveChanges();
                                    // send notification
                                    SendNotification(Convert.ToInt32(trad.CreatedById), notification.Body, trad.TradingSignalId);
                                    break;

                                }
                                var loss = db.StopLoses.Where(x => x.Disable == false && x.TradingSignalId == trad.TradingSignalId).FirstOrDefault();
                                if (loss != null)
                                {
                                    currentRate = Convert.ToDecimal(Convert.ToDecimal(user.ask).ToString("F2"));
                                    double pipsResult = Convert.ToDouble(loss.PIPS);
                                    if (trad.Type == "1" || trad.Type == "3" || trad.Type == "6")
                                    {
                                        countPIPS = (Convert.ToDecimal(trad.Buy) - Convert.ToDecimal(currentRate)) * 100;
                                        countPIPS = Convert.ToDecimal(trad.Buy) > Convert.ToDecimal(currentRate) ? System.Math.Abs(countPIPS) * (-1) : System.Math.Abs(countPIPS);
                                    }
                                    if (trad.Type == "2" || trad.Type == "4" || trad.Type == "5")
                                    {
                                        countPIPS = (Convert.ToDecimal(trad.Buy) - Convert.ToDecimal(currentRate)) * 100;
                                        countPIPS = Convert.ToDecimal(currentRate) > Convert.ToDecimal(trad.Buy) ? System.Math.Abs(countPIPS) * (-1) : System.Math.Abs(countPIPS);
                                    }
                                    if (countPIPS <= Convert.ToDecimal(pipsResult))
                                    {
                                        //SL HIT unfortunately - 30 PIPS - EUR / JPY

                                        // Save result
                                        EmployeePIPS employeePIPS = new EmployeePIPS();
                                        employeePIPS.TradingSignalId = trad.TradingSignalId;
                                        employeePIPS.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);
                                        employeePIPS.CompanyId = trad.Companyid.ToString();
                                        employeePIPS.CreatedTime = DateTime.Now;
                                        employeePIPS.ModifyTime = DateTime.Now;
                                        employeePIPS.Disable = false;
                                        // if employeePips less than 0 than Loss
                                        //if (pipsResult > 0)
                                        //{
                                        //    employeePIPS.PIPS = -(Convert.ToInt32(pipsResult));
                                        //}
                                        //else if (pipsResult == 0)
                                        //{
                                        //    employeePIPS.PIPS = -1;
                                        //}
                                        //else
                                        //{
                                        //    employeePIPS.PIPS = Convert.ToInt32(pipsResult);
                                        //}
                                        //db.EmployeePIPs.Add(employeePIPS);
                                        //db.SaveChanges();
                                        //change tP status
                                        loss.Disable = true;
                                        loss.ModifyTime = DateTime.Now;
                                        db.Entry(loss).State = EntityState.Modified;
                                        db.SaveChanges();
                                        // trade idea end 
                                        trad.Disable = true;
                                        // if take profit is null or count =0 then idea shuld be loss else idea shuld be won
                                        var takeprofit = db.TakeProfits.Where(x => x.Disable == true && x.TradingSignalId == trad.TradingSignalId && x.Companyid == trad.Companyid).ToList();
                                        if(takeProfit.Count>0)
                                        {
                                            trad.Status = "5";
                                            body = "Trade Ended & Idea Won - " + trad.CurrencyList.CurrencyName;
                                            var tpHit = takeProfit.OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                                            employeePIPS.Status = "Won";
                                            employeePIPS.PIPS = Convert.ToInt32(tpHit.PIPS);
                                            db.EmployeePIPs.Add(employeePIPS);
                                            db.SaveChanges();
                                        }
                                       
                                        else
                                        {
                                            trad.Status = "4";
                                            body = "SL HIT unfortunately " + pipsResult + " PIPS - " + trad.CurrencyList.CurrencyName;
                                            employeePIPS.Status = "Loss";
                                            employeePIPS.PIPS = Convert.ToInt32(pipsResult);
                                            db.EmployeePIPs.Add(employeePIPS);
                                            db.SaveChanges();
                                        }
                                        db.Entry(trad).State = EntityState.Modified;
                                        db.SaveChanges();

                                        Notification notification = new Notification();
                                        notification.RegisterComapanyID = Convert.ToInt32(trad.Companyid);
                                        notification.Title = "Trade Idea";
                                        notification.CreatedTime = DateTime.Now;
                                        notification.CurrencyName = trad.CurrencyList.CurrencyName;
                                        notification.EmployeeName = trad.CompanyEmployee.fName + " " + trad.CompanyEmployee.lName;
                                        notification.Type = trad.Type;
                                        notification.Body = body;
                                        notification.TradingSignalId = trad.TradingSignalId.ToString();
                                        notification.CreatedById = Convert.ToInt32(trad.CreatedById);
                                        notification.Status = trad.Status;
                                        notification.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);

                                        db.Notifications.Add(notification);
                                        db.SaveChanges();
                                        // send notification
                                        SendNotification(Convert.ToInt32(trad.CreatedById), notification.Body, trad.TradingSignalId);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var trad in tradeideas)
                            {
                                int j = 0;
                                decimal countPIPS = 0;
                                var takeProfit = db.TakeProfits.Where(x => x.Disable == false && x.TradingSignalId == trad.TradingSignalId && x.Companyid == trad.Companyid).ToList();
                                foreach (var tpItem in takeProfit)
                                {
                                    j = j + 1;
                                    currentRate = Convert.ToDecimal(Convert.ToDecimal(user.ask).ToString("F4"));
                                    if (trad.Type == "1" || trad.Type == "3" || trad.Type == "6")
                                    {
                                        countPIPS = (Convert.ToDecimal(trad.Buy) - Convert.ToDecimal(currentRate)) * 10000;
                                        countPIPS = Convert.ToDecimal(trad.Buy) > Convert.ToDecimal(currentRate) ? System.Math.Abs(countPIPS) * (-1) : System.Math.Abs(countPIPS);
                                    }
                                    if (trad.Type == "2" || trad.Type == "4" || trad.Type == "5")
                                    {
                                        countPIPS = (Convert.ToDecimal(currentRate) - Convert.ToDecimal(trad.Buy)) * 10000;
                                        countPIPS = Convert.ToDecimal(currentRate) > Convert.ToDecimal(trad.Buy) ? System.Math.Abs(countPIPS) * (-1) : System.Math.Abs(countPIPS);
                                    }
                                    if (countPIPS >= Convert.ToDecimal(tpItem.PIPS))
                                    {
                                        body = "Take Profit " + tpItem.No + " Achieved +" + tpItem.PIPS + " PIPS - " + trad.CurrencyList.CurrencyName;

                                        // change tP status
                                        tpItem.Disable = true;
                                        tpItem.ModifyTime = DateTime.Now;
                                        db.Entry(tpItem).State = EntityState.Modified;
                                        db.SaveChanges();

                                        // send notification


                                        // add in past result
                                        EmployeePIPS employeePIPS = new EmployeePIPS();
                                        employeePIPS.TradingSignalId = trad.TradingSignalId;
                                        employeePIPS.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);
                                        employeePIPS.CompanyId = trad.Companyid.ToString();
                                        employeePIPS.CreatedTime = DateTime.Now;
                                        employeePIPS.ModifyTime = DateTime.Now;
                                        // disable is true because this will not show in pass result , these records will only show in past result when the idea is completely won or loss
                                        employeePIPS.Disable = true;
                                        double pipsResult = Convert.ToDouble(tpItem.PIPS);
                                        employeePIPS.PIPS = Convert.ToInt32(pipsResult);
                                        db.EmployeePIPs.Add(employeePIPS);
                                        db.SaveChanges();
                                        // Save notification
                                        Notification notification = new Notification();
                                        notification.RegisterComapanyID = Convert.ToInt32(trad.Companyid);
                                        notification.Title = "Trade Idea";
                                        notification.CreatedTime = DateTime.Now;
                                        notification.CurrencyName = trad.CurrencyList.CurrencyName;
                                        notification.EmployeeName = trad.CompanyEmployee.fName + " " + trad.CompanyEmployee.lName;
                                        notification.Type = trad.Type;
                                        notification.Body = body;
                                        notification.TradingSignalId = trad.TradingSignalId.ToString();
                                        notification.CreatedById = Convert.ToInt32(trad.CreatedById);
                                        notification.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);

                                        notification.Status = "5";
                                        db.Notifications.Add(notification);
                                        db.SaveChanges();
                                        SendNotification(Convert.ToInt32(trad.CreatedById), notification.Body, trad.TradingSignalId);
                                    }
                                }
                                // when all tp are hitted then end the trade idea
                                var findTakePrfit = db.TakeProfits.Where(x => x.Disable == false && x.TradingSignalId == trad.TradingSignalId && x.Companyid == trad.Companyid).ToList();
                                if (findTakePrfit.Count() == 0)
                                {
                                    EmployeePIPS employeePIPS = new EmployeePIPS();
                                    employeePIPS.TradingSignalId = trad.TradingSignalId;
                                    employeePIPS.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);
                                    employeePIPS.CompanyId = trad.Companyid.ToString();
                                    employeePIPS.CreatedTime = DateTime.Now;
                                    employeePIPS.ModifyTime = DateTime.Now;
                                    employeePIPS.Disable = false;
                                    // if pips greater than 11111 than won
                                    var tpend = db.TakeProfits.Where(x => x.TradingSignalId == trad.TradingSignalId).OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                                    double pipsResult = Convert.ToDouble(tpend.PIPS);
                                    employeePIPS.Status = "Won";
                                    employeePIPS.PIPS = Convert.ToInt32(pipsResult);
                                    db.EmployeePIPs.Add(employeePIPS);
                                    db.SaveChanges();
                                    //Trade Ended - EUR/JPY
                                    body = "Trade Ended & Idea Won - " + trad.CurrencyList.CurrencyName;
                                    
                                    trad.Disable = true;
                                    trad.Status = "5";
                                    db.Entry(trad).State = EntityState.Modified;
                                    db.SaveChanges();
                                    // Save notification
                                    Notification notification = new Notification();
                                    notification.RegisterComapanyID = Convert.ToInt32(trad.Companyid);
                                    notification.Title = "Trade Idea";
                                    notification.CreatedTime = DateTime.Now;
                                    notification.CurrencyName = trad.CurrencyList.CurrencyName;
                                    notification.EmployeeName = trad.CompanyEmployee.fName + " " + trad.CompanyEmployee.lName;
                                    notification.Type = trad.Type;
                                    notification.Body = body;
                                    notification.Status = trad.Status;
                                    notification.TradingSignalId = trad.TradingSignalId.ToString();
                                    notification.CreatedById = Convert.ToInt32(trad.CreatedById);
                                    notification.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);
                                    db.Notifications.Add(notification);
                                    db.SaveChanges();
                                    // SEND NOTIFICATION
                                    SendNotification(Convert.ToInt32(trad.CreatedById), notification.Body, trad.TradingSignalId);
                                    // END TRADE IDEA 
                                    break;
                                }
                                //Buy EP> CP - Pips
                                //Buy EP<CP +Pips
                                //Sell CP> EP - Pips
                                //Sell CP<EP +Pips
                                var loss = db.StopLoses.Where(x => x.Disable == false && x.TradingSignalId == trad.TradingSignalId).FirstOrDefault();
                                if (loss != null)
                                {
                                    currentRate = Convert.ToDecimal(Convert.ToDecimal(user.ask).ToString("F4"));
                                    double pipsResult = Convert.ToDouble(loss.PIPS);
                                    if (trad.Type == "1" || trad.Type == "3" || trad.Type == "6")
                                    {
                                        countPIPS = (Convert.ToDecimal(trad.Buy) - Convert.ToDecimal(currentRate)) * 10000;
                                        countPIPS = Convert.ToDecimal(trad.Buy) > Convert.ToDecimal(currentRate) ? System.Math.Abs(countPIPS) * (-1) : System.Math.Abs(countPIPS);
                                    }
                                    if (trad.Type == "2" || trad.Type == "4" || trad.Type == "5")
                                    {
                                        countPIPS = (Convert.ToDecimal(currentRate) - Convert.ToDecimal(trad.Buy)) * 10000;
                                        countPIPS = Convert.ToDecimal(currentRate) > Convert.ToDecimal(trad.Buy) ? System.Math.Abs(countPIPS) * (-1) : System.Math.Abs(countPIPS);
                                    }
                                    if (countPIPS <= Convert.ToDecimal(pipsResult))
                                    {
                                        // Save result
                                        EmployeePIPS employeePIPS = new EmployeePIPS();
                                        employeePIPS.TradingSignalId = trad.TradingSignalId;
                                        employeePIPS.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);
                                        employeePIPS.CompanyId = trad.Companyid.ToString();
                                        employeePIPS.CreatedTime = DateTime.Now;
                                        employeePIPS.ModifyTime = DateTime.Now;
                                        employeePIPS.Disable = false;
                                        // if pips less than 1 than loss
                                        if (pipsResult > 0)
                                        {
                                            employeePIPS.PIPS = -(Convert.ToInt32(pipsResult));
                                        }
                                        else if (pipsResult == 0)
                                        {
                                            employeePIPS.PIPS = -1;
                                        }
                                        else
                                        {
                                            employeePIPS.PIPS = Convert.ToInt32(pipsResult);
                                        }
                                        //db.EmployeePIPs.Add(employeePIPS);
                                        //db.SaveChanges();
                                        //
                                        // change tP status
                                        loss.Disable = true;
                                        loss.ModifyTime = DateTime.Now;
                                        db.Entry(loss).State = EntityState.Modified;
                                        db.SaveChanges();
                                        // trade idea end 
                                        trad.Disable = true;
                                        // if take profit is null or count =0 then idea shuld be loss else idea shuld be won
                                        var takeprofit = db.TakeProfits.Where(x => x.Disable == true && x.TradingSignalId == trad.TradingSignalId && x.Companyid == trad.Companyid).ToList();
                                        //if (takeprofit.Count() > 0)
                                        //{
                                        //    trad.Status = "5";
                                        //    body = "Trade Ended & Idea Won - " + trad.CurrencyList.CurrencyName;
                                        //}
                                        //else
                                        //{
                                        //    trad.Status = "4";
                                        //    body = "SL HIT unfortunately " + pipsResult + " PIPS - " + trad.CurrencyList.CurrencyName;
                                        //}
                                        if (takeProfit.Count > 0)
                                        {
                                            trad.Status = "5";
                                            body = "Trade Ended & Idea Won - " + trad.CurrencyList.CurrencyName;
                                            var tpHit = takeProfit.OrderByDescending(x => x.ModifyTime).FirstOrDefault();
                                            employeePIPS.Status = "Won";
                                            employeePIPS.PIPS = Convert.ToInt32(tpHit.PIPS);
                                            db.EmployeePIPs.Add(employeePIPS);
                                            db.SaveChanges();
                                        }

                                        else
                                        {
                                            trad.Status = "4";
                                            body = "SL HIT unfortunately " + pipsResult + " PIPS - " + trad.CurrencyList.CurrencyName;
                                            employeePIPS.Status = "Loss";
                                            employeePIPS.PIPS = Convert.ToInt32(pipsResult);
                                            db.EmployeePIPs.Add(employeePIPS);
                                            db.SaveChanges();
                                        }
                                        db.Entry(trad).State = EntityState.Modified;
                                        db.SaveChanges();

                                        Notification notification = new Notification();
                                        notification.RegisterComapanyID = Convert.ToInt32(trad.Companyid);
                                        notification.Title = "Trade Idea";
                                        notification.CreatedTime = DateTime.Now;
                                        notification.CurrencyName = trad.CurrencyList.CurrencyName;
                                        notification.EmployeeName = trad.CompanyEmployee.fName + " " + trad.CompanyEmployee.lName;
                                        notification.Type = trad.Type;
                                        notification.Body = body;
                                        notification.Status = trad.Status;
                                        notification.TradingSignalId = trad.TradingSignalId.ToString();
                                        notification.CreatedById = Convert.ToInt32(trad.CreatedById);
                                        notification.CompanyEmployeeID = Convert.ToInt32(trad.CreatedById);

                                        db.Notifications.Add(notification);
                                        db.SaveChanges();
                                        SendNotification(Convert.ToInt32(trad.CreatedById), notification.Body, trad.TradingSignalId);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public async void SendNotification(int employeeId, string body, int tradeId)
        {
            try
            {
                List<int> userid = db.AllowNotifications.Where(u => u.CompanyEmployeeID == employeeId).Select(u => u.DeviceUserId).ToList();

                var user = db.Users.ToList();

                List<string> singlebatch = new List<string>();
                for (int i = 0; i < user.Count; i++)
                {
                    bool result = true;
                    for (int j = 0; j < userid.Count; j++)
                    {
                        if (user[i].DeviceUserId == userid[j])
                        {
                            result = false;
                            break;
                        }
                    }
                    if (result)
                    {
                        singlebatch.Add(user[i].DeviceToken);
                    }
                }

                if (singlebatch.Count > 0)
                {
                    dynamic data = new
                    {
                        registration_ids = singlebatch,
                        notification = new
                        {
                            title = "Trade Idea",      // Notification title
                            body = body,              // Notification body data
                            link = "--link--",      // When click on notification user redirect to this link
                            tradeId = tradeId,
                            sound = "bing"
                        },
                        aps = new
                        {
                            badge = 0,
                            sound = "bingbong.aiff"
                        },
                        messageID = "ABCDEFIGHIJ"
                    };

                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var json = serializer.Serialize(data);
                    Byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);

                    string SERVER_API_KEY = SERVERAPIKEY;
                    string SENDER_ID = SENDERID;

                    WebRequest tRequest;
                    tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    tRequest.Method = "post";
                    tRequest.ContentType = "application/json";
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));

                    tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));

                    tRequest.ContentLength = byteArray.Length;
                    Stream dataStream = tRequest.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                    WebResponse tResponse = tRequest.GetResponse();
                    dataStream = tResponse.GetResponseStream();

                    StreamReader tReader = new StreamReader(dataStream);

                    String sResponseFromServer = tReader.ReadToEnd();

                    tReader.Close();
                    dataStream.Close();
                    tResponse.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}