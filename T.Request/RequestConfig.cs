using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

namespace T.Request
{
    /*
    public class RequestConfigArray<T> : RequestConfig<T>
    {
        public ArrayList Parameters { get; set; }
        public RequestConfigArray() : base()
        {
            Parameters = new ArrayList();
            Method = HttpMethod.Get;
        }
    }
    */

    public class RequestConfig<T> : RequestConfig
    {
        public T Param { get; set; }
    }

    public class RequestConfig
    {
        private List<RequestHeader> _headers;

        public RequestConfig()
        {
            this.Method = HttpMethod.Get;
            this._headers = new List<RequestHeader>();
            this._headers.Add(new RequestHeader()
            {
                Name = "FIXED_TOKEN_KEY",
                Value = "8FFD2D34-6A68-4E73-A96F-59E83001C033"
            });
        }

        public List<RequestHeader> Headers
        {
            get
            {
                return this._headers;
            }
        }

        public HttpMethod Method { get; set; }

        public string Controller { get; set; }

        public string ActionName { get; set; }

        public string URL { get; set; }
    }
}

