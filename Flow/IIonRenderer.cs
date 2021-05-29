using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Flow
{
    public interface IIonRenderer
    {
        IIonRenderResult Render(IIonFlowState flowState);
    }
}
