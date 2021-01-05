using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace xdfdownloader
{
    public class Web
    {
        private IWebDriver driver;
        private WebDriverWait wait;
  

        public Web()
        {
            driver = new ChromeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromMinutes(10));
        }
        public void Login(string user, string pwd)
        {
            driver.Navigate().GoToUrl("https://il.xdf.cn/plus/");
            driver.FindElement(By.ClassName("loginBtn")).FindElement(By.ClassName("student")).Click();
            driver.FindElement(By.Id("txtUser")).SendKeys(user);
            driver.FindElement(By.Id("txtPwd")).SendKeys(pwd);
            driver.FindElement(By.Id("btnLogin")).Click();
        }

        public void WaitUserSelect()
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(By.ClassName("item")));
            wait.Until(ExpectedConditions.UrlContains("class-detail"));
        }

        public void DownloadAllVideo()
        {
            ReadOnlyCollection<IWebElement> list = GetVideoList();
            Thread.Sleep(1000);

            foreach (var box in list)
            {
                string title = ClickVideoMenu(box);

                string window = ClickPlayButton();

                DownloadVideo(title);

                driver.SwitchTo().Window(window);
            }
        }

        private string ClickVideoMenu(IWebElement box)
        {
            var item = box.FindElement(By.ClassName("text"));
            var title = item.Text.Trim();
            //Console.WriteLine(title);
            Thread.Sleep(1000);

            item.Click();
            wait.Until(ExpectedConditions.ElementExists(By.ClassName("scheduleTaskList")));
            Thread.Sleep(2000);
            return title;
        }

        private string ClickPlayButton()
        {
            //var practice = driver.FindElements(By.ClassName("goPractice"));
            //if (!practice.Any())
            //{
            //    practice = driver.FindElements(By.ClassName("rePractice"));
            //}
            var practice = driver.FindElements(By.CssSelector(".goPractice, .rePractice"));
            var window = driver.CurrentWindowHandle;
            practice.First().Click();
            return window;
        }

        private ReadOnlyCollection<IWebElement> GetVideoList()
        {
            var list = driver.FindElements(By.ClassName("secondLevel-box"));
            if (!list.Any())
            {
                list = driver.FindElements(By.ClassName("firstLevel-box"));
            }
            else
            {
                var menus = driver.FindElements(By.ClassName("firstLevel-box"));
                foreach (var menu in menus.Skip(1))
                {
                    menu.FindElement(By.ClassName("text")).Click();
                }
                Thread.Sleep(1000);
                list = driver.FindElements(By.ClassName("secondLevel"));
            }
            Console.WriteLine("items: {0}", list.Count);
            
            return list;
        }

        private void DownloadVideo(string title)
        {
            driver.SwitchTo().Window(driver.WindowHandles.Last());
            wait.Until(ExpectedConditions.UrlContains("video"));
            var url = driver.FindElement(By.TagName("video")).GetAttribute("src");
            var file = title + ".mp4";
            Console.WriteLine("{0} -> {1}", url, file);
            var bytes = DownloadByApiCall(url);
            ByteArrayToFile(file, bytes);
            driver.Close();
        }

        private bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught when write file: {0}", ex);
                return false;
            }
        }

        private byte[] DownloadByApiCall(string url)
        {
            var uri = new Uri(url);
            byte[] data = null;
            try
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.CookieContainer = new CookieContainer();
                foreach (var cookie in driver.Manage().Cookies.AllCookies)
                    webRequest.CookieContainer.Add(new System.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, string.IsNullOrWhiteSpace(cookie.Domain) ? uri.Host : cookie.Domain));

                var webResponse = (HttpWebResponse)webRequest.GetResponse();
                var ms = new MemoryStream();
                var responseStream = webResponse.GetResponseStream();
                responseStream.CopyTo(ms);
                data = ms.ToArray();
                responseStream.Close();
                webResponse.Close();
            }
            catch (WebException ex)
            {
                Console.WriteLine("Exception caught when download: {0}", ex.Response);
            }

            return data;
        }
    }
}
