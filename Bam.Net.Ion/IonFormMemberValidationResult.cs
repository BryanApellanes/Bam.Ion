using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents the result of form member validation.
    /// </summary>
    public class IonFormMemberValidationResult : IonFormValidationResult
    {
        /// <summary>
        /// Create a new instance of `IonFormMemberValidationResult`.
        /// </summary>
        /// <param name="ionMember"></param>
        public IonFormMemberValidationResult(IonMember ionMember)
        {
            this.ValidatedMember = ionMember;
        }

        /// <summary>
        /// Gets the `IonMember` that is the target of validation.
        /// </summary>
        public IonMember ValidatedMember { get; private set; }

        /// <summary>
        /// Gets a value indicating if validation succeeded.
        /// </summary>
        public override bool Success
        {
            get
            {
                return (IsLink || ValidatedMember.IsMemberNamed("form")) && HasRelArray && HasValueArray && HasOnlyFormFields && FormFieldsHaveUniqueNames;
            }
        }
    }
}
