using System;
using System.Collections.Generic;
using System.Text;

namespace Api_Geo
{
    class EventMessageReceived : EventArgs
    {

        public EventMessageReceived(string message) : base()
        {
            this.Message = message;
        }
        public string Message { get; set; }

    }
}
