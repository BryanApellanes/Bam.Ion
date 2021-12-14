using System.Collections.Generic;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents an ion form field option, see // https://ionspec.org/#form-field-option-members.
    /// </summary>
    public class IonFormFieldOption : IonObject
    {

        static HashSet<string> _formFieldOptionMembers;
        static object _formFieldOptionMembersLock = new object();

        /// <summary>
        /// Gets the valid form field option members.
        /// </summary>
        public static HashSet<string> FormFieldOptionMembers
        {
            get
            {
                if(_formFieldOptionMembers == null)
                {
                    lock(_formFieldOptionMembersLock)
                    {
                        if(_formFieldOptionMembers == null)
                        {
                            _formFieldOptionMembers = new HashSet<string>(new[]
                            {
                                "enabled",
                                "label",
                                "value"
                            });
                        }
                    }
                }
                return _formFieldOptionMembers;
            }
        }
    }
}
