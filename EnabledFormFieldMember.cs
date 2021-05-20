using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    [RegisteredFormFieldMember("enabled")]
    public class EnabledFormFieldMember : IonFormFieldMember
    {
        public EnabledFormFieldMember()
        {
            this.Name = "enabled";
            this.Value = true;
            this.Optional = true;

            this.Description = @"The enabled member indicates whether or not the field value may be modified or submitted to a linked resource location.";
        }

        public EnabledFormFieldMember(bool enabled) : this()
        {
            this.Value = enabled;
        }
    }
}
