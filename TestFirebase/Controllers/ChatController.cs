using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace TestFirebase.Controllers
{
    public class ChatController : Controller
    {
        // GET: Chat
        [HttpPost]
        public ActionResult Index(int channel)
        {
            if (Session["ID_Channel"].ToString() == channel.ToString())
            {
                ViewBag.ID_channel = channel;
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public ActionResult OutChannel()
        {
            Session["ID_Channel"] = "";
            return Redirect("/");
        }
        public ActionResult LogOut()
        {
            //đăng xuất tài khoản
            Session.Abandon();
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();
            return Redirect("/");
        }
    }
}