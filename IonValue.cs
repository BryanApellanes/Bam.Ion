using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net;

namespace Bam.Ion
{
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

        public IonValue AddContext(string name, object data)
        {
            if(ContextData == null)
            {
                ContextData = new Dictionary<string, object>();
            }
            ContextData.Add(name, data);
            return this;
        }

        protected IonValue AddTypeContext(Type type)
        {
            return AddContext("type", type.Name);
        }

        protected IonValue AddTypeFullNameContext(Type type)
        {
            return AddContext("fullName", type.FullName);
        }

        protected IonValue AddTypeAssemblyQaulifiedNameContext(Type type)
        {
            return AddContext("assemblyQualifiedName", type.AssemblyQualifiedName);
        }

        public IonMember Value { get; set; }

        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = this.ToDictionary(pi => pi.Name.Equals("Value"));
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

        protected override void SetTypeContext()
        {
            throw new NotImplementedException();
        }
    }
}
