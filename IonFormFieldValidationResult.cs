using Bam.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonFormFieldValidationResult
    {
        public bool ValueIsArray { get; set; }
        public bool ValueHasOnlyFormFields { get; private set; }
        public bool ValueHasFormFieldsWithUniqueNames { get; private set; }

        public Dictionary<string, List<IonFormField>> FormFieldsWithDuplicateNames { get; private set; }

        public static IonFormFieldValidationResult ValidateFormFields(IonMember ionMember)
        {
            if (ionMember == null)
            {
                throw new ArgumentNullException("ionMember");
            }

            if (ionMember.Value is null)
            {
                return new IonFormFieldValidationResult();
            }

            if (ionMember.Value is JArray jArrayValue)
            {
                return ValidateFormFields(jArrayValue);
            }

            if (ionMember.Value is string stringValue)
            {
                if (stringValue.IsJsonArray(out JArray jArray))
                {
                    return ValidateFormFields(jArray);
                }
            }

            string valueJson = ionMember.Value.ToJson();
            if(valueJson.IsJsonArray(out JArray jsonArray))
            {
                return ValidateFormFields(jsonArray);
            }

            return new IonFormFieldValidationResult { ValueIsArray = false };
        }

        public static IonFormFieldValidationResult ValidateFormFields(JArray jArrayValue)
        {
            bool valueHasOnlyFormFields = true;
            bool valueHasFormFieldsWithUniqueNames = true;
            Dictionary<string, List<IonFormField>> formFieldsWithDuplicateNames;
            HashSet<IonFormField> formFields = new HashSet<IonFormField>();
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

            return new IonFormFieldValidationResult
            {
                ValueIsArray = true,
                ValueHasOnlyFormFields = valueHasOnlyFormFields,
                ValueHasFormFieldsWithUniqueNames = valueHasFormFieldsWithUniqueNames,
                FormFieldsWithDuplicateNames = formFieldsWithDuplicateNames
            }; ;
        }

    }
}
