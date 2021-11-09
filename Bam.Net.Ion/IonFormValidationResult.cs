using System.Collections.Generic;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents the result of Ion form validation.
    /// </summary>
    public class IonFormValidationResult
    {
        /// <summary>
        /// Gets or sets the source json.
        /// </summary>
        public object SourceJson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the form is a link.
        /// </summary>
        public bool IsLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if a rel array is present.
        /// </summary>
        public bool HasRelArray { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if a value array is present.
        /// </summary>
        public bool HasValueArray { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if only form fields are present.
        /// </summary>
        public bool HasOnlyFormFields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if form fields have unique names.
        /// </summary>
        public bool FormFieldsHaveUniqueNames { get; set; }

        /// <summary>
        /// Gets or sets a dictionary containing form fields with duplicate names.
        /// </summary>
        public Dictionary<string, List<IonFormField>> FormFieldsWithDuplicateNames { get; set; }

        /// <summary>
        /// Gets a value indicating if validation succeeded.
        /// </summary>
        public virtual bool Success
        {
            get { return IsLink && HasRelArray && HasValueArray && HasOnlyFormFields && FormFieldsHaveUniqueNames; }
        }
    }
}
