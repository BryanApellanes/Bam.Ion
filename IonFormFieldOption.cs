using Bam.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonFormFieldOption : IonValueObject
    {

        static HashSet<string> _formFieldOptionMembers;
        static object _formFieldOptionMembersLock = new object();

        public static HashSet<string> FormFieldOptionMembers
        {
            get
            {
                return _formFieldOptionMembersLock.DoubleCheckLock(ref _formFieldOptionMembers, () => new HashSet<string>(new[]
                {
                    "enabled",
                    "label",
                    "value"
                }));
            }
        }
    }
}
