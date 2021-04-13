using Bam.Net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonCollection<T> : IonCollection
    {
        public T ToInstance()
        {
            throw new NotImplementedException();
        }
    }
    
    public class IonCollection : IonValue, IEnumerable
    {
        private readonly List<IonMember> _innerList;

        public IonCollection()
        {
            _innerList = new List<IonMember>();
            Value = _innerList;
        }

        public new List<IonMember> Value { get; set; }

        public virtual IEnumerator GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public virtual void Add(object value)
        {
            if (value is IonMember ionMember)
            {
                AddIonMember(ionMember);
            }
            else
            {
                foreach (IonMember member in IonMember.GetMemberList(value))
                {
                    AddIonMember(member);
                }
            }
        }

        public virtual void AddIonMember(IonMember ionMember)
        {
            _innerList.Add(ionMember);
        }
        
        public virtual bool Contains(object value)
        {
            return _innerList.Contains(value);
        }

        public static IonCollection Read(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<IonMember> members = new List<IonMember>();
            foreach(System.Collections.Generic.KeyValuePair<string, object> keyValuePair in dictionary)
            {
                members.Add(keyValuePair);
            }
            return new IonCollection { Value = members };
        }
        
        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach(IonMember member in Value)
            {
                data.Add(member.Name, member.Value);
            }
            foreach (string key in ContextData?.Keys)
            {
                data.Add(key, ContextData[key]);
            }
            return data.ToJson(pretty, nullValueHandling);
        }
    }
}
