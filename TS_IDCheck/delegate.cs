using System.Drawing;
using Emgu.CV;
using TCPLibrary.DeailMsg;
using Common;

namespace TS_IDCheck
{
    /// <summary>
    /// 视频显示
    /// </summary>
    /// <param name="frame"></param>
    public delegate void VedioPlayDelegate(Bitmap frame);


    /// <summary>
    /// 视频显示
    /// </summary>
    /// <param name="frame"></param>
    public delegate void VedioPlayDelegateFrame(Mat frame);

    /// <summary>
    /// 读卡完成
    /// </summary>
    /// <param name="info"></param>
    public delegate void CaptureCardCompleteDelegate(SCardInfo info);

    /// <summary>
    /// 读卡完成，刘飞翔,2018-4-28
    /// </summary>
    /// <param name="info">读卡数据</param>
    /// <param name="nType">卡类型</param>
    public delegate void ReadCardCompleteDelegate(object info, int nType);

    /// <summary>
    /// 抓捕线程完成
    /// </summary>
    /// <param name="rgb24"></param>
    /// <param name="rgb8"></param>
    /// <param name="rect"></param>
    /// <param name="nIndex"></param>
    public delegate void CaptureFaceCompleteDelegate(Bitmap bm, byte[] rgb24,int w,int h, FacePointInfo[] facePoint,RECT[] rect,int nIndex);

    /// <summary>
    /// 比对完成事件
    /// </summary>
    /// <param name="detectRes"></param>
    public delegate void DetectCompleteDelegate(int detectRes,Bitmap bm,string strCardPhoto);

    /// <summary>
    /// 状态转变事件
    /// </summary>
    /// <param name="oldStatue"></param>
    /// <param name="newStatue"></param>
    public delegate void StatueChangeDelegate(ESTATUSTIP oldStatue, ESTATUSTIP newStatue);

    public delegate void OpenDoorDelegate(string Type);
    public delegate void ChangeConfigDelegate(MsgDownConfig mdc);
    public delegate void RestratDelegate(int nInfo);
    //public delegate void UploadCardInfoDelegate(string cardNum);

    /// <summary>
    /// 是否需要画线
    /// </summary>
    /// <param name="rc"></param>
    /// <param name="bNeed"></param>
    public delegate void DrawLineDelegate(RECT rc, bool bNeed);

    /// <summary>
    /// 连接客户端
    /// </summary>
    /// <param name="bConn"></param>
    public delegate void ConnectManagerDelegate(bool bConn);

    /// <summary>
    /// 无卡搜索
    /// </summary>
    /// <param name="idNum"></param>
    public delegate void NoCardSearchDelegate(string idNum);
}
