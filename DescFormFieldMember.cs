using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    [RegisteredFormFieldMember("desc")]
    public class DescFormFieldMember : IonFormFieldMember
    {
        public DescFormFieldMember()
        {
            this.Optional = true;
            this.FullName = "description";
        }

        public DescFormFieldMember(string value) : this()
        {
            this.Value = value;
        }
    }
}
