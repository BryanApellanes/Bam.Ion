using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Flow
{
    public interface IIonFlowState
    {
        IonFlowStatus Status { get; set; }
        IonForm Previous { get; set; }
        IonForm Current { get; set; }
        IonForm Next { get; set; }
    }
}
