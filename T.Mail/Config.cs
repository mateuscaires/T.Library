using System.Linq;
using System.Configuration;

namespace T.Mail
{
    internal sealed class Config
    {
        public static string GetValueKey(string key)
        {
            if(ConfigurationManager.AppSettings.AllKeys.Where(a => a.ToLower().Equals(key.ToLower())).Count() > 0)
                return ConfigurationManager.AppSettings[key];
            return string.Empty;
        }
    }
}
