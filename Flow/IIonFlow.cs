using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Flow
{
    public interface IIonFlow
    {
        string Name { get; }
        string Description { get; }
        IIonRenderer Renderer { get; set; }
        IIonFlowState Start();

        IIonFlowState Process(IIonFlowState flowState);

        IIonFlowState Next(IIonFlowState flowState);

        IIonFlowState Cancel(IIonFlowState flowState = null);

        IIonFlowState HandleError(IIonFlowState flowState);

        IIonFlowState Complete(IIonFlowState flowState);

        IIonRenderResult Render(IIonFlowState flowState);
    }
}
