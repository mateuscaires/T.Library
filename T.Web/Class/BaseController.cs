using T.Common;
using T.Request;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Collections;
using System.Linq;

namespace T.Web
{
    public class BaseController : Controller
    {
        #region Constructors

        public BaseController()
        {
            //_memoryCache = new InMemoryCache();
            _path = Config.GetValueKey(CT_AD_Adress);
            LoginRequired = true;
            _request = new Request.Request();
        } 

        #endregion

        #region Fields

        private string _path;
        private string _filterAttribute;
        //private InMemoryCache _memoryCache;
        private Request.Request _request;

        #endregion

        #region Constants

        private const string CT_ContentTypeText = "application/text";

        private const string CT_ContentTypePDF = "application/PDF";

        private const string CT_ContentTypeOctetStream = "application/octet-stream";

        private const string CT_ContentTypeExcell = "application/vnd.ms-excel";

        private const string CT_Null = "null";

        public const string CT_Slash = "/";

        public const string CT_BackSlash = @"\";

        private const string CT_UserCookie = "T.SECURITY.USERNAME";

        private const string CT_PasswordCookie = "T.SECURITY.USERPASSWORD";

        public const string CT_Message = "message";

        public const string CT_AlertType = "AlertType";

        public const string CT_NoData = "Nenhum dado encontrado";

        public const string CT_JsonContent = "application/json";

        private const string CT_AD_Domain = "AD_Domain";

        private const string CT_AD_Adress = "AD_Adress";

        private const string CT_MemberOf = "memberOf";

        public const string CT_URL_API = "URL_API";

        public class Views
        {
            public const string CT_Index = "Index";
            public const string CT_Item = "Item";
            public const string CT_Items = "Items";
            public const string CT_Detail = "Detail";
        }

        public class Actions
        {
            public const string CT_Search = "Search";
            public const string CT_Index = "Index";
            public const string CT_Item = "Item";
            public const string CT_Items = "Items";
            public const string CT_Detail = "Detail";
            public const string CT_Find = "Find";
            public const string CT_GetById = "GetById";
            public const string CT_GetByName = "GetByName";
            public const string CT_GetByFollowUpId = "GetByFollowUpId";
            public const string CT_GetAll = "GetAll";
            public const string CT_GetData = "GetData";
            public const string CT_GetReport = "GetReport";
            public const string CT_GetLog = "GetLog";
            public const string CT_GetReports = "GetReports";
            public const string CT_GetTypes = "GetTypes";
            public const string CT_Post = "Post";
            public const string CT_Save = "Save";
            public const string CT_Cancel = "Cancel";
            public const string CT_FlagInvoice = "FlagInvoice";
            public const string CT_SetImport = "SetImport";
            public const string CT_Acept = "Acept";
            public const string CT_Import = "Import";
            public const string CT_InsertINPC = "InsertINPC";
            public const string CT_SelectINPC = "SelectINPC";
            public const string CT_GetAvaliableTotal = "GetAvaliableTotal";
            public const string CT_RemoveCancellation = "RemoveCancellation";
        }

        public class Buttons
        {
            public const string CT_Search = "Search";
            public const string CT_Login = "Login";
            public const string CT_Save = "Save";
            public const string CT_Update = "Update";
            public const string CT_Delete = "Delete";
            public const string CT_Insert = "Insert";
            public const string CT_ExportCsv = "ExportCsv";
            public const string CT_ExportTxt = "ExportTxt";
            public const string CT_ExportXlsx = "ExportXlsx";
        }

        public class Controllers
        {
            public const string User = "User";
            public const string UserProfile = "UserProfile";
            public const string Holder = "Holder";
            public const string Redefine = "Redefine";
            public const string HolderCancellation = "HolderCancellation";
            public const string Cover = "Cover";
            public const string Beneficiary = "Beneficiary";
            public const string Parcel = "Parcel";
            public const string ParcelManualPayment = "ParcelManualPayment";
            public const string Login = "Login";
            public const string Search = "Search";
            public const string Report = "Report";
            public const string FollowUp = "FollowUp";
            public const string FollowUpCancelation = "FollowUpCancelation";
            public const string FollowUpReasonCancelation = "FollowUpReasonCancelation";
            public const string Home = "Home";
            public const string Kinship = "Kinship";
            public const string Proposal = "Proposal";
            public const string Funding = "Funding";
            public const string Parameter = "Parameter";
            public const string Batch = "Batch";
            public const string BatchType = "BatchType";
        }

        public class Extensions
        {
            public const string CT_CSV = ".csv";
            public const string CT_TXT = ".txt";
            public const string CT_PDF = ".pdf";
            public const string CT_XLSX = ".xlsx";
        }

        public string Mensagem
        {
            get { return TempData[CT_Message] as string; }
            set { TempData[CT_Message] = value; }
        }

        public AlertType AlertType
        {
            get { return (AlertType)TempData[CT_AlertType]; }
            set { TempData[CT_AlertType] = value; }
        }

