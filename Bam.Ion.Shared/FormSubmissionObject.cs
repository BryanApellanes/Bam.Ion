using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class FormSubmissionObject : IonValueObject
    {
        public FormSubmissionObject(IonForm form, List<IonMember> members) : base(members)
        { 
            this.SourceForm = form;
        }

        public IonForm SourceForm { get; private set; }
    }
}
