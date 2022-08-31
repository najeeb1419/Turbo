using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Contaxt;
using Turbo.Models;

namespace Turbo.Controllers
{
    public class AuthenticationController : Controller
    {
        TurboContext db = new TurboContext();
        // GET: Authentication
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            try
            {

           
            CompanyEmployee employee = new CompanyEmployee();
            RegisterComapany company = new RegisterComapany();
            employee = db.CompanyEmployees.Where(x => x.Enable == true && x.Email == Email && x.Password == Password).FirstOrDefault();
            company = db.RegisterComapany.Where(x => x.Enable == true && x.Email == Email && x.Password == Password).FirstOrDefault();
            if (employee != null)
            {
                Session["Employee"] = employee;
                var Privilige = db.privileges.Where(x => x.Enalbe == true && x.DesignationId == employee.DesignationId).FirstOrDefault();
                if (Privilige == null || employee.IsBlocked==true)
                {
                    TempData["mdg"] = "Sorry You don't have any access please contact to the admin.";
                    TempData.Keep();
                    return RedirectToAction("Login");
                }
                
                Session["Priviliges"] = Privilige;
              
                return RedirectToAction("Index", "Home");
             
            }
            else if (company != null)
            {
                Session["Company"] = company;
                Privileges privileges = new Privileges();
                privileges.isClients = true;
                privileges.isClientView = true;
                privileges.isClientCreate = true;
                privileges.isClientUpdate = true;
                privileges.isClientDelet = true;

                privileges.isDesignation = true;
                privileges.isDesignationView = true;
                privileges.isDesignationUpdate = true;
                privileges.isDesignationCreate = true;

                privileges.isStaff = true;
                privileges.isStaffView = true;
                privileges.isStaffCreate = true;
                privileges.isStaffUpdate = true;
                privileges.isStaffDelet = true;
                privileges.isLeadStaff = true;
                privileges.isConverLeadPartner = true;

                privileges.isEmployee = true;
                privileges.isEmployeeView = true;
                privileges.isEmployeeCreate = true;
                privileges.isEmployeeUpdate = true;

                privileges.IsDashboard  =true;
                privileges.IsSetting = true;
                    privileges.isAdminAccess = true;
                Session["Priviliges"] = privileges;

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["mdg"] = "Wrong email or password!";
                TempData.Keep();
                return RedirectToAction("Login");
            }
            }
            catch (Exception ex) 
            {
               string msg = ex.Message;
                TempData["mdg"] = msg;
                TempData.Keep();
                return RedirectToAction("Login");
                throw;
            }
        }
        //catch (Exception ex)

        //{
        //    TempData["mdg"] = "database issue.    " + ex.StackTrace;
        //    TempData.Keep();
        //    return RedirectToAction("Login");
        //}
        //}
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}