        #endregion

        #region Properties

        public bool LoginRequired { get; set; }

        public AppUser CurrentUser { get { return WebContext.CurrentUser; } }

        public bool IsLogged { get { return WebContext.IsLogged; } }

        //public ICacheService MemoryCache { get { return _memoryCache; } }

        public Encoding EncodingWindows_1254 { get { return Encoding.GetEncoding("windows-1254"); } }

        #endregion

        #region Actions

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            try
            {
                if (!WebContext.IsLogged)
                {
                    WindowsIdentity userIdentity = System.Web.HttpContext.Current.User.Identity as WindowsIdentity;
                    
                    if (userIdentity.HasValue() && !userIdentity.Name.IsNullOrEmpty())
                    {
                        WebContext.SetUserContext(userIdentity);
                    }
                }
            }
            catch
            {
                //Nothing to do.
            }

            base.OnActionExecuting(filterContext);

        }

        #endregion

        #region Public Methods

        public string GetGroups()
        {
            DirectorySearcher search = new DirectorySearcher(_path);
            search.Filter = "(cn=" + _filterAttribute + ")";
            search.PropertiesToLoad.Add(CT_MemberOf);
            StringBuilder groupNames = new StringBuilder();

            try
            {
                SearchResult result = search.FindOne();

                int propertyCount = result.Properties[CT_MemberOf].Count;

                string dn;
                int equalsIndex, commaIndex;

                for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                {
                    dn = (string)result.Properties[CT_MemberOf][propertyCounter];

                    equalsIndex = dn.IndexOf("=", 1);
                    commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }

                    groupNames.Append(dn.Substring((equalsIndex + 1), (commaIndex - equalsIndex) - 1));
                    groupNames.Append("|");

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error obtaining group names. " + ex.Message);
            }
            return groupNames.ToString();
        }

        public ActionResult GoHome()
        {
            return RedirectToAction(Actions.CT_Index, Controllers.Search);
        }

        public bool Authenticate(string username, string password)
        {
            bool autenticated = false;
            try
            {
                AppUser user = CheckCredentials(username, password);
                if (user != null)
                {
                    autenticated = true;
                    SaveCookie(username, password);
                }

            }
            catch
            {
                autenticated = false;
            }

            if (!autenticated)
                Mensagem = "Usuário e/ou senha inválidos.";

            return autenticated;
        }

        public List<AppUser> GetADListUser(string domain)
        {
            List<AppUser> users = new List<AppUser>();
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    string value;

                    Func<DirectoryEntry, string, string> GetValue = (de, @prop) =>
                    {
                        try
                        {
                            value = de.Properties[@prop.Trim()].Value.ToString();
                        }
                        catch
                        {
                            value = string.Empty;
                        }

                        return value;
                    };

                    Func<string, bool> CheckActive = (username) =>
                    {
                        using (var foundUser = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username))
                        {
                            if (foundUser.Enabled.HasValue)
                            {
                                return (bool)foundUser.Enabled;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    };

                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        //Console.WriteLine("First Name: " + de.Properties["givenName"].Value);
                        //Console.WriteLine("Last Name : " + de.Properties["sn"].Value);
                        //Console.WriteLine("SAM account name   : " + de.Properties["samAccountName"].Value);
                        //Console.WriteLine("User principal name: " + de.Properties["userPrincipalName"].Value);

                        AppUser user = new AppUser
                        {
                            ID = 0,
                            FullName = GetValue(de, "displayName"),
                            Login = GetValue(de, "samAccountName"),
                            Name = GetValue(de, "givenName"),
                            Email = GetValue(de, "mail")
                        };

                        if (user.Validate())
                        {
                            user.Active = CheckActive(user.Login);
                            users.Add(user);
                        }
                    }
                }

