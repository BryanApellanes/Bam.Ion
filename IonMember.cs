using Bam.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsQuery.StringScanner.Patterns;

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
            return new IonMember { Name = keyValuePair.Key, Value = keyValuePair.Value };
        }

        public static implicit operator string(IonMember ionMember)
        {
            return ionMember?.ToJson() ?? "null";
        }

        public static implicit operator IonMember(string value)
        {
            if(value.TryFromJson<IonMember>(out IonMember result))
            {
                return result;
            }
            return new IonMember { Name = "value", Value = value };
        }

        public IonMember(object value)
        {
            this.Name = "value";
            this.Value = value;
        }

        public IonMember(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            if (pretty)
            {
                return $"{{\r\n  \"{Name}\": {Value?.ToJson(pretty, nullValueHandling)}\r\n}}";
            }
            return $"{{\"{Name}\": {Value?.ToJson(pretty, nullValueHandling)}}}";
        }
        
        public string Name { get; set; }

        public object Value { get; set; }

        public override string ToString()
        {
            return $"\"{Name}\": {Value?.ToJson()}";
        }

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

        public static IEnumerable<IonMember> GetMemberList(object instance)
        {
            return GetMemberList(instance, (propertyInfo) => true);
        }

        public static IEnumerable<IonMember> GetMemberList(object instance, Func<PropertyInfo, bool> propertyFilter)
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
