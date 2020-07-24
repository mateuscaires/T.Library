using System;
using System.Web;
using T.Common;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;

namespace T.Web
{
    public class WebContext
    {
        #region Constants

        private const string CT_CurrentUser = "CURRENT_USER";
        private const string CT_IdentityUser = "CURRENT_IDENTITY_USER";
        
        #endregion

        #region Properties

        public static AppUser CurrentUser { get { return GetCurrentUser(); } }
        public static WindowsIdentity CurrentIdentityUser { get { return GetHttpContextUserIdentity(); } }
        public static Action OnUserLogin;

        #endregion

        #region Métodos

        public static void SetUserContext(AppUser user)
        {
            HttpContext.Current.Session[CT_CurrentUser] = user;
            OnUserLogin?.Invoke();
        }

        public static void SetUserProfile(UserProfile userProfile)
        {
            if (IsLogged && userProfile.ID > 0)
            {
                CurrentUser.UserProfile = userProfile;
            }
        }

        public static void SetUserId(int id)
        {
            if (IsLogged && id > 0)
            {
                CurrentUser.ID = id;
            }
        }

        public static void SetUserImage(string content)
        {
            if (IsLogged)
            {
                CurrentUser.Image = content;
            }
        }

        public static void SetUserContext(WindowsIdentity userIdentity)
        {
            HttpContext.Current.Session[CT_IdentityUser] = userIdentity.Clone();

            AppUser current = null;

            try
            {
                //UserPrincipal userPrincipal = System.Security.Principal.WindowsIdentity.GetCurrent();//UserPrincipal.Current;
                PrincipalContext principalContext = new PrincipalContext(ContextType.Domain);

                UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(principalContext, HttpContext.Current.User.Identity.Name);

                if (userPrincipal != null)
                {
                    current = new AppUser { FullName = userPrincipal.Name, Name = userPrincipal.DisplayName, Login = userPrincipal.SamAccountName };
                }
            }
            catch
            {
                if (CurrentIdentityUser != null)
                {
                    string[] parts;

                    Func<string, string> RemoveDomain = (input) =>
                    {
                        parts = input.Split('\\');
                        return (parts.Length > 1 ? parts[1] : parts[0]);                        
                    };

                    current = new AppUser { FullName = RemoveDomain(CurrentIdentityUser.NameClaimType), Name = RemoveDomain(CurrentIdentityUser.Name), Login = RemoveDomain(CurrentIdentityUser.Name)};
                }
            }

            if (current != null)
            {
                SetUserContext(current);
            }
        }

        public static string GetIPAdress()
        {
            string IPAddress = string.Empty;

            String strHostName = HttpContext.Current.Request.UserHostAddress.ToString();

            IPAddress = System.Net.Dns.GetHostAddresses(strHostName).GetValue(0).ToString();

            return IPAddress;
        }

        public static bool IsLogged
        {
            get
            {
                try
                {
                    return (GetHttpContextUser() != null) || (GetHttpContextUserIdentity() != null);
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void Logout()
        {
            System.Web.Security.FormsAuthentication.SignOut();
            HttpContext.Current.Session.Abandon();
            HttpContext.Current.Response.Redirect("/Login", true);
        }

        public static WindowsIdentity GetHttpContextUserIdentity()
        {
            return GetHttpContextUserIdentity(HttpContext.Current);
        }

        public static AppUser GetCurrentUser()
        {
            AppUser current = GetHttpContextUser(HttpContext.Current);

            return current;
        }

        public static AppUser GetHttpContextUser()
        {
            return GetHttpContextUser(HttpContext.Current);
        }

        public static AppUser GetHttpContextUser(HttpContext context)
        {
            try
            {
                if (HttpContext.Current.Session == null)
                    return null;

                return HttpContext.Current.Session[CT_CurrentUser] as AppUser;
            }
            catch
            {
                return null;
            }
        }

        public static WindowsIdentity GetHttpContextUserIdentity(HttpContext context)
        {
            try
            {
                if (HttpContext.Current.Session == null)
                    return null;

                return HttpContext.Current.Session[CT_IdentityUser] as WindowsIdentity;
            }
            catch
            {
                return null;
            }
        }
        
        #endregion
    }
}
