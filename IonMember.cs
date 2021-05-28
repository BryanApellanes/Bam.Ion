using Bam.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsQuery.StringScanner.Patterns;
using YamlDotNet.Serialization;
using Newtonsoft.Json.Linq;

namespace Bam.Ion
{
    public class IonMember<T> : IonMember
    {
        public static implicit operator T(IonMember<T> ionMember)
        {
            return ionMember.Value;
        }

        public static explicit operator IonMember<T>(T value)
        {
            return new IonMember<T>(value);
        }
        
        public IonMember() { }
        public IonMember(T value)
        {
            Value = value;
        }

        public new T Value { get; set; }

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
                return Value.Equals(ionMember.Value) && Name.Equals(ionMember.Name);
            }
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Value == null)
            {
                return -1;
            }
            return Value.GetHashCode() + Name.GetHashCode();
        }
    }

    public class IonMember 
    {
        static IonMember()
        {
        }

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
            if(value.TryFromJson<IonMember>(out IonMember result))
            {
                result.SourceValue = result.Value;
                return result;
            }
            return new IonMember { Name = "value", Value = value, SourceValue = value };
        }

        public IonMember(object value)
        {
            this.Name = "value";
            this.Value = value;
            this.SourceValue = value;
        }

        public IonMember(string name, object value)
        {
            this.Name = name;
            this.Value = value;
            this.SourceValue = value;
        }

        public string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            if (pretty)
            {
                return $"{{\r\n  \"{Name}\": {Value?.ToJson(pretty, nullValueHandling)}\r\n}}";
            }
            return $"{{\"{Name}\": {Value?.ToJson(pretty, nullValueHandling)}}}";
        }
        
        public T ValueAs<T>() where T : class
        {
            if(Value == null)
            {
                return default;
            }
            T typedValue = Value as T;
            if(typedValue == null)
            {
                if(Value is IJsonable jsonable)
                {
                    return JsonConvert.DeserializeObject<T>(jsonable.ToJson());
                }
            }

            return typedValue;
        }

        public IonObject ValueObject()
        {
            if(Value == null)
            {
                return default;
            }

            if(Value is IJsonable jsonable)
            {
                return IonObject.ReadObject(jsonable.ToJson());
            }

            return IonObject.ReadObject(JsonConvert.SerializeObject(Value));

        }

        public string Name { get; set; }

        public virtual object Value { get; set; }

        [YamlIgnore]
        [JsonIgnore]
        public object SourceValue { get; set; }

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

        public static IEnumerable<IonMember> ListFromObject(object value)
        {
            if(value is string stringValue)
            {
                if (stringValue.IsJson())
                {
                    return ListFromJson(stringValue);
                }
            }
            if(value is IonMember ionMemberValue)
            {
                if (ionMemberValue.Value is JObject jobjectValue)
                {
                    return ListFromJson(jobjectValue.ToString());
                }
            }
            return ListFromJson(JsonConvert.SerializeObject(value));
        }

        public static IEnumerable<IonMember> ListFromJson(string json)
        {
            Dictionary<string, object> members = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            foreach(string key in members.Keys)
            {
                yield return new IonMember(key, members[key]);
            }
        }

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

        public override bool Equals(object obj)
        {
            if (obj == null && Value == null)
            {
                return true;
            }
            if(obj == null && Value != null)
            {
                return false;
            }
            if(obj != null && Value == null)
            {
                return false;
            }
            if(obj is IonMember ionMember)
            {
                return Value.Equals(ionMember.Value) && Name.Equals(ionMember.Name);
            }
            return Value.Equals(obj);
        }

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
            Args.ThrowIfNull(instance);
            Args.ThrowIfNull(propertyFilter);

            foreach (PropertyInfo propertyInfo in instance.GetType().GetProperties().Where(propertyFilter))
            {
                yield return new IonMember { Name = propertyInfo.Name, Value = propertyInfo.GetValue(instance) };
            }
        }
    }
}
