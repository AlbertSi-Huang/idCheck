using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.InteropServices;

namespace TS_IDCheck
{
    #region C# serial 操作串口
    public class ComOpenDoor
    {
        private SerialPort serialOpenDoor = new SerialPort();
        private bool bInit = false;

        /// <summary>
        /// 开门延时时间ms
        /// </summary>
        private int DelayTime = 1000;

        static ComOpenDoor instance = null;

        public static ComOpenDoor getInstance()
        {
            if (instance == null)
            {
                instance = new ComOpenDoor();
            }
            return instance;
        }

        public ComOpenDoor()
        {
            try
            {
                DelayTime = ConfigOperator.Single().DoorCloseDelay;//Convert.ToInt16(ConfigurationManager.AppSettings["DoorCloseDelay"]);
            }
            catch
            {
                DelayTime = 1000;
            }
        }

        ~ComOpenDoor()
        {
            if (serialOpenDoor.IsOpen)
            {
                Trace.WriteLine("析构函数关闭串口");
                serialOpenDoor.Close();
            }
        }

        /// <summary>
        /// 初始化开门
        /// </summary>
        /// <returns></returns>
        public bool InitOpenCom()
        {
            string[] str = SerialPort.GetPortNames();
            if (str.Length < 1)
            {
                Trace.WriteLine("没有搜索到串口 串口打开失败");
                return false;
            }

            serialOpenDoor.BaudRate = 9600;
            serialOpenDoor.DataBits = 8;
            serialOpenDoor.StopBits = StopBits.One;
            serialOpenDoor.Parity = Parity.None;
            if (string.IsNullOrEmpty(ConfigOperator.Single().StrRelaySerial) == true)
            {
                serialOpenDoor.PortName = "COM1";
            }
            else
            {
                serialOpenDoor.PortName = ConfigOperator.Single().StrRelaySerial;
            }

            if (serialOpenDoor.IsOpen)
            {
                serialOpenDoor.Close();
            }
            try
            {
                serialOpenDoor.Open();
                Trace.WriteLine("串口打开成功");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(" 串口打开失败" + ex.Message);
                return false;
            }

            bInit = true;
            return true;
        }

        /// <summary>
        /// 侧屏发送开门命令
        /// </summary>
        public void Double_StaticOpenDoor()
        {
            try
            {
                if (!bInit)
                    return;
                byte[] bt = new byte[] { 0x01 };
                serialOpenDoor.Write(bt, 0, bt.Length);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 侧屏发送关门命令
        /// </summary>
        public void Double_StaticCloseDoor()
        {
            try
            {
                if (!bInit)
                    return;
                byte[] bt = new byte[] { 0xf1 };//{ 0xf1 };
                Trace.WriteLine("准备开门.....");
                serialOpenDoor.Write(bt, 0, bt.Length);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public void CloseCom()
        {
            if (serialOpenDoor.IsOpen)
            {
                Trace.WriteLine("关闭串口");
                serialOpenDoor.Close();
                serialOpenDoor.Dispose();
            }
        }

        /// <summary>
        /// 开门
        /// </summary>
        public void WriteCom()
        {
            if (!bInit)
                return;
            byte[] bt = new byte[] { 0x01 };
            serialOpenDoor.Write(bt, 0, bt.Length);
            System.Timers.Timer t = new System.Timers.Timer(DelayTime);//实例化Timer类，设置间隔时间；
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        }

        /// <summary>
        /// 关门
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!bInit)
                return;
            byte[] bt = new byte[] { 0xf1 };//{ 0xf1 };
            Trace.WriteLine("准备写入继电器，发送关门信号");
            serialOpenDoor.Write(bt, 0, bt.Length);
        }
    }
    #endregion
    //public class ComOpenDoor
    //{
    //    public const string dllPath = @"TaisauComEx.dll";

    //    [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    //    public extern static bool Taisau_Com_OpenDoor(int nTimeOut);

    //    [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    //    public extern static void Taisau_Com_CloseDoor();

    //    static ComOpenDoor instance = null;

    //    public static ComOpenDoor getInstance()
    //    {
    //        if (instance == null)
    //        {
    //            instance = new ComOpenDoor();
    //        }
    //        return instance;
    //    }

    //    private ComOpenDoor()
    //    {
    //    }

    //    ~ComOpenDoor()
    //    {
    //        Taisau_Com_CloseDoor();
    //    }
    //    public void CloseCom()
    //    {
    //        Trace.WriteLine("关闭串口");
    //        Taisau_Com_CloseDoor();
    //    }
    //    public bool InitOpenCom()
    //    {
    //        bool rel = Taisau_Com_OpenDoor(0);
    //        Taisau_Com_CloseDoor();

    //        return rel;
    //    }

    //    public bool WriteCom()
    //    {
    //        return Taisau_Com_OpenDoor(0);
    //    }

    //    public void theout()
    //    {
    //        Taisau_Com_CloseDoor();
    //    }
    //}
}
