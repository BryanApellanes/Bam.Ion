using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonFormMemberValidationResult : IonFormValidationResult
    {
        public IonFormMemberValidationResult(IonMember ionMember)
        {
            this.ValidatedMember = ionMember;
        }

        public IonMember ValidatedMember { get; private set; }
        public override bool Success
        {
            get
            {
                return (IsLink || ValidatedMember.IsMemberNamed("form") )&& HasRelArray && HasValueArray && HasOnlyFormFields && FormFieldsHaveUniqueNames;
            }
        }
    }
}
