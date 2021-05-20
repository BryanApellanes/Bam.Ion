using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    [RegisteredFormFieldMember("etype")]
    public class ETypeFormFieldMember : IonFormFieldMember
    {
        public ETypeFormFieldMember()
        {
            this.FullName = "element type";
            this.Description = @"The etype member specifies the mandatory data type of each element in a form field’s value array. The name ""etype"" is short for ""element type"".";
        }

        public override bool IsValid()
        {
            return base.IsValid();
        }

        public override bool ParentFieldIsValid(IonValueObject ionValueObject)
        {
            IonMember typeMember = ionValueObject["type"];
            if("array".Equals(typeMember.Value) || "set".Equals(typeMember.Value))
            {
                return true;
            }
            return false;
        }

    }
}
