using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.BO
{
    // Class for call the APIs by html
    public static class HtmlHelpers
    {
        public static string PostAPIRequest(string url, string para)
        {
            HttpClient client;
            string result = string.Empty;
            try
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(para);
                var content = new ByteArrayContent(messageBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                content.Headers.Add("access-control-allow-origin", "*");

                var response = client.PostAsync(url, content).Result;
                if (response.IsSuccessStatusCode)
                    result = response.Content.ReadAsStringAsync().Result;
                else
                    result = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public static async Task<string> PostAPIRequest_result(string url, string para)
        {
            HttpClient client;
            string result = string.Empty;
            try
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                // increase timeout to avoid err: "A task was cancelled."
                client.Timeout = TimeSpan.FromMinutes(30);

                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(para);
                var content = new ByteArrayContent(messageBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                content.Headers.Add("access-control-allow-origin", "*");

                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                    result = await response.Content.ReadAsStringAsync();
                else
                    result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // Console.WriteLine(ex);
                // result = ex.Message;
                result = JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = ex.Message
                });
            }

            return result;
        }

        public static async Task<Tuple<String, String>> PostAPIRequest_message(string url, string para, string type = "POST")
        {
            HttpClient client;
            string result = string.Empty;
            String message = String.Empty;

            try
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(para);
                var content = new ByteArrayContent(messageBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                content.Headers.Add("access-control-allow-origin", "*");
                if (!string.IsNullOrEmpty(type) && type == "POST")
                {
                    var response = await client.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        dynamic dynamicObject = JObject.Parse(result);
                        if (dynamicObject.status != null)
                        {
                            result = Convert.ToString(dynamicObject.status);
                            message = Convert.ToString(dynamicObject.message);
                        }
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new Tuple<String, String>(result, message);
        }

        public static string MakeHttpClientRequest(string requestUrl, Dictionary<string, string> requestContent, HttpMethod verb)
        {
            string result = string.Empty;
            using (WebClient client1 = new WebClient())
            {
                try
                {

                    var requestData = new NameValueCollection();
                    if (requestContent != null)
                    {
                        foreach (var item in requestContent)
                        {
                            requestData.Add(item.Key, item.Value);
                        }
                    }
                    byte[] response1 = client1.UploadValues(requestUrl, requestData);

                    result = System.Text.Encoding.UTF8.GetString(response1);
                }
                catch (Exception ex)
                {
                    result = ex.Message;

                }

            }
            return result;


        }
    }
}