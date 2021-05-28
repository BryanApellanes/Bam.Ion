using Bam.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    [RegisteredFormFieldMember("eform")]
    public class EFormFormFieldMember : IonFormFieldMember
    {
        public EFormFormFieldMember()
        {
            this.Name = "eform";
            this.Optional = true;
            this.Description = @"The eform member value is either a Form object or a Link to a Form object that reflects the required object structure of each element in the field’s value array. The name ""eform"" is short for ""element form"".";
            this.FullName = "element form";
        }

        public EFormFormFieldMember(object value) : this()
        {
            this.Value = value;
        }

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
