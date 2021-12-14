using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Flow
{
    public enum IonFlowStatus
    {
        Uninitialized,
        Initialized,
        Started,
        StepDataRequired,
        FlowDataRequired,
        StepComplete,
        FlowComplete,
        FlowCanceled,
        StepError,
        FlowError,
        Fatal
    }
}
