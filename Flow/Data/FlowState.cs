using Bam.Net.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Flow.Data
{
    public class FlowState : AuditRepoData
    {
        public int SequenceNumber { get; set; }
    }
}
