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

        public static bool IsValid(object valueToCheck)
        {
            return IsValid(valueToCheck, out IonValueObject ignore);
        }

        public static bool IsValid(object valueToCheck, out IonValueObject ionValueObject)
        {
            string json = valueToCheck?.ToJson();
            bool isValid =  IsValid(json);
            ionValueObject = null;
            if (isValid)
            {
                ionValueObject = IonValueObject.ReadValue(json);
            }
            return isValid;
        }

        public static bool IsValid(string json)
        {
            return IsValid(json, out Dictionary<string, List<IonFormField>> formFieldsWithDuplicateNames);
        }

        public static bool IsValid(string json, out IonFormValidationResult ionFormValidationResult)
        {
            return IsValid(json, out Dictionary<string, List<IonFormField>> ignore, out ionFormValidationResult);
        }

        public static bool IsValid(string json, out Dictionary<string, List<IonFormField>> formFieldsWithDuplicateNames)
        {
            return IsValid(json, out formFieldsWithDuplicateNames, out IonFormValidationResult ignore);
        }

        public static bool IsValid(string json, out Dictionary<string, List<IonFormField>> formFieldsWithDuplicateNames, out IonFormValidationResult ionFormValidationResult)
        {
            /**
             * 6.1. Form Structure

Ion parsers MUST identify any JSON object as an Ion Form if the object matches the following conditions:

    Either:

        The JSON object is discovered to be an Ion Link as defined in Section 4 AND its meta member has 
            an internal rel member that contains one of the octet sequences form, edit-form, create-form or query-form, OR:

        The JSON object is a member named form inside an Ion Form Field.

    The JSON object has a value array member with a value that is not null or empty.

    The JSON object’s value array contains one or more Ion Form Field objects.

    The JSON object’s value array does not contain elements that are not Ion Form Field objects.

             */
            bool isLink = Ion.IsLink(json);
            bool valueHasOnlyFormFields = true;
            bool valueHasFormFieldsWithUniqueNames = true;
            
            HashSet<IonFormField> formFields = new HashSet<IonFormField>();

            IonValueObject ionValue = IonValueObject.ReadValue(json);
            bool hasRelArray = HasValidRelArray(ionValue);
            bool hasValueArray = HasValueArray(ionValue, out JArray jArrayValue);

            formFieldsWithDuplicateNames = new Dictionary<string, List<IonFormField>>();
            HashSet<string> duplicateNames = new HashSet<string>();
            foreach (JToken jToken in jArrayValue)
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
            if (duplicateNames.Count > 0)
            {
                foreach (string duplicateName in duplicateNames)
                {
                    if (!formFieldsWithDuplicateNames.ContainsKey(duplicateName))
                    {
                        formFieldsWithDuplicateNames.Add(duplicateName, new List<IonFormField>());
                    }
                    formFieldsWithDuplicateNames[duplicateName].AddRange(formFields.Where(ff => ff.Name.Equals(duplicateName)));
                }
            }
            ionFormValidationResult = new IonFormValidationResult
            {
                SourceJson = json,
                IsLInk = isLink,
                HasRelArray = hasRelArray,
                HasValueArray = hasValueArray,
                HasOnlyFormFields = valueHasOnlyFormFields,
                FormFieldsHaveUniqueNames = valueHasFormFieldsWithUniqueNames,
                FormFieldsWithDuplicateNames = formFieldsWithDuplicateNames
            };
            // HASRELARRAY needs to be evaluated as the EITHER statement above
            // (hasRelarray || IsChildOfFormField named "form")
            // the problem is we don't have the parent from this context
            // THIS WILL NEED TO BE REFACTORED SOMEHOW, ADDED IonMember.Parent to help but 
            // this needs more analysis
            return isLink && hasRelArray && hasValueArray && valueHasOnlyFormFields && valueHasFormFieldsWithUniqueNames;
        }

        private static bool HasValueArray(IonValueObject ionValue, out JArray arrayValue)
        {
            arrayValue = new JArray();
            if (ionValue["value"]?.Value is JArray valueArray)
            {
                arrayValue = valueArray;
                if (valueArray != null && valueArray.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasValidRelArray(IonValueObject ionValue)
        {
            JArray relArray = ionValue["rel"].Value as JArray;
            if (relArray != null)
            {
                foreach (JToken jToken in relArray)
                {
                    if (formRelValues.Contains(jToken?.ToString()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
