using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net;

namespace Bam.Ion
{
    /// <summary>
    /// Ion value whose value property is of the specified generic type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IonValue<T> : IonValue
    {
        public IonValue()
        {
            ContextData = new Dictionary<string, object>();
        }

        
        public new IonMember<T> Value { get; set; }
    }

    public class IonValue : IonType
    {
        public static implicit operator IonValue(string value)
        {
            return new IonValue { Value = value };
        }
        
        public static implicit operator string(IonValue value)
        {
            return value.Value;
        }

        public IonValue()
        {
            this.ContextData = new Dictionary<string, object>();
        }

        public Dictionary<string, object> ContextData
        {
            get;
            set;
        }

        public IonValue SetContextData(string name, object data)
        {
            if(ContextData == null)
            {
                ContextData = new Dictionary<string, object>();
            }

            if (ContextData.ContainsKey(name))
            {
                ContextData[name] = data;
            }
            else
            {
                ContextData.Add(name, data);
            }
            return this;
        }

        public IonValue SetTypeContext<T>()
        {
            return SetTypeContext(typeof(T));
        }

        public IonValue SetTypeContext(Type type)
        {
            return SetContextData("type", type.Name);
        }

        public IonValue SetTypeFullNameContext<T>()
        {
            return SetTypeFullNameContext(typeof(T));
        }

        public IonValue SetTypeFullNameContext(Type type)
        {
            return SetContextData("fullName", type.FullName);
        }

        public IonValue SetTypeAssemblyQualifiedNameContext<T>()
        {
            return SetTypeAssemblyQualifiedNameContext(typeof(T));
        }

        public IonValue SetTypeAssemblyQualifiedNameContext(Type type)
        {
            return SetContextData("assemblyQualifiedName", type.AssemblyQualifiedName);
        }

        public IonMember Value { get; set; }

        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add(this.Value.Name, this.Value.Value);
            foreach (string key in ContextData?.Keys)
            {
                data.Add(key, ContextData[key]);
            }
            return data.ToJson(pretty, nullValueHandling);
        }

        public static IonCollection Read(string json)
        {
            return IonCollection.Read(json);
        }

        public static IonCollection<T> Read<T>(string json)
        {
            return IonCollection<T>.Read(json);
        }
        
        protected override void SetTypeContext()
        {
            switch (TypeContextKind)
            {
                case TypeContextKind.Invalid:
                case TypeContextKind.TypeName:
                    SetTypeContext(Type);
                    break;
                case TypeContextKind.FullName:
                    SetTypeFullNameContext(Type);
                    break;
                case TypeContextKind.AssemblyQualifiedName:
                    SetTypeAssemblyQualifiedNameContext(Type);
                    break;
            }
        }
    }
}
