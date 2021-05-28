using Bam.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonFormField : IonObject
    {
        public IonFormField() : base() { }
        public IonFormField(List<IonMember> members) : base(members) { }
        public IonFormField(params IonMember[] members) : base(members) { }

        public IonFormField(string name, params IonMember[] members) : this(members)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get
            {
                IonMember nameMember = this["name"];
                return nameMember.Value as string;
            }
            set
            {
                this["name"].Value = value;
            }
        }

        /// <summary>
        /// All registered form field members.
        /// </summary>
        public static HashSet<string> RegisteredMembers
        {
            get => IonFormFieldMember.RegisteredNames;
        }


        public string Desc()
        {
            return this["desc"]?.Value?.ToString();
        }

        public IonFormField Desc(string value)
        {
            this["desc"].Value = value;
            return this;
        }

        public object EForm()
        {
            return this["eform"].Value;
        }

        public bool Enabled()
        {
            return (bool)this["enabled"].Value?.ToString()?.IsAffirmative();
        } 

        public IonFormField Enabled(bool enabled)
        {
            this["enabled"].Value = enabled;
            return this;
        }

        public new IonFormFieldOption Value { get; set; }

        public override IonMember this[string memberName] 
        {
            get
            {
                IonMember baseMember = base[memberName];
                if (IonFormFieldMember.RegisteredNames.Contains(memberName))
                {
                    if (IonFormFieldMember.RegisteredFormFieldIsValid(memberName, baseMember) != true)
                    {
                        return null;
                    }
                }
                return baseMember;
            }
            set
            {
                base[memberName] = value;
            }
        }

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

            bool allFieldsAreFormFieldMembers = true;
            formField = null;
            Dictionary<string, object> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            bool hasNameMember = keyValuePairs.ContainsKey("name");
            if (hasNameMember)
            {
                foreach (string key in keyValuePairs.Keys)
                {
                    if (!RegisteredMembers.Contains(key))
                    {
                        allFieldsAreFormFieldMembers = false;
                    }
                }
            }

            if (hasNameMember && allFieldsAreFormFieldMembers)
            {
                formField = IonFormField.Read(json);
                return true;
            }

            return false;
        }
    }
}
