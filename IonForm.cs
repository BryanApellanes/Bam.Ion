using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonForm : IonCollection
    {
        public IonForm() : base()
        {
            this.Value = new List<IonFormField>();
        }

        public IonForm(List<JToken> jTokens) : base(jTokens) 
        {
            this.Value = new List<IonFormField>();
        }

        public new List<IonFormField> Value
        {
            get;
            set;
        }

        public new static IonForm Read(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<JToken> jTokens = new List<JToken>();
            if (dictionary.ContainsKey("value"))
            {
                JArray arrayValue = dictionary["value"] as JArray;
                foreach (JToken token in arrayValue)
                {
                    jTokens.Add(token);
                }
            }
            IonForm ionForm = new IonForm(jTokens);

            foreach (string key in dictionary.Keys)
            {
                if (!"value".Equals(key))
                {
                    ionForm.AddElementMetaData(key, dictionary[key]);
                }
            }
            ionForm.SourceJson = json;
            return ionForm;
        }

        private static HashSet<string> formRelValues = new HashSet<string>(new[] { "form", "edit-form", "create-form", "query-form" });

        public static bool IsValid(string json)
        {
            return IsValid(json, out Dictionary<string, List<IonFormField>> formFieldsWithDuplicateNames);
        }

        public static bool IsValid(string json, out Dictionary<string, List<IonFormField>> formFieldsWithDuplicateNames)
        {
            /**
             * 6.1. Form Structure

Ion parsers MUST identify any JSON object as an Ion Form if the object matches the following conditions:

    Either:

        The JSON object is discovered to be an Ion Link as defined in Section 4 AND its meta member has an internal rel member that contains one of the octet sequences form, edit-form, create-form or query-form, OR:

        The JSON object is a member named form inside an Ion Form Field.

    The JSON object has a value array member with a value that is not null or empty.

    The JSON object’s value array contains one or more Ion Form Field objects.

    The JSON object’s value array does not contain elements that are not Ion Form Field objects.

             */
            bool isLink = Ion.IsLink(json);
            bool hasRelArray = false;
            bool hasValueArray = false;
            bool valueHasOnlyFormFields = true;
            bool valueHasFormFieldsWithUniqueNames = true;
            formFieldsWithDuplicateNames = new Dictionary<string, List<IonFormField>>();
            HashSet<IonFormField> formFields = new HashSet<IonFormField>();

            IonValueObject ionValue = IonValueObject.ReadValue(json);
            JArray relArray = ionValue["rel"].Value as JArray;
            if (relArray != null)
            {
                foreach (JToken jToken in relArray)
                {
                    if (formRelValues.Contains(jToken?.ToString()))
                    {
                        hasRelArray = true;
                        break;
                    }
                }
            }

            HashSet<string> duplicateNames = new HashSet<string>();

            if (ionValue["value"]?.Value is JArray valueArray)
            {
                if (valueArray != null && valueArray.Count > 0)
                {
                    hasValueArray = true;
                }
                foreach (JToken jToken in valueArray)
                {
                    if (!IonFormField.IsValid(jToken?.ToString(), out IonFormField formField))
                    {
                        valueHasOnlyFormFields = false;
                    }
                    List<IonFormField> existing = formFields.Where(ff => ff.Name.Equals(formField.Name)).ToList();
                    if (existing.Any())
                    {
                        duplicateNames.Add(formField.Name);

                        valueHasFormFieldsWithUniqueNames = false;                        
                    }
                    formFields.Add(formField);
                }
            }
            if(duplicateNames.Count > 0)
            {
                foreach(string duplicateName in duplicateNames)
                {
                    if (!formFieldsWithDuplicateNames.ContainsKey(duplicateName))
                    {
                        formFieldsWithDuplicateNames.Add(duplicateName, new List<IonFormField>());
                    }
                    formFieldsWithDuplicateNames[duplicateName].AddRange(formFields.Where(ff => ff.Name.Equals(duplicateName)));
                }                
            }
            return isLink && hasRelArray && hasValueArray && valueHasOnlyFormFields && valueHasFormFieldsWithUniqueNames;
        }
    }
}
