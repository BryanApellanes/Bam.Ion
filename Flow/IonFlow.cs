using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Flow
{
    public abstract class IonFlow : IIonFlow
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public IIonRenderer Renderer { get; set; }

        public abstract IIonFlowState Cancel(IIonFlowState flowState = null);

        public abstract IIonFlowState Complete(IIonFlowState flowState);

        public abstract IIonFlowState HandleError(IIonFlowState flowState);

        public abstract IIonFlowState Next(IIonFlowState flowState);

        public abstract IIonFlowState Process(IIonFlowState flowState);

        public IIonRenderResult Render(IIonFlowState flowState)
        {
            return Renderer.Render(flowState);
        }

        public abstract IIonFlowState Start();
    }
}
