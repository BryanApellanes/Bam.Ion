using Bam.Net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Bam.Ion
{    
    public class IonCollection : IJsonable, IEnumerable, IEnumerable<IonObject>
    {
        private List<IonObject> _innerList;

        public IonCollection()
        {
            _innerList = new List<IonObject>();
            Value = _innerList;
        }

        public new List<IonObject> Value
        {
            get => _innerList;
            set
            {
                _innerList = value;
            }
        }

        IEnumerator<IonObject> IEnumerable<IonObject>.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public virtual IEnumerator GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public virtual void Add(IonObject ionMember)
        {
            _innerList.Add(ionMember);
        }
        
        public virtual bool Contains(object value)
        {
            return _innerList.Contains(value);
        }


        [YamlIgnore]
        [JsonIgnore]
        public int Count => _innerList.Count;

        [YamlIgnore]
        [JsonIgnore]
        public IonObject this[int index]
        {
            get
            {
                return _innerList[index];
            }
        }

        public string ToJson()
        {
            return ToJson(false);
        }

        public string ToJson(bool pretty, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            return _innerList.ToJson(pretty, nullValueHandling);
        }
        
        protected void RemoveObject(IonObject ionObject)
        {
            if (_innerList.Contains(ionObject))
            {
                _innerList.Remove(ionObject);
            }
        }
    }
}
