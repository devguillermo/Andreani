using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryCommond
{
    public class AppSettings
    {
        public string baseUrlOms { get; set; }
        public string UserNameRabbit { get; set; }

        public string PasswordRabbit { get; set; }

        public string VirtualHost { get; set; }

        public string HostNameRabbit { get; set; }

        public string QueueSend { get; set; }

        public string QueueResponse { get; set; }

        public string HostDb { get; set; }

        public string UserDb { get; set; }

        public string PasswordDb { get; set; }
        public string NameDb { get; set; }

        public int PortDb { get; set; }


    }
}
