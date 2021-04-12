using Bam.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonMember<T> : IonMember
    {
        public static implicit operator T(IonMember<T> ionMember)
        {
            return ionMember.Value;
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
            if (value.Trim().StartsWith("{"))
            {
                return value.FromJson<IonMember>();
            }
            return new IonMember { Name = "value", Value = value };
        }

        public IonMember(object value)
        {
            this.Name = "Value";
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
                return $"{{\r\n  \"{Name}\": {Value.ToJson(pretty, nullValueHandling)}\r\n}}";
            }
            return $"{{\"{Name}\": {Value.ToJson(pretty, nullValueHandling)}}}";
        }

        public string Name { get; set; }

        public object Value { get; set; }

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
    }
}
