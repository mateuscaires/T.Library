using System;

namespace T.Common
{
    public class UserProfile
    {
        public int ID { get; set; }
	    public string Description { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
