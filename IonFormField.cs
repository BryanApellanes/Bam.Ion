using Bam.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonFormField : IonValueObject
    {
        public IonFormField() : base() { }
        public IonFormField(List<IonMember> members) : base(members) { }

        public string Name
        {
            get
            {
                return (string)this["name"].Value;
            }
            set
            {
                this["name"].Value = value;
            }
        }

        public new IonFormFieldOption Value { get; set; }

        public static IonFormField Read(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<IonMember> members = new List<IonMember>();
            foreach (System.Collections.Generic.KeyValuePair<string, object> keyValuePair in dictionary)
            {
                members.Add(keyValuePair);
            }
            return new IonFormField(members) { SourceJson = json };
        }

        static HashSet<string> _formFieldMembers;
        static object _formFieldMembersLock = new object();

        public static HashSet<string> FormFieldMembers
        {
            get
            {
                return _formFieldMembersLock.DoubleCheckLock(ref _formFieldMembers, () => new HashSet<string>(new[]
                {
                    "desc",
                    "eform",
                    "enabled",
                    "etype",
                    "form",
                    "label",
                    "max",
                    "maxLength",
                    "maxsize",
                    "min",
                    "minLength",
                    "minsize",
                    "mutable",
                    "name",
                    "options",
                    "pattern",
                    "placeHolder",
                    "required",
                    "secret",
                    "type",
                    "value",
                    "visible",
                }));
            }
        }

        public static bool IsValid(string json)
        {
            return IsValid(json, out IonFormField ignore);
        }

        public static bool IsValid(string json, out IonFormField formField)
        {
            /**
             * 
An Ion Form Field MUST have a string member named name.

Each Ion Form Field within an Ion Form’s value array MUST have a unique name value compared to any other Form Field within the same array.
             * 
             */

            bool hasNameMember = false;
            bool allFieldsAreFormFieldMembers = true;
            formField = null;
            Dictionary<string, object> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            hasNameMember = keyValuePairs.ContainsKey("name");
            if (hasNameMember)
            {
                foreach (string key in keyValuePairs.Keys)
                {
                    if (!FormFieldMembers.Contains(key))
                    {
                        allFieldsAreFormFieldMembers = false;
                    }
                }
            }

            if(hasNameMember && allFieldsAreFormFieldMembers)
            {
                formField = IonFormField.Read(json);
                return true;
            }

            return false;
        }
    }
}
