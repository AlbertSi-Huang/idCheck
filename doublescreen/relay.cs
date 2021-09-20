using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;


namespace doublescreen
{
    class Relay
    {
        private SerialPort serialRelayPort = new SerialPort();
        private string myPortName = String.Empty; 
        public Relay(string myCom)
        {
            myPortName = myCom;
        }

        ~Relay()
        {
            serialRelayPort.Close();
        }
        public bool OpenCom(string myPortName)
        {
            /*
            string[] str = SerialPort.GetPortNames();
            if (str == null)
            {
                return false;
            }
            */
            serialRelayPort.BaudRate = 9600;
            serialRelayPort.DataBits = 8;
            serialRelayPort.StopBits = StopBits.One;
            serialRelayPort.Parity = Parity.None;
            serialRelayPort.PortName = myPortName;
            serialRelayPort.DataReceived += new SerialDataReceivedEventHandler(serialRelayPort_DataReceived);

            if (serialRelayPort.IsOpen)
            {
                serialRelayPort.Close();
            }
            try
            {
                serialRelayPort.Open();
            }
            catch
            {
                return false;
            }
            return true;
        }
        private bool WriteCom( byte[] bt )
        {
            try
            {
                serialRelayPort.Write(bt, 0, bt.Length);
                return true;
            }
            catch
            {
                return false;
            }
            

        }
        /// <summary>
        /// 打开继电器1，1S后自己关闭
        /// </summary>
        public  void  Relay1Open() 
        {
            byte[] bt = new byte[] { 0xf1 };
            WriteCom(bt);
            System.Timers.Timer t = new System.Timers.Timer(1000);//实例化Timer类，设置间隔时间为10000毫秒；
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        }
        /// <summary>
        /// 打开继电器1，Xms后自己关闭
        /// </summary>
        public void Relay1Open(Int32 Xms)
        {
            byte[] bt = new byte[] { 0xf1 };
            WriteCom(bt);
            System.Timers.Timer t = new System.Timers.Timer(Xms);//实例化Timer类，设置间隔时间为10000毫秒；
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        }
        /// <summary>
        /// 打开继电器2，1S后自己关闭
        /// </summary>
        public void Relay2Open()
        {
            byte[] bt = new byte[] { 0xf1 };
            WriteCom(bt);
            System.Timers.Timer t = new System.Timers.Timer(1000);//实例化Timer类，设置间隔时间为10000毫秒；
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        }
        /// <summary>
        /// 打开继电器2，Xms后自己关闭
        /// </summary>
        public void Relay2Open(Int32 Xms)
        {
            byte[] bt = new byte[] { 0xf1 };
            WriteCom(bt);
            System.Timers.Timer t = new System.Timers.Timer(Xms);//实例化Timer类，设置间隔时间为10000毫秒；
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        }
      

        const Int32 RS232RecvBuffLen = 100;

        /// <summary>
        /// 串口收到数据回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialRelayPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialRelayPort.IsOpen)
            {
                try
                {
                    byte[] btReceiveData = new byte[RS232RecvBuffLen];
                    serialRelayPort.Read(btReceiveData, 0, btReceiveData.Length);
                    serialRelayPort.DiscardInBuffer();
                    string strRecMsg = Encoding.UTF8.GetString(btReceiveData);
                    strRecMsg = strRecMsg.Trim('\0');
                   
                }
                catch 
                {
                }
            }
        }
        private void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            byte[] bt = new byte[] { 0xf1 };
            serialRelayPort.Write(bt, 0, bt.Length);

        }


    }
}
