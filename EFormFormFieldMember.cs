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
            if (Value is JObject jObject) 
            {
                string typeValue = (string)jObject["type"];
                if (string.IsNullOrEmpty(typeValue))
                {
                    return false;
                }
                if(!"array".Equals(typeValue) && !"set".Equals(typeValue))
                {
                    return false;
                }
                string etypeValue = (string)jObject["etype"];
                if(etypeValue != null && !"object".Equals(etypeValue))
                {
                    return false;
                }
                return true;
            }
            if(Value is Dictionary<string, object> dictionary)
            {
                string typeValue = (string)dictionary["type"];
                if (string.IsNullOrEmpty(typeValue))
                {
                    return false;
                }
                if (!"array".Equals(typeValue) && !"set".Equals(typeValue))
                {
                    return false;
                }
                string etypeValue = (string)dictionary["etype"];
                if (etypeValue != null && !"object".Equals(etypeValue))
                {
                    return false;
                }
                return true;
            }
            bool isForm = false;
            if (Value is IIonJsonable ionJsonable)
            {
                string ionJson = ionJsonable.ToIonJson();
                isForm = IonForm.IsValid(ionJson);
            }
            else if (Value is IJsonable jsonable)
            {
                string json = jsonable.ToJson();
                isForm = IonForm.IsValid(json);
            }
            else
            {
                string json = JsonConvert.SerializeObject(Value);
                isForm = IonForm.IsValid(json);
            }
            return isForm;
        }
    }
}
