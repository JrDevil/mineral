﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MineralCLI.Network
{
    public class RcpClient
    {
        public class ResponeData
        {
            public JObject Result { get; set; }
            public HttpStatusCode StatusCode { get; set; }
        }

        public static JObject RequestGet(string url)
        {
            string respone_data = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var respone = client.GetAsync(url).Result;
                    if (respone.IsSuccessStatusCode)
                    {
                        respone_data = respone.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return JObject.Parse(respone_data);
        }

        public static JObject RequestPost(string url, string text)
        {
            string respone_data = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var respone = client.PostAsync(url, new StringContent(text, Encoding.UTF8, "application/json")).Result;
                    if (respone.IsSuccessStatusCode)
                    {
                        respone_data = respone.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return JObject.Parse(respone_data);
        }

        public static async Task<JObject> RequestGetAnsyc(string url)
        {
            string respone_data = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var respone = await client.GetAsync(url);
                    respone.EnsureSuccessStatusCode();

                    respone_data = await respone.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return JObject.Parse(respone_data);
        }

        public static async Task<JObject> RequestPostAnsyc(string url, string text)
        {
            string respone_data = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var respone = await client.PostAsync(url, new StringContent(text, Encoding.UTF8, "application/json"));
                    respone.EnsureSuccessStatusCode();

                    respone_data = await respone.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine(respone_data);
            return respone_data != null ? JObject.Parse(respone_data) : null;
        }
    }
}