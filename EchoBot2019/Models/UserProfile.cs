using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot2019.Models
{
    public class UserProfile
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description { get; internal set; }
        public DateTime CallbackTime { get; internal set; }
        public string PhoneNumber { get; internal set; }
        public string Bug { get; internal set; }
    }
}
