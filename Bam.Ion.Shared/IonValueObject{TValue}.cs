// <copyright file="IonObject.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bam.Ion
{
    /// <summary>
    /// Represents an IonObject whose value is of the specified generic type TValue.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class IonValueObject<TValue> : IonValueObject
    {
        private TValue? value;

        public static implicit operator IonValueObject<TValue>(TValue value)
        {
            return new IonValueObject<TValue> { Value = value };
        }

        public static implicit operator string(IonValueObject<TValue> value)
        {
            return value.ToJson();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonValueObject{TValue}"/> class.
        /// </summary>
        public IonValueObject() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonValueObject{TValue}"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public IonValueObject(List<IonMember> members) : base(members)
        {
            this.Value = this.ToInstance();
        }

        public IonValueObject(IonValueObject ionValueObject) : base(ionValueObject)
        {
            this.Value = this.ToInstance();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonValueObject{TValue}"/> class.
        /// </summary>
        /// <param name="json">A json string representing the members.</param>
        public IonValueObject(string json)
            : this(IonMember.ListFromJson(json).ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonValueObject{TValue}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public IonValueObject(TValue value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public new TValue? Value
        {
            get => this.value;
            set
            {
                this.value = value;
                base.Value = value;
                if ((this.Members == null || this.Members?.Count == 0) &&
                    this.value != null)
                {
                    Type typeOfValue = this.value.GetType();
                    if (!IonTypes.All.Contains(typeOfValue))
                    {
                        this.Members = IonMember.ListFromJson(this.value.ToJson()).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Returns a json string representing the current `IonObject`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <param name="nullValueHandling">Specifies null handling options for the JsonSerializer.</param>
        /// <returns></returns>
        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if (this.Value != null)
            {
                data.Add("value", this.Value);
            }

            foreach (IonMember member in this.Members)
            {
                data.Add(member.Name, member.Value);
            }

            foreach (string key in this.SupportingMembers.Keys)
            {
                data.Add(key, this.SupportingMembers[key]);
            }

            return data.ToJson(pretty, nullValueHandling);
        }

        /// <summary>
        /// Returns the current `IonObject` as an instance of the specified generic type `TValue`.
        /// </summary>
        /// <returns>`TValue`.</returns>
        public TValue ToInstance()
        {
            ConstructorInfo? ctor = typeof(TValue).GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new InvalidOperationException($"The specified type ({typeof(TValue).AssemblyQualifiedName}) does not have a parameterless constructor.");
            }

            TValue instance = (TValue)ctor.Invoke(null);
            foreach (IonMember ionMember in this)
            {
                ionMember.SetProperty(instance);
            }

            return instance;
        }

        /// <summary>
        /// Returns a value uniquely identifying this instance at runtime.
        /// </summary>
        /// <returns>`int`.</returns>
        public override int GetHashCode()
        {
            return this.ToJson(false).GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating if the current instance is equivalent to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>`bool`.</returns>
        public override bool Equals(object value)
        {
            if (value == null && this.Value == null)
            {
                return true;
            }

            if (value != null && this.Value == null)
            {
                return false;
            }

            if (value != null && this.Value != null)
            {
                if (value is string json && json.TryFromJson<TValue>(out TValue otherValue))
                {
                    return this.Value.ToJson().Equals(otherValue.ToJson());
                }
                else if (value is IonValueObject<TValue> otherIonObject)
                {
                    return this.Value.Equals(otherIonObject.Value);
                }

                return this.Value.Equals(value);
            }

            return false;
        }
    }
}
