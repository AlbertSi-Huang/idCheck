using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TS_IDCheck.http;

namespace TS_IDCheck
{
    /// <summary>
    /// 人脸算法线程
    /// </summary>
    public class InitThread : BaseThread
    {
        private GFaceRecognizer m_gface = GFaceRecognizer.GetInstance();

        /// <summary>
        /// 线程名称
        /// </summary>
        private string ThreadName = "【InitThread】";

        public int NInited { set; get; }

        public override void Run()
        {
            try
            {
                Trace.WriteLine(ThreadName + "Run() start");
                NInited = 0;
                if (!ConfigOperator.Single().InitConfig())
                {
                    Trace.TraceError(ThreadName + "配置初始化失败");
                    Process.GetCurrentProcess().Kill();
                }

                if (!InitGFace())
                {
                    Trace.WriteLine(ThreadName + "InitGFace error");
                    Process.GetCurrentProcess().Kill();
                }
                Trace.WriteLine(ThreadName + "Run() finish");


                SanTranProto.Single().ThirdQueryRegist(false);
                //if(ConfigOperator.Single().PlateChoose == 4)
                //{
                //    if (!SanTranProto.Single().ThirdQueryRegistT())
                //    {
                //        Trace.WriteLine("arm ThirdQueryRegist error");
                //        Process.GetCurrentProcess().Kill();
                //    }
                //}

                NInited = 1;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ThreadName + "算法库启动异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 初始化人脸识别算法
        /// </summary>
        /// <returns></returns>
        private bool InitGFace()
        {
            try
            {
                Trace.WriteLine(ThreadName + "初始化人脸识别");

                if (!m_gface.Init())
                {
                    Trace.TraceWarning(ThreadName + "人脸识别初始化Init失败");
                    return false;
                }
                if (!m_gface.CheckKey())
                {
                    Trace.TraceWarning(ThreadName + "人脸识别初始化CheckKey失败");
                    return false;
                }

                int feaSize = m_gface.GetFeaSize();
                Trace.WriteLine(ThreadName + "初始化人脸识别结束");
                return true;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ThreadName + "初始化人脸识别算法异常:" + ex.Message);
                return false;
            }
        }


    }
}
