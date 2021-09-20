using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPLibrary.DeailMsg
{
    public class MsgDownOpenDoor : BaseDeailMsg
    {
        public string Channel
        {
            set;
            get;
        }
    }
}
