using Newtonsoft.Json.Linq;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents an `IonFormFieldMember` with the member name of `eform`.
    /// </summary>
    [RegisteredFormFieldMember("eform")]
    public class EFormFormFieldMember : IonFormFieldMember
    {
        /// <summary>
        /// Construct an instance of the `EFormFormFieldMember` class.
        /// </summary>
        public EFormFormFieldMember()
        {
            this.Name = "eform";
            this.Optional = true;
            this.Description = @"The eform member value is either a Form object or a Link to a Form object that reflects the required object structure of each element in the field’s value array. The name ""eform"" is short for ""element form"".";
            this.FullName = "element form";
        }

        /// <summary>
        /// Construct an instance of the `EFormFormFieldMember` class with the specified value.
        /// </summary>
        /// <param name="value"></param>
        public EFormFormFieldMember(object value) : this()
        {
            this.Value = value;
        }

        /// <summary>
        /// Get an `EFormFormFieldMember` with the specified value.  Performs validation of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>EFormFormFieldMember</returns>
        public static EFormFormFieldMember FromValue(string value)
        {
            EFormFormFieldMember result = null;
            if (value.IsJson(out JObject jObject))
            {
                result = new EFormFormFieldMember(jObject);
                if (!result.IsValid())
                {
                    return null;
                }
            }
            return result;
        }

        /// <summary>
        /// Determines if the `Value` property is valid.
        /// </summary>
        /// <returns>`true` if the value is valid,</returns>
        public override bool IsValid()
        {
            if(Value == null)
            {
                return false;
            }
            bool hasRequiredMembers = false;
            if (Value is JObject jObject)
            {
                hasRequiredMembers = HasRequiredMembers(jObject);
            }
            else if (JObjectValue != null)
            {
                hasRequiredMembers = HasRequiredMembers(JObjectValue);
            }
            return hasRequiredMembers && IsForm();
        }

        /// <summary>
        /// Determines if the specified `JObject` has required members `type` equal to "array" or "set" and `etype` equal to "object". 
        /// </summary>
        /// <param name="jObject">The JObject</param>
        /// <returns>`true` if required members exist.</returns>
        protected bool HasRequiredMembers(JObject jObject)
        {
            string typeValue = (string)jObject["type"];
            if (string.IsNullOrEmpty(typeValue))
            {
                return false;
            }
            if (!"array".Equals(typeValue) && !"set".Equals(typeValue))
            {
                return false;
            }
            string etypeValue = (string)jObject["etype"];
            if (etypeValue != null && !"object".Equals(etypeValue))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if the Value is a form.
        /// </summary>
        /// <returns>`true` if the Value is a form.</returns>
        protected bool IsForm()
        {
            bool isForm;
            if (Value is IIonJsonable ionJsonable)
            {
                string ionJson = ionJsonable.ToIonJson();
                isForm = IonForm.Validate(ionJson).Success;
            }
            else if (Value is IJsonable jsonable)
            {
                string json = jsonable.ToJson();
                isForm = IonForm.Validate(json).Success;
            }
            else
            {
                isForm = IonForm.Validate((IonMember)this).Success;
            }
            return isForm;
        }
    }
}
