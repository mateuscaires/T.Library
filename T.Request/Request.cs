﻿using T.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace T.Request
{
    public class Request
    {
        #region Constants

        private const string CT_ContentTypeText = "application/text";

        private const string CT_ContentTypePDF = "application/PDF";

        private const string CT_ContentTypeExcell = "application/vnd.ms-excel";

        private const string CT_Null = "null";

        public const string CT_Slash = "/";

        private const string CT_UserCookie = "T.SECURITY.USERNAME";

        private const string CT_PasswordCookie = "T.SECURITY.USERPASSWORD";

        public const string CT_Message = "message";

        public const string CT_AlertType = "AlertType";

        public const string CT_NoData = "Nenhum dado encontrado";

        public const string CT_JsonContent = "application/json";

        public const string CT_BsonContent = "application/bson";

        private const string CT_AD_Domain = "AD_Domain";

        private const string CT_AD_Adress = "AD_Adress";

        private const string CT_MemberOf = "memberOf";

        private const string CT_Delete = "Delete";

        private const string CT_Get = "GET";

        private const string CT_Head = "HEAD";

        private const string CT_Options = "OPTIONS";

        private const string CT_Patch = "PATCH";

        private const string CT_Post = "POST";

        private const string CT_Put = "PUT";

        public const string CT_URL_API = "URL_API";
        
        #endregion

        #region Properties
        
        public Encoding EncodingWindows_1254 { get { return Encoding.GetEncoding("windows-1254"); } }

        #endregion
        
        #region Public Methods
        
        public string GetJsonResult<T>(T item)
        {
            string content = JsonConvert.SerializeObject(item);
            return content;
        }

        public string GetJsonResult<T>(IEnumerable<T> items)
        {
            string content = JsonConvert.SerializeObject(items);

            return content;
        }

        public string GetJsonResult(DataTable table)
        {
            string content = JsonConvert.SerializeObject(table);

            return content;
        }

        #endregion

        #region Private Methods

        private string MakeRout(string controller, string action)
        {
            return string.Concat(controller, CT_Slash, action);
        }

        private void SetApiConfig(HttpClient api)
        {
            string urlAPI = Config.GetValueKey(CT_URL_API);
            SetApiConfig(api, urlAPI);
        }

        private void SetApiConfig(HttpClient api, string url)
        {
            if(url.IsNullOrEmpty())
                url = Config.GetValueKey(CT_URL_API);

            if (!url.EndsWith(CT_Slash))
                url = string.Concat(url, CT_Slash);

            api.BaseAddress = new Uri(url);

            api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CT_JsonContent));

            api.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CT_BsonContent));
        }

        private T DeserializeItem<T>(string item)
        {
            try
            {
                if (item == CT_Null)
                    return default(T);
                return JsonConvert.DeserializeObject<T>(item);
            }
            catch
            {
                throw new Exception("Erro ao Desserializar o conteúdo:" + item);
            }
        }

        private List<T> DeserializeItems<T>(string items)
        {
            if (items == CT_Null)
                return new List<T>();
            return JsonConvert.DeserializeObject<List<T>>(items);
        }

        #endregion

        #region Request
        public void PostData(RequestConfig config)
        {
            ExecRequest(config);
        }

        public void PostData<T>(RequestConfig<T> config)
        {
            ExecRequest(config);
        }

        public T RequestDataItem<T>(string rout)
        {
            string @return = ExecRequest(rout);

            return DeserializeItem<T>(@return);
        }

        public T RequestDataItem<T>(string controller, string action)
        {
            string @return = ExecRequest(string.Concat(controller, CT_Slash, action));

            return DeserializeItem<T>(@return);
        }

        public T RequestDataItem<T>(RequestConfig config)
        {
            string @return = ExecRequest(config);

            return DeserializeItem<T>(@return);
        }

        public T RequestDataItem<T, K>(RequestConfig<K> config)
        {
            string @return = ExecRequest(config);

            return DeserializeItem<T>(@return);
        }
		
		public string RequestDataString(RequestConfig config)
        {
            try
            {
                string @return = ExecRequest(config);

                return @return;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string RequestDataString<T>(RequestConfig config)
        {
            try
            {
                string @return = ExecRequest(config);

                return @return;
            }
            catch
            {
                return string.Empty;
            }
        }

        public List<T> RequestData<T>(string rout)
        {
            string @return = ExecRequest(rout);
            return DeserializeItems<T>(@return);
        }

        public List<T> RequestData<T>(string controller, string action)
        {
            string @return = ExecRequest(string.Concat(controller, CT_Slash, action));
            return DeserializeItems<T>(@return);
        }

        public List<T> RequestData<T>(RequestConfig config)
        {
            string @return = ExecRequest(config);
            return DeserializeItems<T>(@return);
        }

        public List<T> RequestData<T, K>(RequestConfig<K> config)
        {
            string @return = ExecRequest(config);
            return DeserializeItems<T>(@return);
        }

        public string ExecRequest(string rout)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            using (HttpClient api = new HttpClient(handler))
            {
                SetApiConfig(api);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, string.Concat(api.BaseAddress, rout));

                HttpResponseMessage resp = api.SendAsync(req).Result;
                return resp.Content.ReadAsStringAsync().Result;
            }
        }

        public string ExecUrl(string url)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            using (HttpClient api = new HttpClient(handler))
            {
                SetApiConfig(api);
                api.BaseAddress = new Uri(url);
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);

                HttpResponseMessage resp = api.SendAsync(req).Result;
                return resp.Content.ReadAsStringAsync().Result;
            }
        }

        private string ExecRequest(RequestConfig config)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            using (HttpClient api = new HttpClient(handler))
            {
                SetApiConfig(api, config.URL);

                string requestUri = string.Concat(api.BaseAddress, config.Controller, CT_Slash, config.ActionName);

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, requestUri);

                HttpResponseMessage resp = api.SendAsync(req).Result;
                return resp.Content.ReadAsStringAsync().Result;
            }
        }

        private string ExecRequest<K>(RequestConfig<K> config)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            using (HttpClient api = new HttpClient(handler))
            {
                SetApiConfig(api, config.URL);

                string requestUri = string.Concat(api.BaseAddress, config.Controller, CT_Slash, config.ActionName);

                if (config.Method == HttpMethod.Get)
                {
                    if (config.Param.HasValue())
                        requestUri = string.Concat(requestUri, CT_Slash, (config.Param.IsNull() ? string.Empty : config.Param.ToString()));
                }

                HttpRequestMessage req = new HttpRequestMessage(config.Method, requestUri);

                if (config.Method == HttpMethod.Post)
                {
                    string param = string.Empty;
                    if (!config.Param.IsNull())
                        param = JsonConvert.SerializeObject(config.Param);
                    req.Content = new StringContent(param, Encoding.UTF8, CT_JsonContent);
                }

                HttpResponseMessage resp = api.SendAsync(req).Result;
                return resp.Content.ReadAsStringAsync().Result;
            }
        }

        private string ExecRequest1<K>(RequestConfig<K> config)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            using (HttpClient api = new HttpClient(handler))
            {
                SetApiConfig(api, config.URL);

                string requestUri = string.Concat(api.BaseAddress, config.Controller, CT_Slash, config.ActionName);

                MediaTypeFormatter formatter = new JsonMediaTypeFormatter();
                Task<HttpResponseMessage> resp;

                switch (config.Method.Method.ToUpper())
                {
                    case CT_Delete:
                        resp = api.DeleteAsync(requestUri);
                        break;
                    case CT_Get:
                        {
                            if (config.Param.HasValue())
                                requestUri = string.Concat(requestUri, CT_Slash, (config.Param.IsNull() ? string.Empty : config.Param.ToString()));
                            resp = api.GetAsync(requestUri);
                        }
                        break;
                    case CT_Post:
                        {
                            if (config.Param is MultipartFormDataContent)
                            {
                                resp = api.PostAsync(requestUri, config.Param as MultipartFormDataContent);
                            }
                            else
                            {
                                resp = api.PostAsync(requestUri, config.Param, formatter);
                            }
                        }
                        break;
                    case CT_Put:
                        resp = api.PutAsync(requestUri, config.Param, formatter);
                        break;
                    case CT_Head:
                    case CT_Options:
                    case CT_Patch:
                    default:
                        return string.Concat("NOT SUPPORTED Method(", config.Method.Method, ")");
                }
                
                string ret = resp.Result.Content.ReadAsStringAsync().Result;

                return ret;
            }
        }

        #endregion
    }
}
