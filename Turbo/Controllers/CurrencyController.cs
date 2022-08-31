using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Contaxt;
using Turbo.Models;

namespace Turbo.Controllers
{
    public class CurrencyController : Controller
    {
        TurboContext db = new TurboContext();
        List<CurrencyList> currency = new List<CurrencyList>();
        // GET: Currency
        [HttpGet]

        public void SaveCurrencies()
        {
            CurrencyList currencyList = new CurrencyList();

            currency.Add(new CurrencyList() { CurrencyName = "EUR/JPY", CurrencyNo = 3 });
            currency.Add(new CurrencyList() { CurrencyName = "EUR/USD", CurrencyNo = 1 });
            currency.Add(new CurrencyList() { CurrencyName = "EUR/NZD", CurrencyNo = 5 });
            currency.Add(new CurrencyList() { CurrencyName = "EUR/GBP", CurrencyNo = 4 });
            currency.Add(new CurrencyList() { CurrencyName = "EUR/AUD", CurrencyNo = 12 });
            currency.Add(new CurrencyList() { CurrencyName = "EUR/CHF", CurrencyNo = 2 });
            currency.Add(new CurrencyList() { CurrencyName = "EUR/CAD", CurrencyNo = 6 });
            currency.Add(new CurrencyList() { CurrencyName = "CAD/JPY", CurrencyNo = 35 });
            currency.Add(new CurrencyList() { CurrencyName = "CAD/CHF", CurrencyNo = 50 });
            currency.Add(new CurrencyList() { CurrencyName = "CHF/JPY", CurrencyNo = 52 });
            currency.Add(new CurrencyList() { CurrencyName = "GBP/USD", CurrencyNo = 39 });
            currency.Add(new CurrencyList() { CurrencyName = "GBP/JPY", CurrencyNo = 113 });
            currency.Add(new CurrencyList() { CurrencyName = "GBP/AUD", CurrencyNo = 48 });
            currency.Add(new CurrencyList() { CurrencyName = "GBP/CAD", CurrencyNo = 40 });
            currency.Add(new CurrencyList() { CurrencyName = "GBP/CHF", CurrencyNo = 41 });
            currency.Add(new CurrencyList() { CurrencyName = "GBP/NZD", CurrencyNo = 42 });
            currency.Add(new CurrencyList() { CurrencyName = "USD/CAD", CurrencyNo = 18 });
            currency.Add(new CurrencyList() { CurrencyName = "USD/JPY", CurrencyNo = 20 });
            currency.Add(new CurrencyList() { CurrencyName = "USD/CHF", CurrencyNo = 19 });
            currency.Add(new CurrencyList() { CurrencyName = "AUD/NZD", CurrencyNo = 114 });
            currency.Add(new CurrencyList() { CurrencyName = "AUD/USD", CurrencyNo = 13 });
            currency.Add(new CurrencyList() { CurrencyName = "AUD/CAD", CurrencyNo = 16 });
            currency.Add(new CurrencyList() { CurrencyName = "AUD/JPY", CurrencyNo = 14 });
            currency.Add(new CurrencyList() { CurrencyName = "AUD/CHF", CurrencyNo = 15 });
            currency.Add(new CurrencyList() { CurrencyName = "NZD/USD", CurrencyNo = 112 });
            currency.Add(new CurrencyList() { CurrencyName = "NZD/JPY", CurrencyNo = 62 });
            currency.Add(new CurrencyList() { CurrencyName = "NZD/CAD", CurrencyNo = 65 });
            currency.Add(new CurrencyList() { CurrencyName = "NZD/CHF", CurrencyNo = 1243 });
            currency.Add(new CurrencyList() { CurrencyName = "XAU/USD", CurrencyNo = 1984 });
        }


