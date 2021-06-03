using System;
using System.Text;
using System.Linq;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;

namespace Converter
{
    class Program
    {
        
        static void Main(string[] args)
        {
            OneDriveConverter driveConverter = new OneDriveConverter();
            driveConverter.Auth();
            driveConverter.ConvertFile(args[0]);
        }
    }
    class OneDriveConverter
    {
        const string client_id = "b06e91ab-c3f2-46ad-96bb-9c7ed0a55018";
        const string client_secret = "PGUQ.y~~.cDg9Wrt5lal8s7LM2AR_Lc5n4";
        const string tenant = "b73f83ad-d010-4bea-9efa-7281f91efb3e";
        string urlAuth = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
        static JToken token;

        // получаем токен
        public void Auth()
        {
            Dictionary<string, string> body = new Dictionary<string, string>();
            body.Add("client_id", client_id);
            body.Add("scope", ".default");
            body.Add("grant_type", "client_credentials");
            body.Add("client_secret", client_secret);

            FormUrlEncodedContent encodedBody = new FormUrlEncodedContent(body);
            HttpClient httpClient = new HttpClient();
            var answer = httpClient.PostAsync(urlAuth, encodedBody).Result;

            var JSON = answer.Content.ReadAsStringAsync().Result;

            var Document = JObject.Parse(JSON);
            token = Document["access_token"];

        }

        // конвертируем файл
        public void ConvertFile(string fileName)
        {
            SendFile(fileName);
            GetFile(fileName);
        }

        // отправляем файл

        private void SendFile(string fileName)
        {
            byte[] file;

            var fileMemory = new MemoryStream();
            // чтение из файла
            try
            {
                using (FileStream fileStream = File.OpenRead(fileName))
                {
                    fileStream.CopyTo(fileMemory);
                    file = fileMemory.ToArray();
                }
            }
            catch
            {
                Console.WriteLine("Ошибка чтения из файла");
                return;
            }

            // put
            System.Net.WebHeaderCollection webHeader = new System.Net.WebHeaderCollection();
            webHeader.Add("Authorization", "Bearer " + token.ToString());
            string GraphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{fileName}:/content";

            System.Net.WebRequest request = System.Net.WebRequest.Create(GraphUrl);
            request.Headers = webHeader;
            request.Method = "PUT";
            request.ContentType = "text/plain";
            System.Net.WebResponse resp;
            try
            {
                using (var reqStream = request.GetRequestStream())
                {
                    reqStream.Write(file);
                }
                resp = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream stream = resp.GetResponseStream();
                System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                string s = reader.ReadToEnd();
                Console.WriteLine(s);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        }

        // получаем файл
        private void GetFile(string fileName)
        {
            System.Net.WebHeaderCollection webHeader = new System.Net.WebHeaderCollection();
            webHeader.Add("Authorization", "Bearer " + token.ToString());
            string GraphUrl = $"https://graph.microsoft.com/v1.0/drive/root:/{fileName}:/content?format=pdf";

            System.Net.WebRequest request = System.Net.WebRequest.Create(GraphUrl);
            request = System.Net.WebRequest.Create(GraphUrl);
            request.Headers = webHeader;
            request.Method = "GET";
            request.ContentType = "pdf/application";
            System.Net.WebResponse resp;
            try
            {
                resp = (System.Net.HttpWebResponse)request.GetResponse();
                System.IO.Stream stream = resp.GetResponseStream();

                using (var memoryByte = new MemoryStream())
                {
                    stream.CopyTo(memoryByte);
                    byte[] result = memoryByte.ToArray();
                    string newFileName = ChangeExtension(fileName);
                    using (var filestream = new System.IO.FileStream(newFileName, FileMode.Create))
                    {
                        filestream.Write(result, 0, result.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        } 

        // изменяем расширения файла
        private string ChangeExtension(string fileName)
        {
            return fileName.Replace(".docx", ".pdf");
        }
    }
}
