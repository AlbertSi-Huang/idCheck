using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ExeReadPassport630
{
    public partial class Form1 : Form
    {
        ThreadCheckMain tCheck = new ThreadCheckMain();
        CommonSimpleLog _log = null;
        public Form1()
        {
            InitializeComponent();
        }
        ReadPassPort _passportReader = new ReadPassPort();//护照
        private void Form1_Load(object sender, System.EventArgs e)
        {
            ConfigOperator.Single().InitConfig();
            _log = new CommonSimpleLog("ExeReadPassport630");
            Trace.WriteLine("begin......");
            

            Trace.Listeners.Add(_log);
            //MessageBox.Show("");
            bool bInit = _passportReader.InitPassport();
            if (!bInit)
            {
                //弹出提示并关闭主程序
                MessageBox.Show("护照启动失败");
                Process.GetCurrentProcess().Kill();
            }
            tCheck.Start();
            string path = Directory.GetCurrentDirectory();
            path += "\\device\\passport_630";
            Trace.WriteLine("set path = " + path);
            Directory.SetCurrentDirectory(path);
            //_passportReader.StartRun();
        }
    }
}
