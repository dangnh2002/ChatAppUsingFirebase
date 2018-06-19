using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using TestFirebase.Models;
using System.Security.Cryptography;
using System.Text;

namespace TestFirebase.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            //lấy dữ liệu từ DB
            var lstChannel = GetListChannel();
            return View(lstChannel);
        }
        [HttpPost]
        public ActionResult Join_channel(int ID, string Pass)
        {
            //lấy list data
            var lstChannel = GetListChannel();
            //Lưu lại password đã mã hóa 1 lần
            Session["PasswordChannel"] = mahoa(Pass);
            //mã hóa Pass 2 lần 
            var Passmahoa = Encrypt(mahoa(Pass));
            //kiểm tra pass có đúng không 
            var obj = lstChannel.Where(x => x.Id == ID && x.Pass == Passmahoa).SingleOrDefault();
            if (obj != null)
            {
                Session["ID_Channel"] = ID;
                Session["Name_Channel"] = obj.Name;
                Session["Time"] = obj.Time;
                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { data = 2 }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Add_channel(string Name, string Pass)
        {
            try
            {
                string str;
                int id;
                long dateTimeNow = (TimeZoneInfo.ConvertTimeToUtc(DateTime.Now.ToLocalTime()).Ticks - 621355968000000000) / 10000000 + 25200;
                //lấy list data
                var lstChannel = GetListChannel();
                //mã hóa Pass 2 lần
                var Passmahoa = Encrypt(mahoa(Pass));
                //lấy ra id_max trong DB
                if (lstChannel.Count != 0)
                {
                    var obj = lstChannel.OrderByDescending(x => x.Id).Take(1).SingleOrDefault();
                    id = obj.Id + 1;
                }
                else
                {
                    id = 1;
                }
                string path = Server.MapPath("~/DataBase.txt");
                StreamWriter SW = new StreamWriter(path);
                SW.Flush();
                for (int i = 0; i < lstChannel.Count; i++)
                {
                    str = lstChannel[i].Id + "," + lstChannel[i].Name + "," + lstChannel[i].Pass + "," + lstChannel[i].Time;
                    SW.WriteLine(str);
                }
                str = id + "," + Name + "," + Passmahoa + "," + dateTimeNow;
                SW.WriteLine(str);
                SW.Close();
                return Json(new { data = 1 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = 2 }, JsonRequestBehavior.AllowGet);
            }
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