        public ActionResult CurrencyView()
        {
            if (Session["Employee"] != null || Session["Company"] != null)
            {
                int companyid = 0;
                string name = "";
                int? CreatedById = 0;
                if (Session["Company"] != null)
                {
                    RegisterComapany company = new RegisterComapany();
                    company = Session["Company"] as RegisterComapany;
                    companyid = company.RegisterComapanyID;
                    name = company.Name;
                    CreatedById = null;
                }
                else
                {
                    CompanyEmployee employee1 = new CompanyEmployee();
                    employee1 = Session["Employee"] as CompanyEmployee;
                    companyid = employee1.Companyid;
                    name = employee1.fName + " " + employee1.lName;
                    CreatedById = employee1.CompanyEmployeeID;
                }
                //SaveCurrencies();
                //foreach (var item in currency)
                //{
                //    db.CurrencyLists.Add(item);
                //    db.SaveChanges();
                //}

                ViewBag.CurrencyList = db.CurrencyLists.ToList();

                var currencies = db.Currencies.Where(x => x.Disable == false && x.Companyid == companyid.ToString()).ToList();
                return View(currencies);
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }
        }
        [HttpPost]
        public ActionResult CurrencyView(Currencies currencies)
        {
            if (Session["Employee"] != null || Session["Company"] != null)
            {
                int companyid = 0;
                string name = "";
                int? CreatedById = 0;
                if (Session["Company"] != null)
                {
                    RegisterComapany company = new RegisterComapany();
                    company = Session["Company"] as RegisterComapany;
                    companyid = company.RegisterComapanyID;
                    name = company.Name;
                    CreatedById = null;
                }
                else
                {
                    CompanyEmployee employee1 = new CompanyEmployee();
                    employee1 = Session["Employee"] as CompanyEmployee;
                    companyid = employee1.Companyid;
                    name = employee1.fName + " " + employee1.lName;
                    CreatedById = employee1.CompanyEmployeeID;
                }
                //currencies.FirstCurrency = currencies.FirstCurrency;
                //currencies.SecondCurrency = currencies.SecondCurrency;
                //currencies.FirstCurrencyImage=
                currencies.Companyid = companyid.ToString();
                currencies.CreatedBy = name;
                currencies.CreatedTime = DateTime.Now.AddHours(5);
                currencies.ModifyBy = name;
                currencies.ModifyTime = DateTime.Now.AddHours(5);
                currencies.CreatedById = CreatedById;
                db.Currencies.Add(currencies);
                db.SaveChanges();
                TempData["UpdateCurrency"] = "Currecy added successfully.";
                return RedirectToAction("CurrencyView");
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }
        }

