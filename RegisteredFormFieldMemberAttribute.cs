using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisteredFormFieldMemberAttribute: Attribute
    {
        public RegisteredFormFieldMemberAttribute(string memberName)
        {
            this.MemberName = memberName;
        }

        public string MemberName { get; set; }
    }
}
