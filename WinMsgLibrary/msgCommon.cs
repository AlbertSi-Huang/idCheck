using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinMsgLibrary
{
   public class MsgType
    {
        public const int USER = 0x0400;
        public const int WM_START = USER + 101;
        public const int WM_UPDATE = USER + 102;
        public const int WM_SERACH = USER + 103;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COPYDATASTRUCT
    {
        public int dwData;    //not used  
        public int cbData;    //长度  
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }

    public class MsgCommonOper
    {

        



    }
}
