using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class RegisteredFormFieldMember
    {
        public static HashSet<string> Names
        {
            get => IonFormFieldMember.RegisteredNames;
        }
    }
}
