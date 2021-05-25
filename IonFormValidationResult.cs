using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonFormValidationResult
    {
        public object SourceJson { get; set; }

        public bool IsLink { get; set; }

        public bool HasRelArray { get; set; }

        public bool HasValueArray { get; set; }

        public bool HasOnlyFormFields { get; set; }

        public bool FormFieldsHaveUniqueNames { get; set; }

        public Dictionary<string, List<IonFormField>> FormFieldsWithDuplicateNames { get; set; }

        public virtual bool Success
        {
            get { return IsLink && HasRelArray && HasValueArray && HasOnlyFormFields && FormFieldsHaveUniqueNames; }
        }
    }
}
