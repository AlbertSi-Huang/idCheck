using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doublescreen
{
    
    class fileLastName
    {
        private const UInt32 sMaxcnt = 10;   //debug 20
        private const UInt32 sPicPerPage = 8;
        private static UInt32 curPoint = 0; //never equal 0  最新的文件指针
        private static UInt32 showPoint = 1;   //  左右键按下时 最左的文件指针
        private bool hasLeftPic = false;
        private bool hasRighePic = false;
        private static UInt32 myCnt = 0;
        private static UInt32 myPages = 0;
        private static UInt32 curPages = 0;
        public void AddNew()
        {
            if (++curPoint > sMaxcnt)
            {
                curPoint = 1;
             //   showPoint = sPicPerPage;
            }
            myCnt++;
            // showPoint = curPoint - curPoint% sPicPerPage;
            showPoint = curPoint;

            //if (myCnt >= sPicPerPage)
            //{
            //    showPoint = sPicPerPage;
            //}
            //else
            //{
            //    showPoint = curPoint;
            //}
            if (myCnt > sMaxcnt)
            {
                myPages = sMaxcnt / sPicPerPage;
            }
            else
            {
                myPages = (myCnt-1) / sPicPerPage;
            }
            curPages = myPages;
            if (myPages!=0) hasLeftPic = true;
            firstLeftKey = true;
            hasRighePic = false;
        }
        static bool firstLeftKey = true;
        public void LeftKey()
        {

            
            curPages--;
            if (curPages != 1)
            {
                hasLeftPic = true;
            }
            else
            {
                hasLeftPic = false;
            }
            if(curPages != myPages)
            {
                hasRighePic = true;
            }
            else
            {
                hasRighePic = false;
            }
            if (myCnt <= sMaxcnt)
            {             
                    showPoint = curPages * sPicPerPage;
            }
            else
            {
                if (firstLeftKey)
                {
                    firstLeftKey = false;
                    if (showPoint >= 2 * sPicPerPage)
                    {
                        showPoint -= 2 * sPicPerPage;
                    }
                    else
                    {
                        showPoint = showPoint + sMaxcnt - 2 * sPicPerPage;
                    }
                }
                else
                {
                    if (showPoint >= sPicPerPage)
                    {
                        showPoint -=  sPicPerPage;
                    }
                    else
                    {
                        showPoint = showPoint + sMaxcnt -  sPicPerPage;
                    }
                }
               
               
            }
           //     (myPages - curPages)*;
           // i = (myPages - curPages) * sPicPerPage + curPoint % sPicPerPage;
            //if (curPoint >= i) showPoint = curPoint - i;
            //else
            //{
            //    showPoint = sMaxcnt - i + curPoint;
            //}
        }
        public void RightKey()
        {

            curPages++;
            if (curPages != myPages)
            {
                hasRighePic = true;
            }
            else
            {
                hasRighePic = false;
            }
            if (curPages != 0)
            {
                hasLeftPic = true;
            }
            else
            {
                hasLeftPic = false;
            }

            showPoint += sPicPerPage;
            if(showPoint> sMaxcnt)
            {
                showPoint = showPoint - sMaxcnt;
            }
        }
        public UInt32 GetShowPoint()
        {
            
             return showPoint;
           
        }
        public UInt32 GetCurPoint()
        {
            return curPoint;
        }
        public bool HasLeftPic()
        {
            return hasLeftPic;
        }
        public bool HasRightPic()
        {
            return hasRighePic;
        }
        public UInt32 GetmyCnt()
        {
            return myCnt;
        }
    }
}
