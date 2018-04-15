using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;

namespace XForms.XForms
{
    public static class WebServerHelper
    {
#if __IOS__
	        public static List<T> 
                WebCall<T>(string url)
	        {
	            WebRequest request = HttpWebRequest.Create(url);
	            request.ContentType = "application/text";
	            request.Method = "GET";

	            string content = string.Empty;
	            List<T> DeserializedClass = new List<T>();
	            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
	            {                
	                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
	                {
	                    content = reader.ReadToEnd();
	                    //Console.WriteLine(content);
	                    DeserializedClass = JsonConvert.DeserializeObject<List<T>>(content);
	                }
	            }
	            return DeserializedClass;
	        }
	        public static string WebCall(string url)
	        {
	            WebRequest request = HttpWebRequest.Create(url);
	            request.ContentType = "application/text";
	            request.Method = "GET";

	            string content = string.Empty;
	            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
	            {                
	                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
	                {
	                    content = reader.ReadToEnd();                    
	                }
	            }
	            return content;
	        }

            public static string WebCall(string url, string authtoken, bool usingAuth)
        {
            WebRequest request = HttpWebRequest.Create(url);
            request.ContentType = "application/text";
            request.Method = "GET";

            if (usingAuth)
            {
                request.Headers.Add("Authorization", authtoken);
            }

            string content = string.Empty;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();
                }
            }
            return content;
        }
	        public static List<T> WebCall<T>(string url, string postdata)
	        {
	            WebRequest request = HttpWebRequest.Create(url);
	            request.ContentType = "application/text";
	            request.Method = "POST";
	            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
	            {
	                writer.Write(postdata);
	            }
	            string content = string.Empty;
	            List<T> DeserializedClass = new List<T>();
	            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
	            {
	                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
	                {
	                    content = reader.ReadToEnd();
	                    DeserializedClass = JsonConvert.DeserializeObject<List<T>>(content);
	                }
	            }
	            return DeserializedClass;
	        }

     

        public static string WebCall(string url, string postdata)
	        {
	            WebRequest request = HttpWebRequest.Create(url);
	            request.ContentType = "application/text";
	            request.Method = "POST";
	            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
	            {
	                writer.Write(postdata);
	            }
	            string content = string.Empty;
	            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
	            {
	                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
	                {
	                    content = reader.ReadToEnd();                    
	                }
	            }
	            return content;
	        }


             public async static Task<string> WebCallAsync(string url)
             {
                 string content = string.Empty;
                 WebRequest request = WebRequest.Create(url);
                 request.Method = "GET";
                 var responseTask = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
                 using (var response = (HttpWebResponse)await responseTask)
                 {
                     Stream responseStream = response.GetResponseStream();
                     using (StreamReader sr = new StreamReader(responseStream))
                     {
                         content = sr.ReadToEnd();
                     }
                 }
                 return content;
             }

             public async static Task<List<T>> WebCallAsync<T>(string url)
             {
                 List<T> DeserializedClass = new List<T>();
                 WebRequest request = WebRequest.Create(url);
                 request.Method = "GET";
                 var responseTask = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
                 using (var response = (HttpWebResponse)await responseTask)
                 {
                     string content = string.Empty;
                     Stream responseStream = response.GetResponseStream();
                     using (StreamReader sr = new StreamReader(responseStream))
                     {
                         content = sr.ReadToEnd();
                         DeserializedClass = JsonConvert.DeserializeObject<List<T>>(content);
                     }
                 }
                 return DeserializedClass;
             }
#else
        public async static Task<List<T>> WebCall<T>(string base_url, Dictionary<string, string> headers = null)
        {
            List<T> DeserializedClass = new List<T>();

            string content = string.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(base_url);
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    DeserializedClass = JsonConvert.DeserializeObject<List<T>>(content);
                }
            }
            return DeserializedClass;
        }
        public async static Task<string> WebCall(string base_url, Dictionary<string, string> headers = null)
        {
            //https://gowsdev.goget.co.nz/gomobile.svc/
            //login/{token}
            string content = string.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(base_url);
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                HttpResponseMessage response = client.GetAsync("").Result;                
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    
                }
                return content;
            }
        }
        public async static Task<List<T>> WebCall<T>(string base_url, string postdata, Dictionary<string, string> headers = null)
        {
            List<T> DeserializedClass = new List<T>();

            string content = string.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(base_url);
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                HttpResponseMessage response = client.PostAsync("", new StringContent(postdata)).Result;
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();
                    DeserializedClass = JsonConvert.DeserializeObject<List<T>>(content);
                }
            }
            return DeserializedClass;
        }
        public async static Task<string> WebCall(string base_url, string postdata, Dictionary<string, string> headers = null)
        {
            string content = string.Empty;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(base_url);
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                HttpResponseMessage response = client.PostAsync("", new StringContent(postdata)).Result;
                if (response.IsSuccessStatusCode)
                {
                    content = await response.Content.ReadAsStringAsync();                    
                }
            }            
            return content;
        }




