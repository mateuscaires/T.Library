
using System.Collections.Generic;
using T.Interfaces;

namespace T.Common
{
    public class AppUser : IValidate
    {
        public AppUser()
        {
            Properties = new List<AppUserProperties>();
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string Login { get; set; }
        
        public string Image { get; set; }

        public UserProfile UserProfile { get; set; }

        public string Email { get; set; }

        public bool Active { get; set; }

        public List<AppUserProperties> Properties { get; set; }

        public bool Validate()
        {
            return (!Name.IsNullOrEmpty() && !FullName.IsNullOrEmpty() && !Login.IsNullOrEmpty());
        }
    }

    public class AppUserProperties
    {
        public AppUserProperties(string propertyName, string propertyValue)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }

        public string PropertyName { get; private set; }
        public string PropertyValue { get; private set; }
    }
}