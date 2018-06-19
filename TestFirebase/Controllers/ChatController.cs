using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TestFirebase.Models;

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
        public ActionResult JoinMe(int channelID, string ChannelPassword)
        {
            if (channelID != 0 && !string.IsNullOrEmpty(ChannelPassword))
            {
                var list_channel = GetListChannel();
                var password = Encrypt(ChannelPassword);
                var result = list_channel.Where(x => x.Id == channelID && x.Pass == password).FirstOrDefault();
                if (result != null)
                {
                    Session["ID_Channel"] = result.Id;
                    Session["Name_Channel"] = result.Name;
                    return RedirectToAction("RedirectToChannel", "Chat");
                }
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult RedirectToChannel()
        {
            return View();
        }
        public List<Channel> GetListChannel()
        {
            //khởi tạo đầu ra
            List<Channel> lstChannel = new List<Channel>();
            long dateTimeNow = (TimeZoneInfo.ConvertTimeToUtc(DateTime.Now.ToLocalTime()).Ticks - 621355968000000000) / 10000000 + 25200;
            string str;
            //khai báo đường dẫn
            string path = Server.MapPath("~/DataBase.txt");
            StreamReader SR = new StreamReader(path);
            while ((str = SR.ReadLine()) != null)
            {
                //chuyển từ chuỗi sang obj channel
                string[] arr = str.Trim().Split(',');
                Channel c = new Channel()
                {
                    Id = int.Parse(arr[0]),
                    Name = arr[1],
                    Pass = arr[2],
                    Time = int.Parse(arr[3])

                };
                //add obj vào list
                lstChannel.Add(c);
            }
            SR.Close();
            var result = lstChannel.Where(x => (dateTimeNow - x.Time) < 86400).ToList();
            return (result);
        }
        public string mahoa(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bHash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sbHash = new StringBuilder();
            foreach (byte b in bHash)
            {
                sbHash.Append(String.Format("{0:x2}", b));
            }
            return sbHash.ToString();
        }
        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "S6|d'qc1GG,'rx&xn0XC";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
    }
}