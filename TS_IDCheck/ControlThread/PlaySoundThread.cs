using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Controls;

namespace TS_IDCheck
{
    public class PlaySoundThread : BaseThread
    {
        private bool _isPlay;
        private int _playNum = -1;
        private string _strFilePath = Directory.GetCurrentDirectory();

        public void SetPlay(bool isPlay, int playNum)
        {
            _isPlay = isPlay;
            _playNum = playNum;
        }

        public override void Run()
        {
            Thread.Sleep(1000);
            Trace.WriteLine("声音播放路径：" + _strFilePath);
            while (IsAlive)
            {
                if (_playNum == -1)
                {
                    Thread.Sleep(30);
                    continue;
                }
                Trace.WriteLine("准备播放声音：" + _playNum);
                switch (_playNum)
                {
                    case 1:
                        WavPlayer.Play(_strFilePath + "\\sound\\success.wav");
                        break;
                    case 2:
                        WavPlayer.Play(_strFilePath + "\\sound\\fail.wav");
                        break;
                    case 3:
                        WavPlayer.Play(_strFilePath + "\\sound\\register.wav");
                        break;
                    case 4:
                        WavPlayer.Play(_strFilePath + "\\sound\\check.wav");
                        break;
                    case 5:
                        WavPlayer.Play(_strFilePath + "\\sound\\rereadcard.wav");
                        break;
                    case 6:
                        WavPlayer.Play(_strFilePath + "\\sound\\ticketerr.wav");
                        break;
                    case 7://车票检测网络出错
                        WavPlayer.Play(_strFilePath + "\\sound\\ticketneterr.wav");
                        break;
                    case 8://车辆已发班
                        WavPlayer.Play(_strFilePath + "\\sound\\ticketon.wav");
                        break;
                    case 9://证件过期
                        WavPlayer.Play(_strFilePath + "\\sound\\cardinvalid.wav");
                        break;
                    case 10://证件过期
                        WavPlayer.Play(_strFilePath + "\\sound\\ticketcheck.wav");
                        break;
                }
                _isPlay = false;
                _playNum = -1;
            }
        }
    }
}