using Bam.Net.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Flow.Data
{    
    public class FlowInstance : AuditRepoData
    {
        public virtual List<FlowState> FlowStateSequence { get; set; }
    }
}
