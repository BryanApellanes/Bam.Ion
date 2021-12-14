using System;

namespace Bam.Net.Ion
{
    /// <summary>
    /// An attribute used to register custom form field members.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisteredFormFieldMemberAttribute: Attribute
    {
        public RegisteredFormFieldMemberAttribute(string memberName)
        {
            this.MemberName = memberName;
        }

        /// <summary>
        /// Gets or sets the member name.
        /// </summary>
        public string MemberName { get; set; }
    }
}
