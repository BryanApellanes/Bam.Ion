﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents an Ion member whose value is of generic type `TValue`.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class IonMember<TValue> : IonMember
    {
        public static implicit operator TValue(IonMember<TValue> ionMember)
        {
            return ionMember.Value;
        }

        public static explicit operator IonMember<TValue>(TValue value)
        {
            return new IonMember<TValue>(value);
        }
        
        /// <summary>
        /// Create a new instance of `IonMember`.
        /// </summary>
        public IonMember() { }

        /// <summary>
        /// Create a new instance of `IonMember` with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public IonMember(TValue value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public new TValue Value { get; set; }

        /// <summary>
        /// Returns a value indicating if the current instance is equivalent to the specified value.
        /// </summary>
        /// <param name="obj">The value to compare to.</param>
        /// <returns>`bool`.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null && Value == null)
            {
                return true;
            }
            if (obj == null && Value != null)
            {
                return false;
            }
            if (obj != null && Value == null)
            {
                return false;
            }
            if(obj is IonMember ionMember)
            {
                return Value.Equals(ionMember.Value) && (bool)Name?.Equals(ionMember?.Name);
            }
            return Value.Equals(obj);
        }

        /// <summary>
        /// Get hash code uniquely identifying the current instance at runtime.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (Value == null)
            {
                return -1;
            }
            return Value.GetHashCode() + Name.GetHashCode();
        }
    }

    /// <summary>
    /// Represents an Ion member.
    /// </summary>
    public class IonMember 
    {
        /// <summary>
        /// Creates a new instance of `IonMember`.
        /// </summary>
        public IonMember()
        {
        }

        public static implicit operator System.Collections.Generic.KeyValuePair<string, object>(IonMember ionMember)
        {
            return new System.Collections.Generic.KeyValuePair<string, object>(ionMember.Name, ionMember.Value);
        }

        public static implicit operator IonMember(System.Collections.Generic.KeyValuePair<string, object> keyValuePair)
        {
            return new IonMember { Name = keyValuePair.Key, Value = keyValuePair.Value, SourceValue = keyValuePair.Value };
        }

        public static implicit operator string(IonMember ionMember)
        {
            return ionMember?.ToJson() ?? "null";
        }

        public static implicit operator IonMember(string value)
        {
            if(value.TryFromJson(out IonMember result))
            {
                result.SourceValue = result.Value;
                return result;
            }
            return new IonMember { Name = "value", Value = value, SourceValue = value };
        }

        /// <summary>
        /// Create a new instance of `IonMember` with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public IonMember(object value)
        {
            this.Name = "value";
            this.Value = value;
            this.SourceValue = value;
        }

        /// <summary>
        /// Create a new instance of `IonMember` with the specified name and value.
        /// </summary>
        /// <param name="name">The member name.</param>
        /// <param name="value">The value.</param>
        public IonMember(string name, object value)
        {
            this.Name = name;
            this.Value = value;
            this.SourceValue = value;
        }

        /// <summary>
        /// Returns a json string representation of the current Ion member.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <param name="nullValueHandling">Specified null value handling options for the JsonSerializer.</param>
        /// <returns></returns>
        public string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            if (pretty)
            {
                return $"{{\r\n  \"{Name}\": {Value?.ToJson(pretty, nullValueHandling)}\r\n}}";
            }
            return $"{{\"{Name}\": {Value?.ToJson(pretty, nullValueHandling)}}}";
        }
        
        /// <summary>
        /// Returns the value as the specified generic type `T`.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ValueAs<T>() where T : class
        {
            if (Value == null)
            {
                return default;
            }
            T typedValue = Value as T;
            if (typedValue == null)
            {
                if (Value is IJsonable jsonable)
                {
                    return JsonConvert.DeserializeObject<T>(jsonable.ToJson());
                }
            }

            return typedValue;
        }

        /// <summary>
        /// Returns the value as an `IonObject`.
        /// </summary>
        /// <returns>`IonObject`.</returns>
        public IonObject ValueObject()
        {
            if (Value == null)
            {
                return default;
            }

            if (Value is IJsonable jsonable)
            {
                return IonObject.ReadObject(jsonable.ToJson());
            }

            return IonObject.ReadObject(JsonConvert.SerializeObject(Value));
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public virtual object Value { get; set; }

        /// <summary>
        /// Gets or sets the source value.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public object SourceValue { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public IonObject Parent
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a value indicating wether this member is a child of its parent
        /// with the specified member name.
        /// </summary>
        /// <param name="memberNameToCheck"></param>
        /// <returns></returns>
        public bool IsMemberNamed(string memberNameToCheck)
        {
            if(Parent == null)
            {
                return false;
            }

            IonMember namedMember = Parent[memberNameToCheck];
            return namedMember.Equals(this);
        }

        /// <summary>
        /// Returns a string representation of the current member.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"\"{Name}\": {Value?.ToJson()}";
        }

        private static Dictionary<string, Func<object, IonObject>> _registeredMemberReaders = new Dictionary<string, Func<object, IonObject>>()
        {
            {"eform", (obj) =>
                {
                    string stringValue = obj?.ToString();
                    if (stringValue.IsJson())
                    {
                        return IonObject.ReadObject(stringValue);
                    }
                    return new IonObject { Value = obj };
                }
            }
        };

        /// <summary>
        /// Sets the property on the specified instance to the value of the current IonMember where the name matches the name of the current IonMember.
        /// </summary>
        /// <param name="instance"></param>
        public void SetProperty(object instance)
        {
            Type type = instance.GetType();
            Dictionary<string, PropertyInfo> propertyInfos = GetPropertyDictionary(type);
            if (propertyInfos.ContainsKey(Name))
            {
                propertyInfos[Name].SetValue(instance, Value);
            }
        }

        /// <summary>
        /// Returns a list of members that represent the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>`IEnumerable{IonMember}`.</returns>
        public static IEnumerable<IonMember> ListFromObject(object value)
        {
            if (value is string stringValue)
            {
                if (stringValue.IsJson())
                {
                    return ListFromJson(stringValue);
                }
            }
            if (value is IonMember ionMemberValue)
            {
                if (ionMemberValue.Value is JObject jobjectValue)
                {
                    return ListFromJson(jobjectValue.ToString());
                }
            }
            return ListFromJson(JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Returns a list of members that represent the specified json string.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>`IEnumerable{IonMember}`.</returns>
        public static IEnumerable<IonMember> ListFromJson(string json)
        {
            Dictionary<string, object> members = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            foreach(string key in members.Keys)
            {
                yield return new IonMember(key, members[key]);
            }
        }

        /// <summary>
        /// Returns a list of members that represent the specified dictionary.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <returns>`IEnumerable{IonMember}`.</returns>
        public static IEnumerable<IonMember> ListFromDictionary(Dictionary<string, object> members)
        {
            foreach(string key in members.Keys)
            {
                yield return new IonMember(key, members[key]);
            }
        }

        internal static Dictionary<string, PropertyInfo> GetPropertyDictionary(Type type)
        {
            Dictionary<string, PropertyInfo> results = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                string camelCase = propertyInfo.Name.CamelCase();
                string pascalCase = propertyInfo.Name.PascalCase();
                if (!results.ContainsKey(camelCase))
                {
                    results.Add(camelCase, propertyInfo);
                }
                if (!results.ContainsKey(pascalCase))
                {
                    results.Add(pascalCase, propertyInfo);
                }
            }
            return results;
        }

        /// <summary>
        /// Returns a value indicating if the current instance is equivalent to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>`bool`.</returns>
        public override bool Equals(object value)
        {
            if (value == null && Value == null)
            {
                return true;
            }
            if(value == null && Value != null)
            {
                return false;
            }
            if(value != null && Value == null)
            {
                return false;
            }
            if(value is IonMember ionMember)
            {
                return Value.Equals(ionMember.Value) && Name.Equals(ionMember.Name);
            }
            return Value.Equals(value);
        }

        /// <summary>
        /// Returns a value uniquely identifying this instance at runtime.
        /// </summary>
        /// <returns>`int`.</returns>
        public override int GetHashCode()
        {
            if(Value == null)
            {
                return -1;
            }
            return Value.GetHashCode() + Name.GetHashCode();
        }

        /// <summary>
        /// Get a list of IonMembers representing the specified instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IEnumerable<IonMember> ListFor(object instance)
        {
            return ListFor(instance, (propertyInfo) => true);
        }

        /// <summary>
        /// Get a list of IonMembers representing the specified instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyFilter"></param>
        /// <returns></returns>
        public static IEnumerable<IonMember> ListFor(object instance, Func<PropertyInfo, bool> propertyFilter)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (propertyFilter == null)
            {
                throw new ArgumentNullException(nameof(propertyFilter));
            }

            foreach (PropertyInfo propertyInfo in instance.GetType().GetProperties().Where(propertyFilter))
            {
                yield return new IonMember { Name = propertyInfo.Name, Value = propertyInfo.GetValue(instance) };
            }
        }
    }
}
