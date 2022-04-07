// <copyright file="IonFormField.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bam.Ion
{
    /// <summary>
    /// Represents an ion form field, see https://ionspec.org/#form-fields.
    /// </summary>
    public class IonFormField : IonValueObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IonFormField"/> class.
        /// </summary>
        public IonFormField()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonFormField"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public IonFormField(List<IonMember> members)
            : base(members)
        {
            this.SetEtype();
        }

        public IonFormField(string json): this(IonMember.ListFromJson(json).ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonFormField"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public IonFormField(params IonMember[] members)
            : base(members)
        {
            this.SetEtype();
        }

        public IonFormField(IonValueObject ionValueObject) : this(ionValueObject.Members)
        {
            this.Value = ionValueObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonFormField"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="members">The members.</param>
        public IonFormField(string name, params IonMember[] members)
            : this(members)
        {
            this.Name = name;
        }

        /// <summary>
        /// All registered form field members.
        /// </summary>
        public static new HashSet<string> RegisteredMembers
        {
            get => IonFormFieldMember.RegisteredNames;
        }

        public override IonForm? AsForm()
        {
            if (IsForm(out IonForm form))
            {
                return form;
            }
            return null;
        }

        public override bool IsForm()
        {
            return IsForm(out _);
        }

        public override bool IsForm(out IonForm? form)
        {
            form = null;
            if ("object".Equals(this.TypeString))
            {
                return this["form"].IsForm(out form);
            }
            return false;
        }

        public IonMember GetFormSubmissionMember()
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentNullException($"{nameof(IonFormField)}.{nameof(Name)}");
            }
            /*
             * When a form is submitted to a linked resource location, the form’s data will be serialized to a JSON object named the Form Submission Object according to the following rules:

Each form field with a value member will be added to the Form Submission Object as a member with the same name having the same value.

If a form field has an object type and a form member, that form field’s value member will first be serialized to a JSON object according to these rules based on the field’s form member. The resulting object will be added to the Form Submission Object as a member having the same name as the field name.

If the form is transmitted to the href linked resource location via a communication protocol that supports content type identification (such as HTTP), the content type MUST be identified as either application/json or application/ion+json.
             */
            if ("object".Equals(GetMemberValueAs<string>("type")) &&
                HasMember("form", out IonMember formMember))
            {
                IonForm form = formMember.AsForm();
                Dictionary<string, object> members = new Dictionary<string, object>();
                foreach (IonFormField formField in form.Fields)
                {
                    IonMember ionMember = formField.GetFormSubmissionMember();
                    members.Add(ionMember.Name, ionMember.Value);
                }
                return new IonMember(Name, members) { Parent = this };
            }

            return new IonMember(Name, GetMemberValue("value")) { Parent = this };
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name
        {
            get
            {
                return GetMemberValueAs<string>("name");
            }

            set
            {
                SetValue("name", value);
            }
        }

        public string? TypeString
        {
            get
            {
                return GetMemberValueAs<string>("type");
            }
            set
            {
                SetValue("type", value);
            }
        }

        public string? Label
        {
            get
            {
                return GetMemberValueAs<string>("label");
            }
            set
            {
                SetValue("label", value);
            }
        }

        public string? ValueString
        {
            get
            {
                return GetMemberValueAs<string>("value");
            }
            set
            {
                SetValue("value", value);
            }
        }

        public bool? Required
        {
            get
            {
                return GetMemberBooleanValue("required");
            }
            set
            {
                SetValue("required", value);
            }
        }

        public bool? Visible
        {
            get
            {
                return GetMemberBooleanValue("visible");
            }
            set
            {
                SetValue("visible", value);
            }
        }

        public bool? Mutable
        {
            get
            {
                return GetMemberBooleanValue("mutable");
            }
            set
            {
                SetValue("mutable", value);
            }
        }

        public bool? Secret
        {
            get
            {
                return GetMemberBooleanValue("secret");
            }
            set
            {
                SetValue("secret", value);
            }
        }

        /// <summary>
        /// Returns the `desc` member value.
        /// </summary>
        /// <returns>`string`.</returns>
        public string? Desc
        {
            get
            {
                return GetMemberValueToString("desc");
            }
            set
            {
                SetValue("desc", value);
            }
        }

        public bool? Enabled
        {
            get
            {
                return GetMemberBooleanValue("enabled");
            }

            set
            {
                SetValue("enabled", value);
            }
        }

        protected void SetEtype()
        {
            this["etype"] = this["etype"];
        }

        /// <summary>
        /// Gets or sets the member with the specified name.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <returns>`IonMember`.</returns>
        public override IonMember this[string memberName]
        {
            get
            {
                IonMember baseMember = base[memberName];
                if ("etype".Equals(memberName))
                {
                    if (baseMember.Value == null)
                    {
                        if (baseMember.Parent["eform"]?.Value != null)
                        {
                            baseMember.Value = "object";
                        }
                    }

                    if (!"object".Equals(baseMember.Value))
                    {
                        return null;
                    }
                }

                if ("enabled".Equals(memberName))
                {
                    if (baseMember.Value?.GetType() != typeof(bool))
                    {
                        return null;
                    }
                }

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


        /// <summary>
        /// Reads the specified json string as an `IonFormField`.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>`IonFormField`.</returns>
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

        /// <summary>
        /// Returns a value indicating if the specified json is a valid form field.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>`bool`.</returns>
        public static bool IsValid(string json)
        {
            return IsValid(json, out IonFormField ignore);
        }

        /// <summary>
        /// Returns a value indicating if the specified json string is a valid `IonFormField`.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <param name="formField">The parsed `IonFormField`.</param>
        /// <returns>`bool`.</returns>
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
