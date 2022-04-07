// <copyright file="IonForm.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bam.Ion
{
    /// <summary>
    /// Represents an ion form, see https://ionspec.org/#forms.
    /// </summary>
    public class IonForm : IonCollection
    {
        public static readonly HashSet<string> FormRelValues = new HashSet<string>(new[] { "form", "edit-form", "create-form", "query-form" });

        /// <summary>
        /// Initializes a new instance of the <see cref="IonForm"/> class.
        /// </summary>
        public IonForm()
            : base()
        {
            this.Value = new List<IonFormField>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonForm"/> class.
        /// </summary>
        /// <param name="jTokens"></param>
        public IonForm(List<JToken> jTokens)
            : base(jTokens) 
        {
            this.Value = base.Value.Select(ionValue => new IonFormField(ionValue)).ToList();
        }
        public List<string>? RelationTypes
        {
            get
            {
                string? rel = GetMemberValueToString("rel");
                if (!string.IsNullOrEmpty(rel))
                {
                    List<string>? relList = JsonConvert.DeserializeObject<List<string>?>(rel);
                    if (relList != null)
                    {
                        return relList;
                    }
                }
                return new List<string>();
            }
        }

        public string? Name
        {
            get
            {
                return GetMemberValueAs<string>("name");
            }
        }

        public string? Href
        {
            get
            {
                return GetMemberValueAs<string>("href");
            }
        }

        public string? Method
        {
            get
            {
                return GetMemberValueAs<string>("method");
            }
        }

        /// <summary>
        /// Gets the form fields contained by this form.
        /// </summary>
        public List<IonFormField> Fields
        {
            get
            {
                return Value;
            }
        }

        /// <summary>
        /// Gets or sets the form fields contained by this form.
        /// </summary>
        public new List<IonFormField> Value
        {
            get;
            set;
        }

        public FormSubmissionObject GetFormSubmissionObject()
        {
            return new FormSubmissionObject(this, Fields.Select(formField => formField.GetFormSubmissionMember()).ToList());
        }

        /// <summary>
        /// Reads the specified json string as an IonForm.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>IonForm.</returns>
        public static IonForm ReadForm(string json)
        {
            Dictionary<string, object>? dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (dictionary == null)
            {
                throw new ArgumentException($"failed to deserialize json: {json}");
            }

            List<JToken> jTokens = new List<JToken>();
            if (dictionary.ContainsKey("value"))
            {
                JArray? arrayValue = dictionary["value"] as JArray;
                if (arrayValue != null)
                {
                    foreach (JToken token in arrayValue)
                    {
                        jTokens.Add(token);
                    }
                }
            }

            IonForm ionForm = new IonForm(jTokens);

            foreach (string key in dictionary.Keys)
            {
                if (!"value".Equals(key))
                {
                    if (RegisteredMembers.Contains(key))
                    {
                        ionForm.AddMember(key, dictionary[key]);
                    }
                    else
                    {
                        ionForm.AddElementMetaData(key, dictionary[key]);
                    }
                }
            }

            ionForm.SourceJson = json;
            return ionForm;
        }

        public override string ToJson()
        {
            if (!string.IsNullOrEmpty(this.SourceJson))
            {
                return this.SourceJson;
            }

            return base.ToJson();
        }

        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            if (!string.IsNullOrEmpty(this.SourceJson))
            {
                Dictionary<string, object>? keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(this.SourceJson);
                if (keyValuePairs != null)
                {
                    return keyValuePairs.ToJson(pretty, nullValueHandling);
                }
            }

            return base.ToJson(pretty, nullValueHandling);
        }

        /// <summary>
        /// Determines if the specified form value is valid.
        /// </summary>
        /// <param name="formValueToCheck">The form value.</param>
        /// <returns>bool.</returns>
        public static bool IsValid(object formValueToCheck)
        {
            return IsValid(formValueToCheck, out _);
        }

        /// <summary>
        /// Determines if the specified form value is valid.
        /// </summary>
        /// <param name="formValueToCheck">The form value.</param>
        /// <param name="ionForm">The value as an IonObject.</param>
        /// <returns>bool.</returns>
        public static bool IsValid(object formValueToCheck, out IonForm? ionForm)
        {
            ionForm = null;
            if (formValueToCheck == null)
            {
                return false;
            }

            string json = formValueToCheck.ToJson();
            bool isValid = Validate(json).Success;
            
            if (isValid)
            {
                ionForm = ReadForm(json);
            }

            return isValid;
        }
        public static bool IsValid(IonValueObject ionValueObject)
        {
            return IsValid(ionValueObject, out _);
        }

        public static bool IsValid(IonValueObject ionValueObject, out IonForm? ionForm)
        {
            ionForm = null;
            if (ionValueObject == null)
            {
                return false;
            }

            bool isValid = Validate(ionValueObject).Success;

            if (isValid)
            {
                ionForm = ionValueObject.AsForm();
            }

            return isValid;
        }

        public static bool IsValid(IonMember ionMember)
        {
            return Validate(ionMember).Success;
        }

        /// <summary>
        /// Returns an `IonFormValidationResult` that indicates whether the specified member is a valid form.
        /// </summary>
        /// <param name="ionMember">The ion membmer.</param>
        /// <returns>`IonFormValidationResult`.</returns>
        public static IonFormValidationResult Validate(IonMember ionMember)
        {
            IonFormValidationResult ionFormValidationResult = new IonFormValidationResult();
            if (ionMember == null)
            {
                return ionFormValidationResult;
            }

            if (ionMember.Value == null)
            {
                return ionFormValidationResult;
            }

            return Validate(ionMember.ValueObject());
        }

        /// <summary>
        /// Returns an `IonFormValidationResult` that indicates whether the specified json string is a valid form.
        /// </summary>
        /// <param name="json">The json string</param>
        /// <returns>`IonFormValidationResults`.</returns>
        public static IonFormValidationResult Validate(string json)
        {
            IonValueObject ionValue = ReadObject(json);
            return Validate(ionValue);
        }

        /// <summary>
        /// Returns an `IonFormValidationResult` that indicates whether the specified json string is a valid form.
        /// </summary>
        /// <param name="json">The json string</param>
        /// <returns>`IonFormValidationResults`.</returns>
        internal static IonFormValidationResult Validate(IonValueObject ionValue)
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
            return new IonFormValidationResult(ionValue);
        }
    }
}
