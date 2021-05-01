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
    public class IonCollection : IonValueObject, IJsonable, IEnumerable, IEnumerable<IonValueObject>
    {
        private List<IonValueObject> _innerList;

        public IonCollection()
        {
            _innerList = new List<IonValueObject>();
            Value = _innerList;
        }

        public new List<IonValueObject> Value
        {
            get => _innerList;
            set
            {
                _innerList = value;
            }
        }

        IEnumerator<IonValueObject> IEnumerable<IonValueObject>.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public virtual IEnumerator GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public virtual void Add(IonValueObject ionValueObject)
        {
            _innerList.Add(ionValueObject);
        }
        
        public virtual void Add<T>(string json)
        {
            this.Add<T>(new IonValueObject<T>(json));
        }

        public virtual void Add<T>(IonValueObject<T> ionValueObject)
        {
            _innerList.Add(ionValueObject);
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
        public IonValueObject this[int index]
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
            IonMember ionMember = new IonMember(_innerList);
            return ionMember.ToJson(pretty, nullValueHandling);
        }
        
        protected void RemoveObject(IonValueObject ionObject)
        {
            if (_innerList.Contains(ionObject))
            {
                _innerList.Remove(ionObject);
            }
        }
    }
}
