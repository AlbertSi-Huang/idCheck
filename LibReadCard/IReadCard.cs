using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LibReadCard
{
    /// <summary>
    /// 读卡完成委托
    /// </summary>
    /// <param name="cardInfo">卡信息</param>
    /// <param name="nType">0 身份证 ， 1 ic卡 ， 2 护照</param>
    public delegate void ReadCardDelegate(object cardInfo, int nType);
    public interface IReadCard
    {
        /// <summary>
        /// 是否打开
        /// </summary>
        bool IsOpened { set; get; }

        /// <summary>
        /// 是否需要读卡
        /// </summary>
        bool IsNeedCapture { set; get; }
        
        /// <summary>
        /// 读卡器名称
        /// </summary>
        string Name { set; get; }

        /// <summary>
        /// 完成读卡事件
        /// </summary>
        event ReadCardDelegate OnReadCardEvent;

        /// <summary>
        /// 读卡器初始化
        /// </summary>
        /// <returns></returns>
        bool Init();
        
        /// <summary>
        /// 循环读卡
        /// </summary>
        void ReadCard();

        /// <summary>
        /// 关闭读卡
        /// </summary>
        void Close();
    }
}
