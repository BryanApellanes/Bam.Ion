// <copyright file="IonLink.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bam.Ion
{
    /// <summary>
    /// Represents an Ion link.
    /// </summary>
    public class IonLink : IonValueObject, IIonLink, IJsonable
    {
        WebLink webLink;

        /// <summary>
        /// Initializes a new instance of the <see cref="IonLink"/> class.
        /// </summary>
        public IonLink() 
        {
            this.webLink = new WebLink();
        }

        public IonLink(IonValueObject ionValueObject): base(ionValueObject)
        {
            this.webLink = new WebLink();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonLink"/> class.
        /// </summary>
        /// <param name="relationType">The relation type.</param>
        /// <param name="target">The target</param>
        public IonLink(string relationType, Iri target)
        {
            this.webLink = new WebLink
            {
                RelationType = relationType,
                Target = target,
            };
        }

        [JsonProperty("method")]
        public string? Method
        {
            get
            {
                return GetMemberValueToString("method");
            }
        }

        /// <summary>
        /// Gets or sets the href.
        /// </summary>
        [JsonProperty("href")]
        public Iri? Href
        {
            get
            {
                string? href = GetMemberValueToString("href");
                if (!string.IsNullOrEmpty(href))
                {
                    return new Iri(href);
                }
                else if(this.webLink?.Target != null)
                {
                    return this.webLink.Target;
                }
                return null;
            }
            set => SetValue("href", value);
        }

        /// <summary>
        /// Returns the json string representation of the current `IonLink`.
        /// </summary>
        /// <returns>json string.</returns>
        public override string ToJson()
        {
            return this.ToJson(false);
        }

        /// <summary>
        /// Returns the json string representation of the current `IonLink`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <returns>json string.</returns>
        public string ToJson(bool pretty)
        {
            Dictionary<string, object> toSerialize = new Dictionary<string, object>();

            foreach (string key in this.SupportingMembers.Keys)
            {
                toSerialize.Add(key, this.SupportingMembers[key]);
            }

            toSerialize.Add(this.webLink.RelationType, new { href = this.Href });

            return toSerialize.ToJson(pretty);
        }

        /// <summary>
        /// Read the specified json string as an `IonLink`.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <param name="nameKey">The member name to retrieve the name from.</param>
        /// <param name="relationTypeKey">The member name to retrieve the relation type from.</param>
        /// <returns>`IonLink`.</returns>
        public static IonLink Read(string json, string nameKey, string relationTypeKey)
        {
            Dictionary<string, object>? parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            IonLink ionLInk = new IonLink(parsed[relationTypeKey].ToString(), parsed["href"].ToString());
            ionLInk.AddSupportingMember("name", parsed[nameKey].ToString());
            return ionLInk;
        }

        /// <summary>
        /// Returns a value indicating if the specified json string is a valid `IonLink`.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>`bool`.</returns>
        public static bool IsValid(string json)
        {
            return IsValid(json, out IonLink ignore);
        }

        /// <summary>
        /// Returns a value indicating if the specified json string is a valid `IonLink`.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <param name="ionLink">The parsed `IonLink`.</param>
        /// <returns>`bool`.</returns>
        public static bool IsValid(string json, out IonLink? ionLink)
        {
            ionLink = new IonLink();
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            Dictionary<string, object>? keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if(keyValuePairs != null)
            {
                if (keyValuePairs.ContainsKey("href"))
                {
                    string url = (string)keyValuePairs["href"];
                    if (Iri.IsIri(url, out Iri? iri))
                    {
                        if (iri != null)
                        {
                            ionLink.Href = iri;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool IsValid(IonValueObject ionValueObject)
        {
            return IsValid(ionValueObject, out _);
        }

        public static bool IsValid(IonValueObject ionValueObject, out IonLink ionLink)
        {
            ionLink = new IonLink();
            if (ionValueObject == null)
            {
                return false;
            }

            if (ionValueObject.HasMember("href", out IonMember ionMember))
            {
                string? url = ionMember.ValueAs<string>();
                if(Iri.IsIri(url, out Iri? iri))
                {
                    ionLink.Href = iri;
                    return true;
                }
            }

            return false;
        }
    }
}
