using System;
using System.Collections.Generic;
using System.Text;
using MccDaq;
using ErrorDefs;

namespace Counters
{
    public class clsCounters
    {
        public const int CTR8254 = 1;
        public const int CTR9513 = 2;
        public const int CTR8536 = 3;
        public const int CTR7266 = 4;
        public const int CTREVENT = 5;
        public const int CTRSCAN = 6;
        public const int CTRTMR = 7;
        public const int CTRQUAD = 8;
        public const int CTRPULSE = 9;

        public int FindCountersOfType(MccDaq.MccBoard DaqBoard, int CounterType, out int DefaultCtr)
        {
            int NumCounters;
            int ThisType, CounterNum, CtrsFound;
            MccDaq.ErrorInfo ULStat;

            // check supported features by trial 
            // and error with error handling disabled
            ULStat = MccDaq.MccService.ErrHandling
                (MccDaq.ErrorReporting.DontPrint, MccDaq.ErrorHandling.DontStop);

            DefaultCtr = -1;
            CtrsFound = 0;
            ULStat = DaqBoard.BoardConfig.GetCiNumDevs(out NumCounters);
            if (!(ULStat.Value == MccDaq.ErrorInfo.ErrorCode.NoErrors))
            {
                clsErrorDefs.DisplayError(ULStat);
                return CtrsFound;
            }
            for (int CtrDev = 0; CtrDev < NumCounters; ++CtrDev)
            {
                ULStat = DaqBoard.CtrConfig.GetCtrType(CtrDev, out ThisType);
                if (ThisType == CounterType)
                {
                    ULStat = DaqBoard.CtrConfig.GetCtrNum(CtrDev, out CounterNum);
                    CtrsFound = CtrsFound + 1;
                    if (DefaultCtr == -1)
                    {
                        DefaultCtr = CounterNum;
                    }
                }
            }
            ULStat = MccDaq.MccService.ErrHandling
                (clsErrorDefs.ReportError, clsErrorDefs.HandleError);
            return CtrsFound;
        }   
        
    }
}
