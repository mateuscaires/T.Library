using System.Collections;
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
        public RequestConfig() : base()
        {
            Method = HttpMethod.Get;
        }

        public T Param { get; set; }
        public HttpMethod Method { get; set; }
    }

    public class RequestConfig
    {
        public string Controller { get; set; }
        public string ActionName { get; set; }
        public string URL { get; set; }
    }
}

