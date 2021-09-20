using System.Diagnostics;

namespace ExeReadPassport630
{
    public class ThreadCheckMain
    {
        System.Timers.Timer _tCheck;
        const string _strMainProcess = "ts_idcheck";
        int _nLoop = 0;
        
        public void Start()
        {
            _tCheck = new System.Timers.Timer(3000);
            _tCheck.Elapsed += TCheckElapsed;
            _tCheck.AutoReset = true;
            _tCheck.Enabled = true;
        }

        private void TCheckElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Process[] procs = Process.GetProcesses();
            bool bHave = false;
            for (int i = 0; i < procs.Length; ++i)
            {
                if (procs[i].ProcessName.ToLower().CompareTo("ts_idcheck") == 0)
                {
                    bHave = true;
                    break;
                }
            }
            if (!bHave) _nLoop++;
            if(_nLoop >= 2)
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