        [HttpGet]
        public ActionResult EditCurrency(int id)
        {
            if (Session["Employee"] != null || Session["Company"] != null)
            {
                int companyid = 0;
                string name = "";
                int? CreatedById = 0;
                if (Session["Company"] != null)
                {
                    RegisterComapany company = new RegisterComapany();
                    company = Session["Company"] as RegisterComapany;
                    companyid = company.RegisterComapanyID;
                    name = company.Name;
                    CreatedById = null;
                }
                else
                {
                    CompanyEmployee employee1 = new CompanyEmployee();
                    employee1 = Session["Employee"] as CompanyEmployee;
                    companyid = employee1.Companyid;
                    name = employee1.fName + " " + employee1.lName;
                    CreatedById = employee1.CompanyEmployeeID;
                }
                ViewBag.CurrencyList = db.CurrencyLists.ToList();
                var currencies = db.Currencies.Where(x => x.Disable == false && x.Companyid == companyid.ToString() && x.CurrenciesId == id).FirstOrDefault();
                return View(currencies);
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }
        }
        [HttpPost]
        public ActionResult EditCurrency(Currencies cur)
        {
            if (Session["Employee"] != null || Session["Company"] != null)
            {
                int companyid = 0;
                string name = "";
                int? CreatedById = 0;
                if (Session["Company"] != null)
                {
                    RegisterComapany company = new RegisterComapany();
                    company = Session["Company"] as RegisterComapany;
                    companyid = company.RegisterComapanyID;
                    name = company.Name;
                    CreatedById = null;
                }
                else
                {
                    CompanyEmployee employee1 = new CompanyEmployee();
                    employee1 = Session["Employee"] as CompanyEmployee;
                    companyid = employee1.Companyid;
                    name = employee1.fName + " " + employee1.lName;
                    CreatedById = employee1.CompanyEmployeeID;
                }
                var currencies = db.Currencies.Where(x => x.Disable == false && x.Companyid == companyid.ToString() && x.CurrenciesId == cur.CurrenciesId).FirstOrDefault();
                //var findcurfromlist = db.CurrencyLists.Where(x => x.CurrencyName == cur.CurrencyList.CurrencyName).FirstOrDefault();
                if (currencies != null)
                {

                    currencies.FirstCurrency = cur.FirstCurrency;
                    currencies.SecondCurrency = cur.SecondCurrency;
                    if (cur.FirstCurrencyImage != null)
                    {
                        currencies.FirstCurrencyImage = cur.FirstCurrencyImage;
                    }
                    if (cur.SecondCurrencyImage != null)
                    {
                        currencies.SecondCurrencyImage = cur.SecondCurrencyImage;
                    }
                    currencies.CurrencyListId = cur.CurrencyListId;
                    currencies.ModifyBy = name;
                    currencies.CreatedById = CreatedById;
                    currencies.ModifyTime = DateTime.Now.AddHours(5);
                    db.Entry(currencies).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["UpdateCurrency"] = "Currency updated sucessfully.";
                }
                else
                {
                    ViewBag.mdg = "Currency not found.";
                }
                return RedirectToAction("CurrencyView");
            }
            else
            {
                return RedirectToAction("Login", "Authentication");
            }
        }

        [HttpGet]
        public JsonResult CheckCurrency(int id, int Curren)
        {
            string msg = "session";
            if (Session["Employee"] != null || Session["Company"] != null)
            {
                int companyid = 0;
                string name = "";
                int? CreatedById = 0;
                if (Session["Company"] != null)
                {
                    RegisterComapany company = new RegisterComapany();
                    company = Session["Company"] as RegisterComapany;
                    companyid = company.RegisterComapanyID;
                    name = company.Name;
                    CreatedById = null;
                }
                else
                {
                    CompanyEmployee employee1 = new CompanyEmployee();
                    employee1 = Session["Employee"] as CompanyEmployee;
                    companyid = employee1.Companyid;
                    name = employee1.fName + " " + employee1.lName;
                    CreatedById = employee1.CompanyEmployeeID;
                }

                if (id > 0)
                {
                    var findcurrency = db.Currencies.Where(x => x.Disable == false && x.Companyid == companyid.ToString() && x.CurrencyListId== Curren && x.CurrenciesId != id).FirstOrDefault();

                    if (findcurrency == null)
                    {
                        msg = "ok";
                    }
                    else
                    {
                        msg = "no";
                    }
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var findcurrency = db.Currencies.Where(x => x.Disable == false && x.Companyid == companyid.ToString() && x.CurrencyListId == Curren).FirstOrDefault();
                    if (findcurrency == null)
                    {
                        msg = "ok";
                    }
                    else
                    {
                        msg = "no";
                    }
                    return Json(msg, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ClientImage()
        {
            JsonResult result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                var file = Request.Files[0];
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string subPath = "~/Images/Currency/";
                bool isExist = Directory.Exists(Server.MapPath(subPath));
                if (isExist == false)
                {
                    Directory.CreateDirectory(Server.MapPath(subPath));
                }
                else
                {
                    file.SaveAs(HttpContext.Server.MapPath(subPath) + fileName);
                    result.Data = new { Success = true, ImageURL = string.Format(fileName) };
                }
            }
            catch (Exception ex)
            {
                result.Data = ex.Message;
            }
            return result;
        }
    }
}