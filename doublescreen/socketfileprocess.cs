using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace doublescreen
{
    
    class socketfileprocess
    {
        
        public  socketfileprocess(UInt32 filetype,string mystring)
        {
            Debug.Assert(filetype > doubleconst.flieTypeJson);
            string ss = string.Empty;
            
            if (filetype == doubleconst.flieTypeIdimage) //flieTypeIdimage)
            {
                ss = "old image+str";
            }
            else if(filetype == doubleconst.flieTypeCurimage)
            {
                ss = "Cur image+str";
            }
            else if (filetype == doubleconst.flieTypeJson)
            {
                ss = "Json file+str";
            }  
        }
        public socketfileprocess(UInt32 filetype, byte[] mybyte)
        {
            Debug.Assert(filetype > doubleconst.flieTypeJson);
            string ss = string.Empty;

            if (filetype == doubleconst.flieTypeIdimage) //flieTypeIdimage)
            {
                ss = "old image+byte";
            }
            else if (filetype == doubleconst.flieTypeCurimage)
            {
                ss = "Cur image+byte";
            }
            else if (filetype == doubleconst.flieTypeJson)
            {
                ss = "Json file+byte";
            }
        }
    }
}
