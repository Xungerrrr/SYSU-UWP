using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Networking
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Finance : Page
    {
        public HttpClient httpClient;
        public Finance()
        {
            this.InitializeComponent();
            //Create an HTTP client object
            httpClient = new HttpClient();
        }

        /// <summary>
        /// 请求并解析JSON
        /// </summary>
        private async void GetFinanceRateJSON(object sender, RoutedEventArgs e) {
            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header)) {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header)) {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri("http://api.k780.com/?app=finance.rate&appkey=33203&sign=ad5ff571fb64e512ceaec3da7d84297f&format=json&scur=" + source.SelectedValue + "&tcur=" + target.SelectedValue);

            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";

            try {
                //Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex) {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            var rootObject = JsonObject.Parse(httpResponseBody);
            if (rootObject.GetNamedString("success") == "1") {
                var result = rootObject.GetNamedObject("result");
                var name = result.GetNamedString("ratenm");
                if (!name.Equals("/")) {
                    rateName.Text = result.GetNamedString("ratenm");
                    rate.Text = result.GetNamedString("rate");
                    update.Text = "更新: " + result.GetNamedString("update");
                }
            }
        }

        /// <summary>
        /// 请求并解析XML
        /// </summary>
        private async void GetFinanceRateXML(object sender, RoutedEventArgs e) {
            //Add a user-agent header to the GET request. 
            var headers = httpClient.DefaultRequestHeaders;

            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header)) {
                throw new Exception("Invalid header value: " + header);
            }

            header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            if (!headers.UserAgent.TryParseAdd(header)) {
                throw new Exception("Invalid header value: " + header);
            }

            Uri requestUri = new Uri("http://api.k780.com/?app=finance.rate&appkey=33203&sign=ad5ff571fb64e512ceaec3da7d84297f&format=xml&scur=" + source.SelectedValue + "&tcur=" + target.SelectedValue);

            //Send the GET request asynchronously and retrieve the response as a string.
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";

            try {
                //Send the GET request
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex) {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(httpResponseBody);
            XmlNodeList success = xml.GetElementsByTagName("success");
            if (success[0].InnerText == "1") {
                XmlNodeList ratenm = xml.GetElementsByTagName("ratenm");
                if (!ratenm[0].InnerText.Equals("/")) {
                    XmlNodeList _rate = xml.GetElementsByTagName("rate");
                    XmlNodeList _update = xml.GetElementsByTagName("update");
                    rateName.Text = ratenm[0].InnerText;
                    rate.Text = _rate[0].InnerText;
                    update.Text = "更新: " + _update[0].InnerText;
                }
            }
        }
    }
}
