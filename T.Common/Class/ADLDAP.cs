using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace T.Common
{
    public static class ADLDAP
    {
        public static List<AppUser> GetADUsers(string ldap)
        {
            List<AppUser> users = new List<AppUser>();

            try
            {
                DirectoryEntry searchRoot = new DirectoryEntry(ldap);
                DirectorySearcher search = new DirectorySearcher(searchRoot);
                search.Filter = "(&(objectClass=user)(objectCategory=person))";

                SearchResult result = null;
                SearchResultCollection resultCol = search.FindAll();

                Func<string, string> GetPropertyValue = (propertyName) =>
                {
                    try
                    {
                        return result.Properties[propertyName][0].ToString();
                    }
                    catch
                    {
                        return string.Empty;
                    }
                };

                if (resultCol != null)
                {
                    for (int counter = 0; counter < resultCol.Count; counter++)
                    {
                        string current = string.Empty;
                        result = resultCol[counter];
                        if (result.Properties.Contains("samaccountname") || result.Properties.Contains("mail") || result.Properties.Contains("displayname"))
                        {
                            AppUser user = new AppUser();
                            user.Email = GetPropertyValue("mail");
                            user.Login = GetPropertyValue("samaccountname");
                            user.Name = GetPropertyValue("displayname");

                            ResultPropertyCollection prop = result.Properties;
                            ICollection coll = prop.PropertyNames;
                            IEnumerator enu = coll.GetEnumerator();

                            while (enu.MoveNext())
                            {
                                current = (enu.Current ?? string.Empty).ToString();
                                if (current.IsNullOrEmpty())
                                    continue;

                                user.Properties.Add(new AppUserProperties(current, GetPropertyValue(current)));
                            }
                            
                            user.Active = !user.Properties.Where(a => a.PropertyValue.ToLower().Contains("desativado")).HasItems();

                            if(user.Active)
                            {
                                AppUserProperties p = user.Properties.Where(a => a.PropertyName == "useraccountcontrol").FirstOrDefault();

                                if (p.PropertyValue == "514" || p.PropertyValue == "66050")
                                    user.Active = false;
                            }
                            users.Add(user);
                        }
                    }
                }
            }
            catch
            {

            }

            return users;
        }

        public static void ResetPassword(string userName, string newPassword)
        {
            using (var context = new PrincipalContext(ContextType.Domain))
            {
                using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                {
                    user.SetPassword(newPassword);
                    if(user.IsAccountLockedOut())
                    {
                        user.UnlockAccount();
                    }

                    user.Save();
                }
            }
        }

        public static void UnlockAccount(string userName)
        {
            using (var context = new PrincipalContext(ContextType.Domain))
            {
                using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                {
                    if (user.IsAccountLockedOut())
                    {
                        user.UnlockAccount();
                    }

                    user.Save();
                }
            }
        }

        public static void ResetPassword(string ldap, string userName, string newPassword)
        {
            try
            {
                DirectoryEntry directoryEntry = new DirectoryEntry(ldap, userName, newPassword);

                if (directoryEntry != null)
                {
                    DirectorySearcher searchEntry = new DirectorySearcher(directoryEntry);
                    searchEntry.Filter = "(samaccountname=" + userName + ")";
                    SearchResult result = searchEntry.FindOne();
                    if (result != null)
                    {
                        DirectoryEntry userEntry = result.GetDirectoryEntry();
                        if (userEntry != null)
                        {
                            userEntry.Invoke("SetPassword", new object[] { newPassword });
                            userEntry.Properties["lockouttime"].Value = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
