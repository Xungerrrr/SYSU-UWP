using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Networking {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Weather : Page {

        public HttpClient httpClient;
        public Weather() {
            this.InitializeComponent();
            //Create an HTTP client object
            httpClient = new HttpClient();
        }

        /// <summary>
        /// 请求并解析JSON
        /// </summary>
        private async void GetWeatherJSON(object sender, RoutedEventArgs e) {
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

            Uri requestUri = new Uri("http://api.k780.com/?app=weather.today&appkey=33203&sign=ad5ff571fb64e512ceaec3da7d84297f&format=json&weaid=" + city.Text.Trim());

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
                cityName.Text = result.GetNamedString("citynm");
                date.Text = result.GetNamedString("days") + " " + result.GetNamedString("week");
                currentTemperature.Text = result.GetNamedString("temperature_curr");
                weather.Text = result.GetNamedString("weather");
                temperature.Text = result.GetNamedString("temperature");
                detail.Text = "空气质量指数: " + result.GetNamedString("aqi") + "    风速: " + result.GetNamedString("wind")
                    + " " + result.GetNamedString("winp") + "    湿度: " + result.GetNamedString("humidity");
            }
        }

        /// <summary>
        /// 请求并解析XML
        /// </summary>
        private async void GetWeatherXML(object sender, RoutedEventArgs e) {
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

            Uri requestUri = new Uri("http://api.k780.com/?app=weather.today&appkey=33203&sign=ad5ff571fb64e512ceaec3da7d84297f&format=xml&weaid=" + city.Text.Trim());

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
                XmlNodeList citynm = xml.GetElementsByTagName("citynm");
                XmlNodeList days = xml.GetElementsByTagName("days");
                XmlNodeList week = xml.GetElementsByTagName("week");
                XmlNodeList temperature_curr = xml.GetElementsByTagName("temperature_curr");
                XmlNodeList _weather = xml.GetElementsByTagName("weather");
                XmlNodeList _temperature = xml.GetElementsByTagName("temperature");
                XmlNodeList humidity = xml.GetElementsByTagName("humidity");
                XmlNodeList wind = xml.GetElementsByTagName("wind");
                XmlNodeList winp = xml.GetElementsByTagName("winp");
                XmlNodeList aqi = xml.GetElementsByTagName("aqi");
                cityName.Text = citynm[0].InnerText;
                date.Text =  days[0].InnerText + " " + week[0].InnerText;
                currentTemperature.Text =  temperature_curr[0].InnerText;
                weather.Text = _weather[0].InnerText; 
                temperature.Text =  _temperature[0].InnerText;
                detail.Text =  "空气质量指数: " + aqi[0].InnerText + "    风速: " + wind[0].InnerText + " " + winp[0].InnerText
                    + "    湿度: " + humidity[0].InnerText;
            }
        }
    }
}