#endif





#if __ANDROID__
        public static List<T>


        WebCall<T>(string url)
        {
           
            WebRequest request = HttpWebRequest.Create(url);
            request.ContentType = "application/text";
            request.Method = "GET";

            string content = string.Empty;
            List<T> DeserializedClass = new List<T>();

            try
            {
                
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                        //Console.WriteLine(content);
                        DeserializedClass = JsonConvert.DeserializeObject<List<T>>(content);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
            return DeserializedClass;
        }

        public static string WebCall(string url, string postdata, string authtoken, bool usingAuth)
        {
            WebRequest request = HttpWebRequest.Create(url);

            request.ContentType = "application/text";
            request.Method = "POST";
            if (usingAuth)
            {
                request.Headers.Add("Authorization", authtoken);
            }
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(postdata);
            }
            string content = string.Empty;

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();
                }
            }

            return content;
        }
        public static string WebCall(string url, string authtoken, bool usingAuth)
        {
            WebRequest request = HttpWebRequest.Create(url);
            request.ContentType = "application/text";
            request.Method = "GET";

            if (usingAuth)
            {
                request.Headers.Add("Authorization", authtoken);
            }

            string content = string.Empty;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();
                }
            }
            return content;
        }
        public static string WebCall(string url)
        {            
            WebRequest request = HttpWebRequest.Create(url);
            request.ContentType = "application/text";
            request.Method = "GET";           

            string content = string.Empty;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();
                }
            }
            return content;
        }
        public static List<T> WebCall<T>(string url, string postdata)
        {
            WebRequest request = HttpWebRequest.Create(url);
            request.ContentType = "application/text";
            request.Method = "POST";
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(postdata);
            }
            string content = string.Empty;
            List<T> DeserializedClass = new List<T>();
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();
                    DeserializedClass = JsonConvert.DeserializeObject<List<T>>(content);
                }
            }
            return DeserializedClass;
        }
        public static string WebCall(string url, string postdata)
        {
            WebRequest request = HttpWebRequest.Create(url);
            request.ContentType = "application/text";
            request.Method = "POST";
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(postdata);
            }
            string content = string.Empty;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();
                }
            }
            return content;
        }


        public async static Task<string> WebCallAsync(string url)
        {
            string content = string.Empty;
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            var responseTask = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
            using (var response = (HttpWebResponse)await responseTask)
            {
                Stream responseStream = response.GetResponseStream();
                using (StreamReader sr = new StreamReader(responseStream))
                {
                    content = sr.ReadToEnd();
                }
            }
            return content;
        }
        public async static Task<List<T>> WebCallAsync<T>(string url)
        {
            List<T> DeserializedClass = new List<T>();
            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            var responseTask = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
            using (var response = (HttpWebResponse)await responseTask)
            {
                string content = string.Empty;
                Stream responseStream = response.GetResponseStream();
                using (StreamReader sr = new StreamReader(responseStream))
                {
                    content = sr.ReadToEnd();
                    DeserializedClass = JsonConvert.DeserializeObject<List<T>>(content);
                }
            }
            return DeserializedClass;
        }



#endif

        //public static string UploadFile(string url, string sFileName, string authToken, bool usingAuth)
        //{


        //    WebRequest request = HttpWebRequest.Create(url);

        //    String headerInfo = "attachment; filename=" + $"{Path.GetFileNameWithoutExtension(sFileName)}.log";
        //    request.Method = "POST";
        //    request.Headers.Add("Content-Disposition", headerInfo);
        //    request.ContentType = "application/octet-stream";

        //    if (usingAuth)
        //    {
        //        request.Headers.Add("Authorization", authToken);
        //    }

        //    Stream rs = request.GetRequestStream();

        //    FileStream fileStream = new FileStream(sFileName, FileMode.Open, FileAccess.Read);
        //    byte[] buffer = new byte[4096];
        //    int bytesRead = 0;
        //    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
        //    {
        //        rs.Write(buffer, 0, bytesRead);
        //    }
        //    fileStream.Close();

        //    rs.Close();
        //    rs = null;

        //    string content = string.Empty;
        //    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //    {
        //        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //        {
        //            content = reader.ReadToEnd();
        //        }
        //    }
        //    return content;
        //}

        public class WebServiceResponse
        {
            public string data { get; set; }
        }




    }

}
