using Newtonsoft.Json;
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
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Turbo.Contaxt;
using Turbo.Models;
using Turbo.ViewModel;

namespace Turbo.Controllers
{
    public class ServiceController : Controller
    {
        TurboContext db = new TurboContext();

        // GET: Service

        public static string SERVERAPIKEY = WebConfigurationManager.AppSettings["SERVER_API_KEY"];
        public static string SENDERID = WebConfigurationManager.AppSettings["SENDER_ID"];

        public async Task<bool> BackgroundService()
        {
            return true;
        }
    }
}