                return users;
            }
        }
        
        public bool IsAuthenticated()
        {
            try
            {
                if (WebContext.IsLogged)
                    return true;

                if (Request.IsNull())
                    return true;

                if (WebContext.IsLogged)
                    return true;

                HttpCookie cookieUser = Request.Cookies[CT_UserCookie];
                HttpCookie cookiePwd = Request.Cookies[CT_PasswordCookie];

                if (cookiePwd.IsNull() || cookieUser.IsNull())
                    return false;

                AppUser user = CheckCredentials(Cryptography.Decript(cookieUser.Value), Cryptography.Decript(cookiePwd.Value));

                if (user.HasValue())
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        public string MakeRoutFind(string controller)
        {
            return MakeRout(controller, Actions.CT_Find);
        }

        public string MakeRoutGetById(string controller)
        {
            return MakeRout(controller, Actions.CT_GetById);
        }

        public string MakeRoutSearch(string controller)
        {
            return MakeRout(controller, Actions.CT_Search);
        }

        public void DownloadCsv(string content, string fileName)
        {
            Download(content, fileName, Extensions.CT_CSV, CT_ContentTypeText);
        }

        public void DownloadTxt(string content, string fileName)
        {
            Download(content, fileName, Extensions.CT_TXT, CT_ContentTypeText);
        }

        public void DownloadPdf(byte[] content, string fileName)
        {
            Download(content, fileName, Extensions.CT_PDF, CT_ContentTypePDF);
        }

        public void DownloadExcel(byte[] content, string fileName)
        {
            Download(content, fileName, Extensions.CT_XLSX, CT_ContentTypeExcell);
        }

        public void Download(byte[] content, string fileName)
        {
            Download(content, fileName, string.Empty, CT_ContentTypeOctetStream);
        }

        private void Download(string content, string fileName, string extension, string contentType)
        {
            SetResponse(content.Length, fileName, extension, contentType);

            Response.Write(content);
            Response.End();
        }

        private void Download(byte[] content, string fileName, string extension, string contentType)
        {
            SetResponse(content.Length, fileName, extension, contentType);

            Response.BinaryWrite(content);
            Response.End();
        }

        private void SetResponse(int contentLength, string fileName, string extension, string contentType)
        {
            if (fileName.IsNullOrEmpty())
                fileName = Guid.NewGuid().ToString().ToUpper();

            if (extension.HasText())
            {
                if (!fileName.ToLower().EndsWith(extension))
                    fileName = string.Concat(fileName, extension);
            }

            fileName = fileName.Replace(" ", "_");
            fileName = fileName.Replace(",", "_");
            fileName = fileName.Replace(";", "_");

            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.AddHeader("Content-Length", contentLength.ToString());
            Encoding encoding = EncodingWindows_1254;
            Response.Charset = encoding.EncodingName;
            Response.ContentEncoding = encoding;
            Response.ContentType = contentType;
        }

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
        
        private AppUser CheckCredentials(string username, string password)
        {
            string domainAndUsername = string.Concat(Config.GetValueKey(CT_AD_Domain), @"\" + username);
            DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, password);

            AppUser user = new AppUser();

            try
            {
                //Bind to the native AdsObject to force authentication.
                object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);

                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return null;
                }

                //Update the new path to the user in the directory.
                _path = result.Path;
                _filterAttribute = (string)result.Properties["cn"][0];

                user.FullName = _filterAttribute;

                string[] nameParts = _filterAttribute.Split(' ');

                if (nameParts.Length > 1)
                    user.Name = string.Concat(nameParts[0], " ", nameParts[1]);
                else
                    user.Name = _filterAttribute;

                user.Login = username;

            }
            catch
            {
                return null;
            }

            WebContext.SetUserContext(user);

            return user;
        }

        private void SaveCookie(string username, string password)
        {
            string groups = GetGroups();

            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, username, DateTime.Now, DateTime.Now.AddMonths(3), true, groups);

            //Encrypt the ticket.
            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

            //Create a cookie, and then add the encrypted ticket to the cookie as data.
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

            HttpCookie pwd = new HttpCookie(CT_UserCookie, Cryptography.Encript(username));
            HttpCookie user = new HttpCookie(CT_PasswordCookie, Cryptography.Encript(password));

            authCookie.Expires = authTicket.Expiration;

            pwd.Expires = authCookie.Expires;
            user.Expires = authCookie.Expires;

            //Add the cookie to the outgoing cookies collection.
            Response.Cookies.Add(authCookie);
            Response.Cookies.Add(pwd);
            Response.Cookies.Add(user);
        }

        #endregion

        #region Request

        public void PostData(RequestConfig config)
        {
            _request.PostData(config);
        }

        public void PostData<T>(RequestConfig<T> config)
        {
            _request.PostData(config);
        }

        public T RequestDataItem<T>(string rout)
        {
            return _request.RequestDataItem<T>(rout);
        }

        public T RequestDataItem<T>(string controller, string action)
        {
            return _request.RequestDataItem<T>(controller, action);
        }

        public T RequestDataItem<T>(RequestConfig config)
        {
            return _request.RequestDataItem<T>(config);
        }

        public T RequestDataItem<T, K>(RequestConfig<K> config)
        {
            return _request.RequestDataItem<T, K>(config);
        }

        public List<T> RequestData<T>(string rout)
        {
            return _request.RequestData<T>(rout);
        }

        public List<T> RequestData<T>(string controller, string action)
        {
            return _request.RequestData<T>(controller, action);
        }

        public List<T> RequestData<T>(RequestConfig config)
        {
            return _request.RequestData<T>(config);
        }

        public List<T> RequestData<T, K>(RequestConfig<K> config)
        {
            return _request.RequestData<T, K>(config);
        }

        /*
                
        private string ExecRequest<T>(string rout)
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

        private string ExecRequest(RequestConfig config)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;

            using (HttpClient api = new HttpClient(handler))
            {
                SetApiConfig(api);

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
                SetApiConfig(api);

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

        */

        #endregion
    }    
}