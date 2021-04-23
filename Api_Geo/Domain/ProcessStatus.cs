using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api_Geo.Domain
{
    public class ProcessStatus
    {
        public string id { get; set; }
        public string latitud { get; set; }
        public string longitud { get; set; }
        public string estado { get; set; }
    }
}
