using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TS_IDCheck
{
    public class BaseThread
    {
        public enum ThreadStatus
        {
            CREATED,
            RUNNING,
            STOPED,
        };

        private int m_Tid;
        private bool m_IsAlive;
        private ThreadStatus m_Status;
        private Thread m_WorkingThread;
        public static string strThreadName;

        /// <summary>
        /// 线程名称
        /// </summary>
        private static string ThreadName = "";

        /// <summary>
        /// 启动Run
        /// </summary>
        /// <param name="arg"></param>
        private static void WorkFunction(object arg)
        {
            try
            {
                strThreadName = arg.ToString();
                ThreadName = string.Format("【{0}】", strThreadName);
                Trace.WriteLine(ThreadName +  "  BaseThread::WorkFunction {");
                ((BaseThread)arg).Run();
                Trace.WriteLine(ThreadName + "  BaseThread::WorkFunction }");
            }
            catch (ThreadAbortException abt_ex)
            {
                Trace.WriteLine(ThreadName +  " BaseThread::WorkFunction ThreadAbortException " + abt_ex.ToString());
                Thread.ResetAbort();
            }
            catch (ThreadInterruptedException int_ex)
            {
                Trace.WriteLine(ThreadName +   " BaseThread::WorkFunction ThreadAbortException " + int_ex.ToString());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName +  " BaseThread::WorkFunction Exception " + ex.ToString());
            }
            finally
            {
                ((BaseThread)arg).Status = ThreadStatus.STOPED;
            }
        }

        /// <summary>
        /// 实例化
        /// </summary>
        public BaseThread()
        {
            try
            {
                m_WorkingThread = new Thread(WorkFunction);
                m_Tid = -1;
                m_IsAlive = false;
                m_Status = ThreadStatus.CREATED;
                Trace.WriteLine(ThreadName + " BaseThread实例化完成");
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ThreadName + " BaseThread实例化异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 启动线程
        /// </summary>
        /// <returns></returns>
        public int Start()
        {
            try
            {
                Trace.WriteLine(ThreadName + " BaseThread Start() begin");
                m_IsAlive = true;
                Thread.Sleep(200);
                m_WorkingThread.Start(this);
                m_Tid = m_WorkingThread.ManagedThreadId;
                m_Status = ThreadStatus.RUNNING;

                Trace.WriteLine(ThreadName + " BaseThread Start() end");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + " BaseThread start error:" + ex.ToString());
                m_Tid = -1;
                m_IsAlive = false;
                m_Status = ThreadStatus.STOPED;
            }
            return 0;
        }

        public int Stop(int timeout)
        {
            Trace.WriteLine(ThreadName + "thread stop begin....");
            try
            {
                if (m_IsAlive)
                {
                    Trace.WriteLine(ThreadName + "thread stop begin");
                    m_IsAlive = false;

                    if (timeout > 0)
                    {
                        if (!m_WorkingThread.Join(timeout))
                        {
                            Trace.WriteLine(ThreadName + " BaseThread Stop: Join failed, m_WorkingThread.Abort {");
                            m_IsAlive = false;
                            m_WorkingThread.Abort();
                            Trace.WriteLine(ThreadName + " BaseThread Stop: Join failed, m_WorkingThread.Abort }");
                        }
                    }
                    else
                    {
                        if (!m_WorkingThread.Join(3000))
                        {
                            Trace.WriteLine(ThreadName + " BaseThread Stop: Join Timer default = 3000 failed, m_WorkingThread.Abort {");
                            m_IsAlive = false;
                            m_WorkingThread.Abort();
                            Trace.WriteLine(ThreadName + " BaseThread Stop: Join Timer default = 3000 failed, m_WorkingThread.Abort }");
                        }
                    }
                }
                else
                {
                    if (m_WorkingThread != null)
                    {
                        Trace.WriteLine(ThreadName + "  BaseThread Stop: Thread status is UNUSUAL");
                        if (m_WorkingThread.IsAlive)
                        {
                            Trace.WriteLine(ThreadName + "  BaseThread Stop: Force to ABOTR ");
                            m_WorkingThread.Abort();
                        }
                    }
                }
                m_WorkingThread = null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "BaseThread Stop: " + ex.ToString());
            }
            
            return 0;
        }

        public int Tid
        {
            get { return m_Tid; }
        }

        public bool IsAlive
        {
            get { return m_IsAlive; }
        }

        public ThreadStatus Status
        {
            get { return m_Status; }
            set { m_Status = value; }
        }


        public virtual void Run() { }

        ~BaseThread()
        {
            if (m_WorkingThread != null)
            {
                Trace.WriteLine(ThreadName + "  ~BaseThread: Thread status is UNUSUAL");
                if (m_WorkingThread.IsAlive)
                {
                    Trace.WriteLine(ThreadName + "  ~BaseThread: Force to ABOTR ");
                    m_WorkingThread.Abort();
                }
                m_WorkingThread = null;
            }
        }
    }
}
