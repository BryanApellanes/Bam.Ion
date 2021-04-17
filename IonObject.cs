using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net;
using System.Collections;
using System.Reflection;

namespace Bam.Ion
{
    /// <summary>
    /// Ion value whose value property is of the specified generic type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IonObject<T> : IonObject
    {
        public IonObject() { }
        public IonObject(List<IonMember> members) : base(members)
        { }

        public new T Value { get; set; }

        public T ToInstance()
        {
            ConstructorInfo ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new InvalidOperationException($"The specified type ({typeof(T).AssemblyQualifiedName}) does not have a parameterless constructor.");
            }
            T instance = (T)ctor.Invoke(null);
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (IonMember ionMember in this)
            {
                ionMember.SetProperty(instance);
            }
            return instance;
        }
    }

    public class IonObject : IonType, IJsonable, IEnumerable<IonMember>
    {
        public static implicit operator IonObject(string value)
        {
            return new IonObject { Value = value };
        }
        
        public static implicit operator string(IonObject value)
        {
            return value.ToJson();
        }

        private List<IonMember> _memberList;
        private Dictionary<string, IonMember> _memberDictionary;

        public IonObject()
        {
            this._memberList = new List<IonMember>();
            this.ContextData = new Dictionary<string, object>();
        }

        public IonObject(List<IonMember> members)
        {
            this._memberList = members;
            this.ContextData = new Dictionary<string, object>();
            this.SetMemberDictionary();
        }

        public IonObject(List<IonMember> ionMembers, Dictionary<string, object> contextData) : this()
        {
            this._memberList = ionMembers;
            this.ContextData = contextData;
            this.SetMemberDictionary();
        }

        public override bool Equals(object obj)
        {
            if(obj == null && Value == null)
            {
                return true;
            }
            if(obj != null && Value == null)
            {
                return false;
            }
            if(obj != null && Value != null)
            {
                return Value.Equals(obj);
            }
            return false;
        }

        public List<IonMember> Members
        {
            get
            {
                return _memberList;
            }
        }

        public Dictionary<string, object> ContextData
        {
            get;
            set;
        }

        public IonMember this[string name]
        {
            get
            {
                string pascalCase = name.PascalCase();
                if (_memberDictionary.ContainsKey(pascalCase))
                {
                    return _memberDictionary[pascalCase];
                }
                string camelCase = name.CamelCase();
                if (_memberDictionary.ContainsKey(camelCase))
                {
                    return _memberDictionary[camelCase];
                }
                return null;
            }
        }

        public object Value { get; set; }

        public IonObject SetContextData(string name, object data)
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

        public IonObject SetTypeContext<T>()
        {
            return SetTypeContext(typeof(T));
        }

        public IonObject SetTypeContext(Type type)
        {
            return SetContextData("type", type.Name);
        }

        public IonObject SetTypeFullNameContext<T>()
        {
            return SetTypeFullNameContext(typeof(T));
        }

        public IonObject SetTypeFullNameContext(Type type)
        {
            return SetContextData("fullName", type.FullName);
        }

        public IonObject SetTypeAssemblyQualifiedNameContext<T>()
        {
            return SetTypeAssemblyQualifiedNameContext(typeof(T));
        }

        public IonObject SetTypeAssemblyQualifiedNameContext(Type type)
        {
            return SetContextData("assemblyQualifiedName", type.AssemblyQualifiedName);
        }

        public string ToJson()
        {
            return ToJson(false);
        }

        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if(Value != null)
            {
                data.Add("value", Value);
            }
            foreach(IonMember member in _memberList)
            {
                data.Add(member.Name, member.Value);
            }

            foreach (string key in ContextData?.Keys)
            {
                data.Add(key, ContextData[key]);
            }
            return data.ToJson(pretty, nullValueHandling);
        }

        public static IonObject Read(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<IonMember> members = new List<IonMember>();
            foreach(System.Collections.Generic.KeyValuePair<string, object> keyValuePair in dictionary)
            {
                members.Add(keyValuePair);
            }
            return new IonObject(members);
        }

        public static IonObject<T> Read<T>(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            Dictionary<string, PropertyInfo> properties = IonMember.GetPropertyDictionary(typeof(T));
            List<IonMember> members = new List<IonMember>();

            IonObject<T> result = new IonObject<T>(members);
            foreach (System.Collections.Generic.KeyValuePair<string, object> keyValuePair in dictionary)
            {
                if (properties.ContainsKey(keyValuePair.Key))
                {
                    members.Add(keyValuePair);
                }
                else
                {
                    result.SetContextData(keyValuePair.Key, keyValuePair.Value);
                }              
            }
            result.SetMemberDictionary();
            result.Value = result.ToInstance();
            return result;
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
        private void SetMemberDictionary()
        {
            _memberDictionary = new Dictionary<string, IonMember>();
            foreach (IonMember ionMember in _memberList)
            {
                _memberDictionary.Add(ionMember.Name, ionMember);
                _memberDictionary.Add(ionMember.Name.PascalCase(), ionMember);
            }
        }

        public IEnumerator<IonMember> GetEnumerator()
        {
            return _memberList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _memberList.GetEnumerator();
        }
    }
}
